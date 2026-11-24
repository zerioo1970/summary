// 账户迁移工具/src/ProfileCopy/Profiles.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace ProfileCopy
{
    /// <summary>一个账户的配置文件，也就是 C:\Users\某某 这个文件夹。</summary>
    internal sealed class ProfileInfo
    {
        /// <summary>账户的 SID。用“其他文件夹…”手动选的文件夹没有 SID，为 null。</summary>
        public string Sid;

        /// <summary>配置文件夹的完整路径，例如 C:\Users\B。</summary>
        public string Path;

        /// <summary>显示用的账户名，例如 CONTOSO\B；查不到时写“未知账户”。</summary>
        public string Account;

        /// <summary>读取列表时这个账户是否还登录着。</summary>
        public bool IsLoaded;

        public override string ToString()
        {
            return Account + "    " + Path + (IsLoaded ? "    （登录中）" : "");
        }
    }

    internal static class Profiles
    {
        // Windows 在这里登记本机所有配置文件：子键名是 SID，ProfileImagePath 是文件夹路径
        private const string ProfileListKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList";

        /// <summary>读出本机所有“真人账户”的配置文件（跳过 SYSTEM、网络服务等系统账户）。</summary>
        public static List<ProfileInfo> LoadAll()
        {
            var list = new List<ProfileInfo>();
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(ProfileListKey))
            {
                if (key == null) return list;

                foreach (string sid in key.GetSubKeyNames())
                {
                    // S-1-5-21-…：本地账户和 AD 域账户；S-1-12-1-…：Entra ID（Azure AD）账户
                    // 其余（S-1-5-18/19/20 等）是系统账户，不显示
                    bool isUser = sid.StartsWith("S-1-5-21-", StringComparison.Ordinal)
                               || sid.StartsWith("S-1-12-1-", StringComparison.Ordinal);
                    // 以 .bak 结尾的是 Windows 留下的损坏配置文件备份记录，不显示
                    if (!isUser || sid.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)) continue;

                    using (RegistryKey sub = key.OpenSubKey(sid))
                    {
                        string raw = sub == null ? null : sub.GetValue("ProfileImagePath") as string;
                        if (string.IsNullOrEmpty(raw)) continue;

                        list.Add(new ProfileInfo
                        {
                            Sid = sid,
                            // 注册表里常写成 %SystemDrive%\Users\B，要展开成真实路径
                            Path = Environment.ExpandEnvironmentVariables(raw),
                            Account = TranslateSid(sid),
                            IsLoaded = IsHiveLoaded(sid)
                        });
                    }
                }
            }
            list.Sort((a, b) => string.Compare(a.Path, b.Path, StringComparison.OrdinalIgnoreCase));
            return list;
        }

        /// <summary>SID 转成“域名\用户名”。旧域的账户在新域里查不到，会显示“未知账户”。</summary>
        private static string TranslateSid(string sid)
        {
            try
            {
                return new SecurityIdentifier(sid).Translate(typeof(NTAccount)).Value;
            }
            catch (Exception)
            {
                return "未知账户（" + sid + "）";
            }
        }

        /// <summary>
        /// HKEY_USERS 下有这个 SID，说明这个账户的注册表正被加载，也就是还登录着。
        /// 刚注销时系统有时会晚一点释放，重启电脑就一定释放了。
        /// </summary>
        public static bool IsHiveLoaded(string sid)
        {
            if (string.IsNullOrEmpty(sid)) return false;
            using (RegistryKey k = Registry.Users.OpenSubKey(sid))
            {
                return k != null;
            }
        }
    }

    /// <summary>
    /// 取“坐在电脑前登录的那个用户”。
    /// 用别的管理员密码提权运行本程序时，程序本身的身份是那个管理员，
    /// 但会话里登录的仍是新账户，新账户才是要复制到的目标。
    /// </summary>
    internal static class SessionUser
    {
        private const int WTSUserName = 5;
        private const int WTSDomainName = 7;

        [DllImport("wtsapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool WTSQuerySessionInformation(
            IntPtr hServer, int sessionId, int wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pMemory);

        private static string Query(int infoClass)
        {
            IntPtr buffer;
            int bytes;
            // IntPtr.Zero 表示本机；会话号用本程序所在的会话
            if (!WTSQuerySessionInformation(IntPtr.Zero, Process.GetCurrentProcess().SessionId,
                                            infoClass, out buffer, out bytes))
                return null;
            try { return Marshal.PtrToStringUni(buffer); }
            finally { WTSFreeMemory(buffer); }
        }

        /// <summary>返回会话用户的 SID；account 输出“域名\用户名”。查不到返回 null。</summary>
        public static string GetSid(out string account)
        {
            string user = Query(WTSUserName);
            string domain = Query(WTSDomainName);
            account = string.IsNullOrEmpty(domain) ? user : domain + "\\" + user;
            if (string.IsNullOrEmpty(user)) return null;

            try
            {
                var nt = new NTAccount(domain, user);
                return ((SecurityIdentifier)nt.Translate(typeof(SecurityIdentifier))).Value;
            }
            catch (Exception)
            {
                // 连不上域控时可能翻译失败。如果程序就是用会话用户本人提权运行的，直接取自己的 SID
                WindowsIdentity me = WindowsIdentity.GetCurrent();
                if (me.User != null && me.Name.EndsWith("\\" + user, StringComparison.OrdinalIgnoreCase))
                    return me.User.Value;
                return null;
            }
        }
    }
}
