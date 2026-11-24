// 账户迁移工具/src/ProfileCopy/CopyRunner.cs
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace ProfileCopy
{
    /// <summary>
    /// 调用 Windows 自带的 robocopy 做实际复制。
    /// 不自己写 File.Copy 的原因：robocopy 能处理超长路径、只读和隐藏文件、被拒绝访问时改用备份模式读取，
    /// 中途失败会重试，结果有标准的退出码，这些自己写很容易漏。
    /// </summary>
    internal sealed class CopyRunner
    {
        /// <summary>robocopy 每输出一行就触发一次（在后台线程上）。</summary>
        public event Action<string> Line;

        private readonly object gate = new object();
        private Process current;
        private volatile bool cancelled;

        public bool Cancelled { get { return cancelled; } }

        private static string RobocopyPath
        {
            get { return Path.Combine(Environment.SystemDirectory, "robocopy.exe"); }
        }

        /// <summary>运行一次 robocopy，返回退出码；没能启动或被停止返回 -1。</summary>
        public int Run(string arguments)
        {
            // 中文 Windows 上 robocopy 输出到管道时用 OEM 代码页（936），按它解码中文文件名才不乱码
            Encoding oem = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
            var psi = new ProcessStartInfo(RobocopyPath, arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = oem,
                StandardErrorEncoding = oem
            };

            using (var p = new Process { StartInfo = psi })
            {
                p.OutputDataReceived += OnData;
                p.ErrorDataReceived += OnData;

                lock (gate)
                {
                    if (cancelled) return -1;
                    try { p.Start(); }
                    catch (Exception ex)
                    {
                        Raise("无法启动 robocopy：" + ex.Message);
                        return -1;
                    }
                    current = p;
                }

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();

                lock (gate) { current = null; }
                return cancelled ? -1 : p.ExitCode;
            }
        }

        /// <summary>停止：结束正在运行的 robocopy，后面的项目也不再开始。</summary>
        public void Cancel()
        {
            cancelled = true;
            lock (gate)
            {
                try
                {
                    if (current != null && !current.HasExited) current.Kill();
                }
                catch (Exception)
                {
                    // 进程恰好刚结束，忽略
                }
            }
        }

        private void OnData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null) Raise(e.Data);
        }

        private void Raise(string text)
        {
            Action<string> h = Line;
            if (h != null) h(text);
        }

        /// <summary>
        /// 把 robocopy 退出码翻译成中文。退出码是按位组合的：
        /// 1 = 复制了文件，2 = 目标里有源里没有的文件，4 = 有不一致的文件，8 及以上 = 有复制失败。
        /// </summary>
        public static string Describe(int code)
        {
            if (code < 0) return "已停止";
            if (code == 0) return "完成（没有需要复制的）";
            if (code < 8) return "完成";
            return "有失败，见日志";
        }

        /// <summary>给路径加引号。末尾的 \ 要去掉，否则 \" 会被 robocopy 当成转义的引号。</summary>
        public static string Quote(string path)
        {
            return "\"" + path.TrimEnd('\\') + "\"";
        }
    }
}
