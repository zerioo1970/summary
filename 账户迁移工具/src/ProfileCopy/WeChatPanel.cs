// 账户迁移工具/src/ProfileCopy/WeChatPanel.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProfileCopy
{
    /// <summary>“③ 查找微信数据”标签页：在所有本地硬盘上找微信和企业微信的数据文件夹。</summary>
    internal sealed class WeChatPanel : UserControl
    {
        private const string Title = "账户文件迁移工具";

        private readonly Button btnSearch = new Button { Text = "开始查找", AutoSize = true };
        private readonly Button btnStop = new Button { Text = "停止", AutoSize = true, Enabled = false };
        private readonly Label lblStatus = new Label { Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
        private readonly ListView lv = new ListView
        {
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            MultiSelect = false,
            Dock = DockStyle.Fill
        };
        private readonly Button btnOpen = new Button { Text = "打开所在文件夹", AutoSize = true, Enabled = false };
        private readonly Button btnGrant = new Button { Text = "给新账户授权", AutoSize = true, Enabled = false };
        private readonly System.Windows.Forms.Timer uiTimer = new System.Windows.Forms.Timer { Interval = 200 };

        private CancellationTokenSource cts;
        private volatile string currentDir = "";
        private int foundCount;

        public WeChatPanel()
        {
            Dock = DockStyle.Fill;
            Padding = new Padding(10);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // 说明
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // 按钮 + 状态
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // 结果
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // 底部按钮

            root.Controls.Add(new Label
            {
                AutoSize = true,
                MaximumSize = new Size(900, 0),
                Margin = new Padding(3, 3, 3, 8),
                Text = "按文件夹名 WeChat Files（微信 3.x）、xwechat_files（微信 4.x）、WXWork（企业微信）查找所有本地硬盘，" +
                       "并读取各账户的微信 3.x 存储位置设置。不搜 Windows、AppData、回收站和 U 盘。硬盘大时要几分钟。"
            }, 0, 0);

            var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, AutoSize = true };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            top.Controls.Add(btnSearch, 0, 0);
            top.Controls.Add(btnStop, 1, 0);
            top.Controls.Add(lblStatus, 2, 0);
            root.Controls.Add(top, 0, 1);

            root.Controls.Add(lv, 0, 2);

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            bottom.Controls.Add(btnOpen);
            bottom.Controls.Add(btnGrant);
            bottom.Controls.Add(new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Margin = new Padding(8, 8, 3, 3),
                Text = "“给新账户授权”：让当前登录的账户能读写选中的文件夹，用于数据放在 D 盘等位置的情况。"
            });
            root.Controls.Add(bottom, 0, 3);

            Controls.Add(root);

            btnSearch.Click += OnSearch;
            btnStop.Click += delegate { if (cts != null) cts.Cancel(); btnStop.Enabled = false; };
            btnOpen.Click += delegate { OpenSelected(); };
            btnGrant.Click += OnGrant;
            lv.DoubleClick += delegate { OpenSelected(); };
            lv.SelectedIndexChanged += delegate { UpdateButtons(); };
            uiTimer.Tick += delegate { lblStatus.Text = "已找到 " + foundCount + " 项，正在查找：" + currentDir; };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            float k = DeviceDpi / 96F;
            lv.Columns.Add("软件", (int)(110 * k));
            lv.Columns.Add("位置", (int)(300 * k));
            lv.Columns.Add("账号文件夹", (int)(160 * k));
            lv.Columns.Add("最后修改", (int)(120 * k));
            lv.Columns.Add("大小", (int)(80 * k));
            lv.Columns.Add("怎么迁移", (int)(500 * k));
        }

        // ================= 查找 =================

        private async void OnSearch(object sender, EventArgs e)
        {
            lv.Items.Clear();
            foundCount = 0;
            List<ProfileInfo> profiles = Profiles.LoadAll();
            var hits = new List<KeyValuePair<ListViewItem, WeChatHit>>();

            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            btnSearch.Enabled = false;
            btnStop.Enabled = true;
            uiTimer.Start();

            try
            {
                await Task.Run(() => WeChatFinder.Search(profiles,
                    dir => currentDir = dir,
                    hit => BeginInvoke((Action)(() =>
                    {
                        ListViewItem li = AddRow(hit);
                        hits.Add(new KeyValuePair<ListViewItem, WeChatHit>(li, hit));
                    })),
                    token));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "查找时出错：" + ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            uiTimer.Stop();
            bool stopped = token.IsCancellationRequested;
            btnSearch.Enabled = true;
            btnStop.Enabled = false;

            // 等界面线程把排队中的“加一行”全部处理完再统计：
            // 从后台线程 Invoke 一个空操作，它排在所有 BeginInvoke 之后，返回时前面的都执行完了
            if (!IsDisposed) await Task.Run(() => Invoke((Action)(() => { })));
            lblStatus.Text = (stopped ? "已停止。" : "查找完成。") + "共 " + lv.Items.Count + " 项" +
                             (lv.Items.Count == 0 ? "，没有找到微信或企业微信的数据。" : "，正在计算大小…");

            await ComputeSizes(hits, token);
            if (lv.Items.Count > 0)
                lblStatus.Text = (stopped ? "已停止。" : "查找完成。") + "共 " + lv.Items.Count + " 项。双击一行可以打开所在文件夹。";
        }

        private ListViewItem AddRow(WeChatHit hit)
        {
            foundCount++;
            var li = new ListViewItem(hit.Software) { Tag = hit };
            li.SubItems.Add(hit.Path);
            li.SubItems.Add(hit.Accounts);
            li.SubItems.Add(hit.Modified.HasValue ? hit.Modified.Value.ToString("yyyy-MM-dd HH:mm") : "");
            li.SubItems.Add(hit.IsFolder ? "" : "—");
            li.SubItems.Add(hit.Note);
            if (!hit.IsFolder) li.ForeColor = Color.DimGray;   // 设置记录用灰色，和数据文件夹区分开
            lv.Items.Add(li);
            return li;
        }

        /// <summary>数据文件夹可能有几十 GB，算大小放在找完以后，一个一个算。</summary>
        private async Task ComputeSizes(List<KeyValuePair<ListViewItem, WeChatHit>> hits, CancellationToken token)
        {
            foreach (var h in hits)
            {
                if (!h.Value.IsFolder || IsDisposed) continue;
                h.Key.SubItems[4].Text = "计算中…";
                string path = h.Value.Path;
                string text = await Task.Run(() =>
                {
                    bool partial;
                    long bytes = MainForm.DirectorySize(path, out partial);
                    return (partial ? "≥ " : "") + MainForm.FormatBytes(bytes);
                });
                if (IsDisposed) return;
                h.Key.SubItems[4].Text = text;
            }
        }

        // ================= 底部按钮 =================

        private WeChatHit Selected
        {
            get { return lv.SelectedItems.Count == 0 ? null : lv.SelectedItems[0].Tag as WeChatHit; }
        }

        private void UpdateButtons()
        {
            WeChatHit h = Selected;
            btnOpen.Enabled = h != null;
            btnGrant.Enabled = h != null && h.IsFolder && btnSearch.Enabled;
        }

        private void OpenSelected()
        {
            WeChatHit h = Selected;
            if (h == null) return;
            // 文件夹直接打开；设置记录打开它所在的文件夹并选中这个文件
            string args = h.IsFolder ? "\"" + h.Path + "\"" : "/select,\"" + h.Path + "\"";
            Process.Start("explorer.exe", args);
        }

        /// <summary>
        /// 用 icacls 给当前登录的账户加“修改”权限。
        /// 数据放在 D 盘时，文件夹的权限常常只给了旧账户，新账户的微信读不了也写不了。
        /// </summary>
        private async void OnGrant(object sender, EventArgs e)
        {
            WeChatHit h = Selected;
            if (h == null || !h.IsFolder) return;

            string account;
            string sid = SessionUser.GetSid(out account);
            if (sid == null)
            {
                MessageBox.Show(this, "取不到当前登录账户（" + account + "）的信息，无法授权。", Title,
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult r = MessageBox.Show(this,
                "给 " + account + " 加上下面这个文件夹及其所有内容的“修改”权限：\n\n" + h.Path +
                "\n\n原有的权限保留不变。文件很多时要等一会儿。确定吗？",
                Title, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (r != DialogResult.OK) return;

            // *SID 表示按 SID 指定账户，不受账户名写法和语言影响
            // (OI)(CI)M：修改权限，并让以后新建的文件和子文件夹也继承
            // /T 处理所有子文件夹和文件，/C 遇到错误继续，/Q 不逐个输出成功的文件
            string path = h.Path.TrimEnd('\\');
            string args = "\"" + path + "\" /grant *" + sid + ":(OI)(CI)M /T /C /Q";

            btnGrant.Enabled = false;
            btnSearch.Enabled = false;
            Cursor = Cursors.WaitCursor;
            lblStatus.Text = "正在授权：" + path;
            try
            {
                string output = null;
                int code = await Task.Run(() => RunIcacls(args, out output));
                string summary = output.Trim();
                if (summary.Length > 1500) summary = summary.Substring(summary.Length - 1500);
                MessageBox.Show(this,
                    (code == 0 ? "授权完成。" : "授权结束，但有错误（代码 " + code + "）。") + "\n\n" + summary,
                    Title, MessageBoxButtons.OK, code == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                lblStatus.Text = code == 0 ? "授权完成：" + path : "授权有错误：" + path;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "授权失败：" + ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnSearch.Enabled = true;
                UpdateButtons();
            }
        }

        private static int RunIcacls(string args, out string output)
        {
            Encoding oem = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
            var psi = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "icacls.exe"), args)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = oem,
                StandardErrorEncoding = oem
            };
            using (Process p = Process.Start(psi))
            {
                // 错误输出放到另一个线程读，避免两个管道互相等待卡死
                Task<string> err = p.StandardError.ReadToEndAsync();
                string stdout = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                output = stdout + err.Result;
                return p.ExitCode;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (cts != null) cts.Cancel();
                uiTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
