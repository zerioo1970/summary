// 账户迁移工具/src/ProfileCopy/Program.cs
using System;
using System.Security.Principal;
using System.Windows.Forms;

namespace ProfileCopy
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 清单已要求管理员权限，这里再检查一次：
            // 如果有人用“以普通用户身份运行”之类的办法绕过 UAC，读旧账户文件夹会全部失败
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show(
                    "本程序需要管理员权限才能读取旧账户的文件夹。\n请右键 ProfileCopy.exe，选择“以管理员身份运行”。",
                    "账户文件迁移工具", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Application.Run(new MainForm());
        }
    }
}
