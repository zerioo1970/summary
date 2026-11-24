// 账户迁移工具/src/ProfileCopy/AccountPanel.cs
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProfileCopy
{
    /// <summary>“① 建立账号”标签页：新建本机账号，或把已有的域账号加入本机的组。</summary>
    internal sealed class AccountPanel : UserControl
    {
        private const string Title = "账户文件迁移工具";

        private readonly RadioButton rbLocal = new RadioButton { Text = "新建本机账号", AutoSize = true, Checked = true };
        private readonly RadioButton rbDomain = new RadioButton { Text = "加入已有的域账号（域账号要先在服务器上建好）", AutoSize = true };

        // 本机账号
        private readonly TextBox txtLocalUser = NewBox();
        private readonly TextBox txtFullName = NewBox();
        private readonly TextBox txtPassword = NewBox(true);
        private readonly TextBox txtPassword2 = NewBox(true);
        private readonly CheckBox chkNeverExpires = new CheckBox { Text = "密码永不过期", AutoSize = true };

        // 域账号
        private readonly TextBox txtDomain = NewBox();
        private readonly TextBox txtDomainUser = NewBox();

        private readonly CheckBox chkAdmin = new CheckBox { Text = "加入本机管理员组（Administrators）", AutoSize = true };
        private readonly Button btnCreate = new Button { Text = "建立", AutoSize = true };
        private readonly Label lblResult = new Label { AutoSize = true, MaximumSize = new Size(700, 0), Margin = new Padding(3, 12, 3, 3) };

        private GroupBox grpLocal;
        private GroupBox grpDomain;

        public AccountPanel()
        {
            Dock = DockStyle.Fill;
            Padding = new Padding(10);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };

            flow.Controls.Add(rbLocal);
            grpLocal = MakeGroup("本机账号",
                Row("用户名：", txtLocalUser),
                Row("全名（可不填）：", txtFullName),
                Row("密码：", txtPassword),
                Row("再输一次密码：", txtPassword2),
                Row("", chkNeverExpires));
            flow.Controls.Add(grpLocal);

            flow.Controls.Add(rbDomain);
            grpDomain = MakeGroup("域账号",
                Row("域名：", txtDomain),
                Row("用户名：", txtDomainUser),
                Row("", new Label
                {
                    AutoSize = true,
                    ForeColor = Color.DimGray,
                    Text = "域名填短名称，也就是登录界面上“域名\\用户名”里的那个，例如 CONTOSO。"
                }));
            flow.Controls.Add(grpDomain);

            flow.Controls.Add(chkAdmin);
            flow.Controls.Add(new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Text = "不勾选就是普通用户（Users 组）。"
            });
            flow.Controls.Add(btnCreate);
            flow.Controls.Add(lblResult);
            Controls.Add(flow);

            // 这台电脑加入了域，就把域名预先填好；没加入域，就不能选域账号
            string domain = AccountService.JoinedDomain();
            if (domain != null)
            {
                txtDomain.Text = domain;
            }
            else
            {
                rbDomain.Enabled = false;
                rbDomain.Text = "加入已有的域账号（这台电脑没有加入域，不能用）";
            }

            rbLocal.CheckedChanged += delegate { UpdateEnabled(); };
            rbDomain.CheckedChanged += delegate { UpdateEnabled(); };
            btnCreate.Click += OnCreate;
            UpdateEnabled();
        }

        private static TextBox NewBox(bool password = false)
        {
            return new TextBox { Width = 260, UseSystemPasswordChar = password };
        }

        private static Control[] Row(string label, Control input)
        {
            return new Control[]
            {
                new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 7, 3, 3) },
                input
            };
        }

        private static GroupBox MakeGroup(string text, params Control[][] rows)
        {
            var table = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Dock = DockStyle.Fill };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            for (int i = 0; i < rows.Length; i++)
            {
                table.Controls.Add(rows[i][0], 0, i);
                table.Controls.Add(rows[i][1], 1, i);
            }
            var g = new GroupBox { Text = text, AutoSize = true, Padding = new Padding(10, 6, 10, 6), Margin = new Padding(20, 3, 3, 10) };
            g.Controls.Add(table);
            return g;
        }

        private void UpdateEnabled()
        {
            grpLocal.Enabled = rbLocal.Checked;
            grpDomain.Enabled = rbDomain.Checked;
        }

        // ================= 点“建立” =================

        private async void OnCreate(object sender, EventArgs e)
        {
            lblResult.Text = "";
            lblResult.ForeColor = SystemColors.ControlText;
            bool admin = chkAdmin.Checked;
            string groupSid = admin ? AccountService.AdministratorsSid : AccountService.UsersSid;

            string account;      // 加入组时用的“域名\用户名”
            string loginName;    // 提示用户登录时用的名字
            Func<string> work;   // 在后台执行的操作，返回结果说明

            if (rbLocal.Checked)
            {
                string user = txtLocalUser.Text.Trim();
                string error = CheckLocalUserName(user);
                if (error != null) { Warn(error); return; }
                if (txtPassword.Text.Length == 0) { Warn("请输入密码。没有密码的账号不能从网络访问，也不安全。"); return; }
                if (txtPassword.Text != txtPassword2.Text) { Warn("两次输入的密码不一样。"); return; }

                account = Environment.MachineName + "\\" + user;
                loginName = ".\\" + user;
                string fullName = txtFullName.Text;
                string password = txtPassword.Text;
                bool neverExpires = chkNeverExpires.Checked;

                bool exists;
                try
                {
                    exists = await Task.Run(() => AccountService.LocalUserExists(user));
                }
                catch (Exception ex)
                {
                    Warn("检查用户名是否已存在时出错：" + ex.Message);
                    return;
                }
                if (exists)
                {
                    DialogResult r = MessageBox.Show(this,
                        "本机已经有账号 " + user + "，不会重新建立，也不会改它的密码。\n\n是否只把它加入" +
                        (admin ? "管理员组" : "普通用户组") + "？",
                        Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (r != DialogResult.Yes) return;
                    work = () => AddText(account, groupSid);
                }
                else
                {
                    work = () =>
                    {
                        AccountService.CreateLocalUser(user, fullName, password, neverExpires);
                        // 新建的本机账号 Windows 会自动放进 Users 组；勾选了管理员时再加入 Administrators
                        return "已建立本机账号 " + user + "。" + (admin ? AddText(account, groupSid) : "");
                    };
                }
            }
            else
            {
                string domain = txtDomain.Text.Trim();
                string user = txtDomainUser.Text.Trim();
                if (domain.Length == 0 || user.Length == 0) { Warn("请填写域名和用户名。"); return; }
                if (domain.Contains(".")) { Warn("域名请填短名称（例如 CONTOSO），不要填 contoso.com 这样的完整名称。"); return; }
                if (user.Contains("\\")) { Warn("用户名里不要带域名，域名填在上面一栏。"); return; }

                account = domain + "\\" + user;
                loginName = account;
                work = () => AddText(account, groupSid);
            }

            btnCreate.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                // 加域账号要连域控制器，可能要几秒，放到后台避免窗口卡住
                string done = await Task.Run(work);
                lblResult.ForeColor = Color.DarkGreen;
                lblResult.Text = done + "\n\n下一步：注销，用 " + loginName + " 登录一次（Windows 会生成它的配置文件夹），" +
                                 "然后再运行本程序，在“② 复制文件”页复制旧账户的文件。" +
                                 (rbDomain.Checked ? "\n域账号第一次登录时，这台电脑必须能连上域控制器。" : "");
                txtPassword.Clear();
                txtPassword2.Clear();
            }
            catch (Exception ex)
            {
                lblResult.ForeColor = Color.Firebrick;
                lblResult.Text = "失败：" + ex.Message;
            }
            finally
            {
                btnCreate.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        /// <summary>加入组并返回说明文字（在后台线程上调用）。</summary>
        private static string AddText(string account, string groupSid)
        {
            string group = AccountService.GroupName(groupSid);
            bool added = AccountService.AddToLocalGroup(account, groupSid);
            return added
                ? "已把 " + account + " 加入本机 " + group + " 组。"
                : account + " 原来就在本机 " + group + " 组里。";
        }

        /// <summary>按 Windows 的规则检查本机用户名，不合格返回原因。</summary>
        private static string CheckLocalUserName(string name)
        {
            if (name.Length == 0) return "请输入用户名。";
            if (name.Length > 20) return "用户名最多 20 个字符。";
            const string bad = "\"/\\[]:;|=,+*?<>@";
            if (name.Any(c => bad.IndexOf(c) >= 0 || char.IsControl(c)))
                return "用户名不能包含这些字符：\" / \\ [ ] : ; | = , + * ? < > @";
            if (name.Trim('.').Length == 0) return "用户名不能只由句点组成。";
            if (string.Equals(name, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
                return "用户名不能和计算机名相同。";
            return null;
        }

        private void Warn(string text)
        {
            MessageBox.Show(this, text, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
