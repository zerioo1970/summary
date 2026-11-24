// 账户迁移工具/src/ProfileCopy/WeChatFinder.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ProfileCopy
{
    /// <summary>找到的一个微信 / 企业微信数据文件夹，或一条设置记录。</summary>
    internal sealed class WeChatHit
    {
        public string Software;     // 微信 3.x / 微信 4.x / 企业微信 / 微信 3.x 设置记录
        public string Path;         // 文件夹或设置文件的完整路径
        public string Accounts;     // 里面的账号子文件夹，例如 wxid_abc、wxid_def
        public DateTime? Modified;  // 最后修改时间，用来判断哪个是最近在用的
        public string Note;         // 说明：会不会随“文档”复制、该怎么处理
        public bool IsFolder;       // false 表示这是一条设置记录，不是数据文件夹
    }

    /// <summary>
    /// 在所有本地硬盘上按文件夹名查找微信和企业微信的数据。
    /// 位置可以由用户改到任意地方，但文件夹名是固定的，所以按名字搜最可靠。
    /// </summary>
    internal static class WeChatFinder
    {
        // 文件夹名 → 软件名
        private static readonly Dictionary<string, string> Targets =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "WeChat Files", "微信 3.x" },
                { "xwechat_files", "微信 4.x" },
                { "WXWork", "企业微信" },
            };

        // 这些文件夹不进去搜：
        // Windows、回收站、系统还原 —— 不会放聊天数据，而且很大；
        // AppData —— 里面的 Tencent\WXWork 等是程序的设置和缓存，不是聊天文件，会被误认成数据文件夹
        private static readonly HashSet<string> SkipNames =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Windows", "$Recycle.Bin", "System Volume Information", "AppData", "$WinREAgent", "Recovery"
            };

        /// <summary>
        /// 开始查找。progress 报告当前正在看的文件夹（会很频繁），found 每找到一个就调用一次。
        /// 两个回调都在后台线程上执行。
        /// </summary>
        public static void Search(List<ProfileInfo> profiles, Action<string> progress, Action<WeChatHit> found,
                                  CancellationToken token)
        {
            // 先读各账户的微信 3.x 设置记录，很快，先给结果
            foreach (WeChatHit h in ReadWeChat3Settings(profiles))
            {
                if (token.IsCancellationRequested) return;
                found(h);
            }

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                // 只搜本地硬盘；U 盘、光驱、网络驱动器不搜
                bool ok;
                try { ok = drive.DriveType == DriveType.Fixed && drive.IsReady; }
                catch (Exception) { ok = false; }
                if (!ok) continue;

                var stack = new Stack<string>();
                stack.Push(drive.RootDirectory.FullName);
                while (stack.Count > 0)
                {
                    if (token.IsCancellationRequested) return;
                    string dir = stack.Pop();
                    progress(dir);

                    IEnumerable<DirectoryInfo> subs;
                    try { subs = new DirectoryInfo(dir).EnumerateDirectories().ToList(); }
                    catch (Exception) { continue; }   // 没权限或路径太长，跳过这一个

                    foreach (DirectoryInfo sub in subs)
                    {
                        // 跳过链接文件夹（例如“我的文档”“Application Data”），否则会重复找到同一个位置，甚至死循环
                        if ((sub.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                        if (SkipNames.Contains(sub.Name)) continue;

                        string software;
                        if (Targets.TryGetValue(sub.Name, out software))
                        {
                            WeChatHit hit = Inspect(sub, software, profiles);
                            if (hit != null) found(hit);
                            continue;   // 找到了就不再进去搜
                        }
                        stack.Push(sub.FullName);
                    }
                }
            }
        }

        /// <summary>看看找到的文件夹是不是真的数据文件夹，并写好说明。不是数据文件夹返回 null。</summary>
        private static WeChatHit Inspect(DirectoryInfo dir, string software, List<ProfileInfo> profiles)
        {
            // 企业微信默认装在 C:\Program Files (x86)\WXWork，这个安装文件夹也叫 WXWork。
            // 里面直接有 .exe 的就是安装文件夹，不是数据，不显示
            try
            {
                if (dir.EnumerateFiles("*.exe").Any()) return null;
            }
            catch (Exception) { }

            string accounts = "";
            try
            {
                // 每个登录过的账号一个子文件夹（微信是 wxid_… 或自定义微信号，企业微信是一串数字）。
                // All Users、Applet 等是公共文件夹，不算账号
                string[] common = { "All Users", "Applet", "WMPF", "Global", "Plugins", "Temp", "Log", "logs", "Cache" };
                List<string> names = dir.EnumerateDirectories()
                    .Where(d => (d.Attributes & FileAttributes.ReparsePoint) == 0)
                    .Select(d => d.Name)
                    .Where(n => !common.Contains(n, StringComparer.OrdinalIgnoreCase))
                    .ToList();
                accounts = names.Count == 0 ? "（没有账号文件夹）"
                         : string.Join("、", names.Take(5)) + (names.Count > 5 ? " 等 " + names.Count + " 个" : "");
            }
            catch (Exception) { accounts = "（读不了）"; }

            DateTime? modified = null;
            try { modified = dir.LastWriteTime; } catch (Exception) { }

            return new WeChatHit
            {
                Software = software,
                Path = dir.FullName,
                Accounts = accounts,
                Modified = modified,
                Note = Describe(dir.FullName, profiles),
                IsFolder = true
            };
        }

        /// <summary>按文件夹所在位置说明迁移办法。</summary>
        private static string Describe(string path, List<ProfileInfo> profiles)
        {
            foreach (ProfileInfo p in profiles)
            {
                string root = p.Path.TrimEnd('\\') + "\\";
                if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) continue;

                if (path.StartsWith(root + "Documents\\", StringComparison.OrdinalIgnoreCase))
                    return "在 " + p.Account + " 的“文档”里：在“② 复制文件”页复制这个账户的“文档”时会一起复制";
                return "在 " + p.Account + " 的配置文件夹里（不在“文档”中）：不会自动复制，请在新账户的软件里把存储位置改到这里";
            }
            return "不在任何账户的配置文件夹里：不用复制。新账户打开软件后，在设置里把存储位置改到这里（没权限时先点“给新账户授权”）";
        }

        /// <summary>
        /// 微信 3.x 把存储位置写在每个账户的
        /// AppData\Roaming\Tencent\WeChat\All Users\config\3ebffe94.ini 里：
        /// 内容是 MyDocument: 表示默认位置（“文档”），否则是一个路径（微信会用这个路径下的 WeChat Files）。
        /// </summary>
        private static IEnumerable<WeChatHit> ReadWeChat3Settings(List<ProfileInfo> profiles)
        {
            foreach (ProfileInfo p in profiles)
            {
                string ini = System.IO.Path.Combine(p.Path, @"AppData\Roaming\Tencent\WeChat\All Users\config\3ebffe94.ini");
                string text;
                DateTime? modified;
                try
                {
                    if (!File.Exists(ini)) continue;
                    text = ReadText(ini).Trim();
                    modified = File.GetLastWriteTime(ini);
                }
                catch (Exception) { continue; }

                string note;
                if (text.Length == 0 || text.StartsWith("MyDocument", StringComparison.OrdinalIgnoreCase))
                    note = "默认位置：" + System.IO.Path.Combine(p.Path, @"Documents\WeChat Files");
                else
                    note = "改过位置：" + System.IO.Path.Combine(text, "WeChat Files");

                yield return new WeChatHit
                {
                    Software = "微信 3.x 设置记录",
                    Path = ini,
                    Accounts = p.Account,
                    Modified = modified,
                    Note = note + "（文件内容：" + text + "）",
                    IsFolder = false
                };
            }
        }

        /// <summary>先按 UTF-8 读，不是合法的 UTF-8 再按系统默认编码（中文 Windows 是 GBK）读。</summary>
        private static string ReadText(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            try { return new UTF8Encoding(false, true).GetString(bytes).TrimStart('\uFEFF'); }
            catch (DecoderFallbackException) { return Encoding.Default.GetString(bytes); }
        }
    }
}
