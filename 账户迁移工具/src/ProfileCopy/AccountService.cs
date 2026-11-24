// 账户迁移工具/src/ProfileCopy/AccountService.cs
using System;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace ProfileCopy
{
    /// <summary>本机账号的新建，以及把账号（本机或域）加入本机的组。</summary>
    internal static class AccountService
    {
        // 内置组用固定的 SID 找，不用名字：不同语言的 Windows 组名可能不一样
        public const string AdministratorsSid = "S-1-5-32-544";
        public const string UsersSid = "S-1-5-32-545";

        // ---------- 新建本机账号 ----------

        /// <summary>本机是否已有这个用户名。</summary>
        public static bool LocalUserExists(string userName)
        {
            using (var ctx = new PrincipalContext(ContextType.Machine))
            using (UserPrincipal u = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, userName))
            {
                return u != null;
            }
        }

        /// <summary>新建一个已启用的本机账号。失败时抛出异常，Message 是中文说明。</summary>
        public static void CreateLocalUser(string userName, string fullName, string password, bool passwordNeverExpires)
        {
            try
            {
                using (var ctx = new PrincipalContext(ContextType.Machine))
                using (var u = new UserPrincipal(ctx, userName, password, true))
                {
                    // 本机账号的 DisplayName 就是“计算机管理”里看到的“全名”
                    if (!string.IsNullOrWhiteSpace(fullName)) u.DisplayName = fullName.Trim();
                    u.PasswordNeverExpires = passwordNeverExpires;
                    u.Save();
                }
            }
            catch (PasswordException)
            {
                throw new InvalidOperationException(
                    "密码不符合这台电脑的密码策略（长度、复杂度或历史记录要求），请换一个更复杂的密码。");
            }
            catch (PrincipalExistsException)
            {
                throw new InvalidOperationException("这个用户名已经存在。");
            }
        }

        // ---------- 加入本机组 ----------

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LOCALGROUP_MEMBERS_INFO_3
        {
            public string lgrmi3_domainandname;   // “域名\用户名”或“计算机名\用户名”
        }

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetLocalGroupAddMembers(
            string serverName, string groupName, int level, ref LOCALGROUP_MEMBERS_INFO_3 buf, int totalEntries);

        private const int NERR_Success = 0;
        private const int ERROR_ACCESS_DENIED = 5;
        private const int ERROR_NONE_MAPPED = 1332;       // 找不到这个账号
        private const int ERROR_MEMBER_IN_ALIAS = 1378;   // 已经是组成员
        private const int ERROR_INVALID_MEMBER = 1388;
        private const int NERR_GroupNotFound = 2220;

        /// <summary>
        /// 把账号加入本机的组。account 写成“域名\用户名”或“计算机名\用户名”。
        /// 用 Windows 的 NetLocalGroupAddMembers：它让本机通过计算机账户去域里查域账号，
        /// 运行本程序的管理员是本机账号也能加域账号，不需要域管理员密码。
        /// 返回 true 表示新加入，false 表示原来就是成员；失败时抛出异常。
        /// </summary>
        public static bool AddToLocalGroup(string account, string groupSid)
        {
            string group = GroupName(groupSid);
            var info = new LOCALGROUP_MEMBERS_INFO_3 { lgrmi3_domainandname = account };
            int rc = NetLocalGroupAddMembers(null, group, 3, ref info, 1);

            switch (rc)
            {
                case NERR_Success: return true;
                case ERROR_MEMBER_IN_ALIAS: return false;
                case ERROR_NONE_MAPPED:
                case ERROR_INVALID_MEMBER:
                    throw new InvalidOperationException(
                        "找不到账号 " + account + "。请检查域名和用户名，并确认这台电脑能连上域控制器（网线、VPN）。");
                case ERROR_ACCESS_DENIED:
                    throw new InvalidOperationException("没有权限修改本机的组，请以管理员身份运行本程序。");
                case NERR_GroupNotFound:
                    throw new InvalidOperationException("找不到本机的 " + group + " 组。");
                default:
                    throw new InvalidOperationException("加入 " + group + " 组失败，错误代码 " + rc + "。");
            }
        }

        /// <summary>由 SID 取本机上的组名，例如 S-1-5-32-544 → Administrators。</summary>
        public static string GroupName(string groupSid)
        {
            string full = new SecurityIdentifier(groupSid).Translate(typeof(NTAccount)).Value;   // BUILTIN\Administrators
            int slash = full.IndexOf('\\');
            return slash >= 0 ? full.Substring(slash + 1) : full;
        }

        // ---------- 这台电脑加入的域 ----------

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetGetJoinInformation(string server, out IntPtr nameBuffer, out int joinStatus);

        [DllImport("netapi32.dll")]
        private static extern int NetApiBufferFree(IntPtr buffer);

        private const int NetSetupDomainName = 3;

        /// <summary>返回这台电脑加入的域的短名称（例如 CONTOSO）；没加入域返回 null。</summary>
        public static string JoinedDomain()
        {
            IntPtr buffer;
            int status;
            if (NetGetJoinInformation(null, out buffer, out status) != 0) return null;
            try
            {
                return status == NetSetupDomainName ? Marshal.PtrToStringUni(buffer) : null;
            }
            finally
            {
                NetApiBufferFree(buffer);
            }
        }
    }
}
