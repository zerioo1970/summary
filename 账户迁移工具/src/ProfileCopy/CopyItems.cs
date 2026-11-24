// 账户迁移工具/src/ProfileCopy/CopyItems.cs
namespace ProfileCopy
{
    /// <summary>可勾选的一项：要么整个文件夹，要么某个文件夹里的几个指定文件。</summary>
    internal sealed class CopyItem
    {
        /// <summary>界面上显示的名字。</summary>
        public string Name;

        /// <summary>相对配置文件夹的路径，例如 Desktop、AppData\Roaming\Microsoft\Templates。</summary>
        public string RelativeDir;

        /// <summary>null 表示复制整个文件夹；否则只复制这些文件。</summary>
        public string[] Files;

        /// <summary>打开程序时是否默认打钩。</summary>
        public bool CheckedByDefault;

        public bool IsFolder { get { return Files == null; } }

        public string DisplayPath
        {
            get { return IsFolder ? RelativeDir : RelativeDir + "\\" + string.Join("、", Files); }
        }
    }

    internal static class CopyItems
    {
        // 只列“用户自己的数据”。注册表（NTUSER.DAT）和整个 AppData 故意不列：
        // 那些是旧账户的设置，正是 ProfWiz 迁移后出问题的地方，复制过去会把问题一起带过去
        public static readonly CopyItem[] All =
        {
            Folder("桌面", "Desktop", true),
            Folder("文档", "Documents", true),
            Folder("下载", "Downloads", true),
            Folder("图片", "Pictures", true),
            Folder("视频", "Videos", true),
            Folder("音乐", "Music", true),
            Folder("收藏夹（IE）", "Favorites", true),
            Folder("链接", "Links", false),
            Folder("联系人", "Contacts", false),
            Folder("保存的游戏", "Saved Games", false),
            Folder("Outlook 签名", @"AppData\Roaming\Microsoft\Signatures", false),

            FileSet("Word 默认模板", @"AppData\Roaming\Microsoft\Templates", false, "Normal.dotm"),
            FileSet("Chrome 书签", @"AppData\Local\Google\Chrome\User Data\Default", false, "Bookmarks"),
            FileSet("Edge 书签", @"AppData\Local\Microsoft\Edge\User Data\Default", false, "Bookmarks"),
            // 便笺是 SQLite 数据库，主文件和 -wal、-shm 必须成套处理，否则数据会错乱
            FileSet("便笺", @"AppData\Local\Packages\Microsoft.MicrosoftStickyNotes_8wekyb3d8bbwe\LocalState", false,
                    "plum.sqlite", "plum.sqlite-wal", "plum.sqlite-shm"),
        };

        private static CopyItem Folder(string name, string dir, bool check)
        {
            return new CopyItem { Name = name, RelativeDir = dir, CheckedByDefault = check };
        }

        private static CopyItem FileSet(string name, string dir, bool check, params string[] files)
        {
            return new CopyItem { Name = name, RelativeDir = dir, CheckedByDefault = check, Files = files };
        }
    }
}
