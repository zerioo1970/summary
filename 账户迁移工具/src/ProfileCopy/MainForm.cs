// 账户迁移工具/src/ProfileCopy/MainForm.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProfileCopy
{
    internal sealed class MainForm : Form
    {
        private const string Title = "账户文件迁移工具";

        // ---------- 界面控件 ----------
        private readonly ComboBox cboSource = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        private readonly ComboBox cboTarget = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        private readonly Button btnBrowse = new Button { Text = "其他文件夹…", AutoSize = true };
        private readonly Button btnReload = new Button { Text = "刷新列表", AutoSize = true };
        private readonly ListView lvItems = new ListView
        {
            View = View.Details,
            CheckBoxes = true,
            FullRowSelect = true,
            GridLines = true,
            Dock = DockStyle.Fill
        };
        private readonly RadioButton rbSkip = new RadioButton { Text = "跳过：新账户里已有的同名文件不动（推荐）", AutoSize = true, Checked = true };
        private readonly RadioButton rbNewer = new RadioButton { Text = "覆盖：旧账户里的文件较新时，替换新账户里的", AutoSize = true };
        private readonly Label lblStatus = new Label { Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
        private readonly Button btnStart = new Button { Text = "开始复制", AutoSize = true };
        private readonly Button btnStop = new Button { Text = "停止", AutoSize = true, Enabled = false };
        private readonly Button btnLog = new Button { Text = "打开日志", AutoSize = true, Enabled = false };
        private readonly Timer uiTimer = new Timer { Interval = 200 };

        // ---------- 运行状态 ----------
        private CopyRunner runner;
        private string logPath;
        private bool running;
        private bool closeRequested;                // 复制中点了关闭，停止后自动退出
        private volatile string currentStep = "";   // “第 2/7 项：文档”
        private volatile string latestLine = "";    // robocopy 最近输出的一行，由定时器刷到界面上
        private volatile int sizeVersion;           // 换了旧账户后，上一次“计算大小”的结果作废

        public MainForm()
        {
            Text = Title;
            Font = new Font("Microsoft YaHei UI", 9F);
            MinimumSize = new Size(760, 520);
            Size = new Size(980, 660);
            StartPosition = FormStartPosition.CenterScreen;

            SuspendLayout();
            BuildLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ResumeLayout(false);
            PerformLayout();

            cboSource.SelectedIndexChanged += delegate { RefreshSizes(); };
            btnBrowse.Click += OnBrowse;
            btnReload.Click += delegate { LoadProfiles(); };
            btnStart.Click += OnStart;
            btnStop.Click += OnStop;
            btnLog.Click += OnOpenLog;
            uiTimer.Tick += delegate { lblStatus.Text = currentStep + "    " + latestLine.Trim(); };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // 列宽按屏幕缩放比例放大，150% 缩放时不会挤在一起
            float k = DeviceDpi / 96F;
            lvItems.Columns.Add("项目", (int)(170 * k));
            lvItems.Columns.Add("在旧账户里的位置", (int)(400 * k));
            lvItems.Columns.Add("大小", (int)(100 * k));
            lvItems.Columns.Add("结果", (int)(180 * k));
            FillItems();
            LoadProfiles();
        }

        // ================= 界面布局 =================

        private void BuildLayout()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(10) };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // 账户选择
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // 项目列表
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // 选项
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));  // 状态行
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // 按钮

            // 第 1 行：两个下拉框
            var accounts = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, AutoSize = true };
            accounts.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            accounts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            accounts.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            accounts.Controls.Add(MakeLabel("旧账户（从这里复制）："), 0, 0);
            accounts.Controls.Add(cboSource, 1, 0);
            accounts.Controls.Add(btnBrowse, 2, 0);
            accounts.Controls.Add(MakeLabel("新账户（复制到这里）："), 0, 1);
            accounts.Controls.Add(cboTarget, 1, 1);
            accounts.Controls.Add(btnReload, 2, 1);
            root.Controls.Add(accounts, 0, 0);

            // 第 2 行：可勾选的项目
            root.Controls.Add(lvItems, 0, 1);

            // 第 3 行：同名文件怎么处理
            var group = new GroupBox { Text = "新账户里已有同名文件时", Dock = DockStyle.Fill, AutoSize = true };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false };
            flow.Controls.Add(rbSkip);
            flow.Controls.Add(rbNewer);
            flow.Controls.Add(new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Text = "说明：书签、Word 模板、便笺这类“单个文件”总是覆盖，覆盖前会把新账户里原来的文件改名为 .bak-时间 备份。"
            });
            group.Controls.Add(flow);
            root.Controls.Add(group, 0, 2);

            // 第 4 行：状态
            root.Controls.Add(lblStatus, 0, 3);

            // 第 5 行：按钮靠右
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
            buttons.Controls.Add(btnLog);
            buttons.Controls.Add(btnStop);
            buttons.Controls.Add(btnStart);
            root.Controls.Add(buttons, 0, 4);

            // 两个标签页：先建账号，新账号登录一次以后再复制文件
            var tabs = new TabControl { Dock = DockStyle.Fill };
            var pageAccount = new TabPage("① 建立账号");
            pageAccount.Controls.Add(new AccountPanel());
            var pageCopy = new TabPage("② 复制文件");
            pageCopy.Controls.Add(root);
            tabs.TabPages.Add(pageAccount);
            tabs.TabPages.Add(pageCopy);
            var pageWeChat = new TabPage("③ 查找微信数据");
            pageWeChat.Controls.Add(new WeChatPanel());
            tabs.TabPages.Add(pageWeChat);
            Controls.Add(tabs);
        }

        private static Label MakeLabel(string text)
        {
            return new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) };
        }

        private void FillItems()
        {
            var gFolders = new ListViewGroup("文件夹（整个复制）");
            var gFiles = new ListViewGroup("单个文件（覆盖前先备份）");
            lvItems.Groups.Add(gFolders);
            lvItems.Groups.Add(gFiles);

            foreach (CopyItem it in CopyItems.All)
            {
                var li = new ListViewItem(it.Name)
                {
                    Checked = it.CheckedByDefault,
                    Tag = it,
                    Group = it.IsFolder ? gFolders : gFiles
                };
                li.SubItems.Add(it.DisplayPath);
                li.SubItems.Add("");
                li.SubItems.Add("");
                lvItems.Items.Add(li);
            }
        }

        // ================= 账户列表 =================

        private void LoadProfiles()
        {
            List<ProfileInfo> profiles = Profiles.LoadAll();
            cboSource.Items.Clear();
            cboTarget.Items.Clear();
            foreach (ProfileInfo p in profiles)
            {
                cboSource.Items.Add(p);
                cboTarget.Items.Add(p);
            }

            // 目标默认选“现在坐在电脑前登录的账户”
            string account;
            string sid = SessionUser.GetSid(out account);
            ProfileInfo me = profiles.FirstOrDefault(p => p.Sid != null && p.Sid == sid);
            if (me != null)
            {
                cboTarget.SelectedItem = me;
                lblStatus.Text = "已自动选中当前登录的账户 " + account + " 作为新账户。请选择旧账户。";
            }
            else
            {
                lblStatus.Text = "没找到当前登录账户（" + account + "）的配置文件，请手动选择新账户。";
            }
            foreach (ListViewItem li in lvItems.Items) li.SubItems[2].Text = "";
        }

        private void OnBrowse(object sender, EventArgs e)
        {
            // 旧账户已从列表里消失、只剩文件夹时，用这个按钮直接选文件夹
            using (var dlg = new FolderBrowserDialog
            {
                Description = "选择旧账户的配置文件夹（例如 C:\\Users\\B）",
                SelectedPath = Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                var p = new ProfileInfo { Sid = null, Path = dlg.SelectedPath, Account = "（手动选择的文件夹）" };
                cboSource.Items.Add(p);
                cboSource.SelectedItem = p;
            }
        }

        // ================= 计算大小 =================

        private void RefreshSizes()
        {
            var src = cboSource.SelectedItem as ProfileInfo;
            if (src == null) return;

            int version = ++sizeVersion;
            var jobs = new List<KeyValuePair<ListViewItem, CopyItem>>();
            foreach (ListViewItem li in lvItems.Items)
            {
                li.SubItems[2].Text = "计算中…";
                li.SubItems[3].Text = "";
                jobs.Add(new KeyValuePair<ListViewItem, CopyItem>(li, (CopyItem)li.Tag));
            }

            // 大文件夹算大小要几秒，放到后台，界面不卡
            Task.Run(() =>
            {
                foreach (var job in jobs)
                {
                    if (version != sizeVersion) return;
                    string text = SizeText(src.Path, job.Value);
                    ListViewItem li = job.Key;
                    if (IsDisposed) return;   // 窗口已关闭
                    BeginInvoke((Action)(() =>
                    {
                        if (version == sizeVersion) li.SubItems[2].Text = text;
                    }));
                }
            });
        }

        private static string SizeText(string profilePath, CopyItem item)
        {
            string dir = Path.Combine(profilePath, item.RelativeDir);
            if (item.IsFolder)
            {
                if (!Directory.Exists(dir)) return "没有";
                bool partial;
                long bytes = DirectorySize(dir, out partial);
                // 有子文件夹读不了时只能算出一部分，前面加 ≥ 提示实际更大
                return (partial ? "≥ " : "") + FormatBytes(bytes);
            }

            long total = 0;
            bool any = false;
            foreach (string f in item.Files)
            {
                try
                {
                    var fi = new FileInfo(Path.Combine(dir, f));
                    if (fi.Exists) { total += fi.Length; any = true; }
                }
                catch (Exception) { }
            }
            return any ? FormatBytes(total) : "没有";
        }

        internal static long DirectorySize(string root, out bool partial)
        {
            partial = false;
            long total = 0;
            var stack = new Stack<string>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                string dir = stack.Pop();
                try
                {
                    foreach (FileSystemInfo fsi in new DirectoryInfo(dir).EnumerateFileSystemInfos())
                    {
                        // 跳过“我的音乐”这类链接文件夹，它们指向别处，算进去会重复甚至死循环
                        if ((fsi.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                        var sub = fsi as DirectoryInfo;
                        if (sub != null) stack.Push(sub.FullName);
                        else total += ((FileInfo)fsi).Length;
                    }
                }
                catch (Exception)
                {
                    partial = true;   // 没权限或路径太长，robocopy 复制时能处理，这里只是少算
                }
            }
            return total;
        }

        internal static string FormatBytes(long b)
        {
            if (b < 1024L * 1024) return (b / 1024.0).ToString("0") + " KB";
            if (b < 1024L * 1024 * 1024) return (b / 1024.0 / 1024).ToString("0.0") + " MB";
            return (b / 1024.0 / 1024 / 1024).ToString("0.00") + " GB";
        }

        // ================= 开始复制 =================

        private async void OnStart(object sender, EventArgs e)
        {
            var src = cboSource.SelectedItem as ProfileInfo;
            var dst = cboTarget.SelectedItem as ProfileInfo;

            if (src == null) { Warn("请先选择旧账户。"); return; }
            if (dst == null) { Warn("请先选择新账户。"); return; }
            if (string.Equals(src.Path.TrimEnd('\\'), dst.Path.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
            {
                Warn("旧账户和新账户是同一个文件夹，请重新选择。");
                return;
            }
            if (!Directory.Exists(dst.Path))
            {
                Warn("新账户的文件夹 " + dst.Path + " 不存在。\n请先用新账户登录一次，让 Windows 生成它。");
                return;
            }

            List<ListViewItem> chosen = lvItems.CheckedItems.Cast<ListViewItem>().ToList();
            if (chosen.Count == 0) { Warn("请至少勾选一项。"); return; }

            // 旧账户还登录着时，它正在用的文件（Outlook 邮件、浏览器数据）是锁住的，复制会失败
            if (src.Sid != null && Profiles.IsHiveLoaded(src.Sid))
            {
                DialogResult r = MessageBox.Show(this,
                    "旧账户 " + src.Account + " 目前还登录着（或刚注销、系统还没释放）。\n" +
                    "它正在使用的文件会复制失败。建议先重启电脑，再用新账户登录后运行本程序。\n\n是否仍然继续？",
                    Title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (r != DialogResult.Yes) return;
            }

            string names = string.Join("、", chosen.Select(li => ((CopyItem)li.Tag).Name));
            DialogResult ok = MessageBox.Show(this,
                "从：" + src.Path + "\n到：" + dst.Path + "\n\n复制：" + names + "\n\n" +
                "开始前请关闭新账户里的 Chrome、Edge、Outlook、Word 和便笺，\n否则它们退出时可能把复制过去的书签等文件改回去。\n\n确定开始吗？",
                Title, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (ok != DialogResult.OK) return;

            // 日志放在新账户的桌面上，用户登录后一眼能看到
            string desktop = Path.Combine(dst.Path, "Desktop");
            logPath = Path.Combine(Directory.Exists(desktop) ? desktop : dst.Path,
                                   "迁移日志_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");

            var steps = chosen.Select(li => new KeyValuePair<ListViewItem, CopyItem>(li, (CopyItem)li.Tag)).ToList();
            foreach (var s in steps) s.Key.SubItems[3].Text = "等待";

            bool skipExisting = rbSkip.Checked;
            runner = new CopyRunner();
            SetRunning(true);

            int failed = 0;
            try
            {
                failed = await Task.Run(() => CopyAll(src, dst, steps, skipExisting));
            }
            catch (Exception ex)
            {
                Warn("复制过程中出错：" + ex.Message);
            }
            finally
            {
                SetRunning(false);
            }

            if (closeRequested) { Close(); return; }

            string done = runner.Cancelled ? "已停止。" : (failed == 0 ? "全部完成。" : "完成，其中 " + failed + " 项有失败。");
            lblStatus.Text = done + "日志：" + logPath;
            MessageBox.Show(this, done + "\n\n日志文件：\n" + logPath, Title, MessageBoxButtons.OK,
                            failed == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        /// <summary>在后台线程上依次复制每一项，返回失败的项数。</summary>
        private int CopyAll(ProfileInfo src, ProfileInfo dst, List<KeyValuePair<ListViewItem, CopyItem>> steps, bool skipExisting)
        {
            string stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            int failed = 0;

            // UTF-8 带 BOM，用记事本打开中文不乱码
            using (var log = new StreamWriter(logPath, false, new UTF8Encoding(true)) { AutoFlush = true })
            {
                Action<string> write = text => { lock (log) log.WriteLine(text); };
                runner.Line += line => { latestLine = line; write(line); };

                write("账户文件迁移工具  " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                write("旧账户：" + src.Account + "  " + src.Path);
                write("新账户：" + dst.Account + "  " + dst.Path);
                write("同名文件：" + (skipExisting ? "跳过" : "旧的较新时覆盖"));

                for (int i = 0; i < steps.Count; i++)
                {
                    ListViewItem li = steps[i].Key;
                    CopyItem item = steps[i].Value;

                    if (runner.Cancelled) { SetResult(li, "已停止"); continue; }

                    currentStep = "第 " + (i + 1) + "/" + steps.Count + " 项：" + item.Name;
                    latestLine = "";
                    SetResult(li, "复制中…");
                    write("");
                    write("==================== " + item.Name + " ====================");

                    string result;
                    try
                    {
                        result = item.IsFolder
                            ? CopyFolder(src, dst, item, skipExisting, write)
                            : CopyFiles(src, dst, item, stamp, write);
                    }
                    catch (Exception ex)
                    {
                        result = "出错：" + ex.Message;
                        write(result);
                    }

                    if (result.StartsWith("有失败") || result.StartsWith("出错")) failed++;
                    write("结果：" + result);
                    SetResult(li, result);
                }
            }
            return failed;
        }

        private string CopyFolder(ProfileInfo src, ProfileInfo dst, CopyItem item, bool skipExisting, Action<string> write)
        {
            string from = Path.Combine(src.Path, item.RelativeDir);
            string to = Path.Combine(dst.Path, item.RelativeDir);
            if (!Directory.Exists(from)) return "旧账户里没有，跳过";

            // /E      包括所有子文件夹（含空文件夹）
            // /XJ     跳过链接文件夹（Documents 里的“我的音乐”等），否则会重复复制甚至死循环
            // /ZB     先用可重启模式，没权限时改用备份模式读取（管理员才有这个权限）
            // /R:1 /W:1  失败只重试 1 次、等 1 秒，默认的 100 万次会让程序看起来卡死
            // /NP /NDL   不输出百分比和文件夹名，日志只记文件
            // /XF     不复制 desktop.ini（文件夹显示名的配置，新账户自己有）、缩略图缓存、Office 打开时的临时锁文件
            // 没写 /COPY 时默认只复制内容、属性和时间，不复制旧账户的权限，复制过去的文件自动用新账户的权限
            string args = CopyRunner.Quote(from) + " " + CopyRunner.Quote(to) +
                          " /E /XJ /ZB /R:1 /W:1 /NP /NDL /XF desktop.ini Thumbs.db ~$*";
            // 跳过：排除“有改动的、较新的、较旧的”，只剩新账户里没有的文件会被复制
            // 覆盖：只排除“较旧的”，旧账户里较新的文件会替换新账户里的
            args += skipExisting ? " /XC /XN /XO" : " /XO";

            write("robocopy " + args);
            int code = runner.Run(args);
            write("robocopy 退出码：" + code);
            return CopyRunner.Describe(code);
        }

        private string CopyFiles(ProfileInfo src, ProfileInfo dst, CopyItem item, string stamp, Action<string> write)
        {
            string fromDir = Path.Combine(src.Path, item.RelativeDir);
            string toDir = Path.Combine(dst.Path, item.RelativeDir);

            List<string> present = item.Files.Where(f => File.Exists(Path.Combine(fromDir, f))).ToList();
            if (present.Count == 0) return "旧账户里没有，跳过";

            Directory.CreateDirectory(toDir);

            // 先处理新账户里已有的同组文件：
            // 旧账户里也有的 → 复制一份备份（随后被覆盖）；
            // 旧账户里没有的 → 改名移走。例如便笺的 -wal 文件，留着会被套用到复制过来的数据库上，导致数据错乱
            foreach (string f in item.Files)
            {
                string target = Path.Combine(toDir, f);
                if (!File.Exists(target)) continue;
                string backup = target + ".bak-" + stamp;
                if (present.Contains(f))
                {
                    File.Copy(target, backup, true);
                    write("已备份：" + target + " → " + backup);
                }
                else
                {
                    File.Move(target, backup);
                    write("已移走（旧账户里没有这个文件）：" + target + " → " + backup);
                }
            }

            // /IS /IT：即使大小、时间相同也照样复制，保证用的是旧账户的版本
            string args = CopyRunner.Quote(fromDir) + " " + CopyRunner.Quote(toDir) + " " +
                          string.Join(" ", present.Select(f => "\"" + f + "\"")) +
                          " /ZB /R:1 /W:1 /NP /NDL /IS /IT";

            write("robocopy " + args);
            int code = runner.Run(args);
            write("robocopy 退出码：" + code);
            return CopyRunner.Describe(code);
        }

        // ================= 其他按钮和状态 =================

        private void OnStop(object sender, EventArgs e)
        {
            if (runner == null) return;
            btnStop.Enabled = false;
            currentStep = "正在停止…";
            runner.Cancel();
        }

        private void OnOpenLog(object sender, EventArgs e)
        {
            if (logPath != null && File.Exists(logPath))
                Process.Start("notepad.exe", "\"" + logPath + "\"");
        }

        private void SetRunning(bool value)
        {
            running = value;
            cboSource.Enabled = cboTarget.Enabled = !value;
            btnBrowse.Enabled = btnReload.Enabled = btnStart.Enabled = !value;
            rbSkip.Enabled = rbNewer.Enabled = !value;
            lvItems.Enabled = !value;
            btnStop.Enabled = value;
            btnLog.Enabled = logPath != null;
            if (value) uiTimer.Start(); else uiTimer.Stop();
        }

        private void SetResult(ListViewItem li, string text)
        {
            // 后台线程不能直接改控件，交给界面线程
            BeginInvoke((Action)(() => li.SubItems[3].Text = text));
        }

        private void Warn(string text)
        {
            MessageBox.Show(this, text, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (running)
            {
                // 先不关窗口：等后台把当前的 robocopy 结束、日志写完，再由 OnStart 关闭
                e.Cancel = true;
                DialogResult r = MessageBox.Show(this, "正在复制，确定要停止并退出吗？", Title,
                                                 MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
                closeRequested = true;
                OnStop(this, EventArgs.Empty);
                return;
            }
            base.OnFormClosing(e);
        }
    }
}
