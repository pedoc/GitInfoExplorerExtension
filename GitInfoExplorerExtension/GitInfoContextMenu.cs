using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using LibGit2Sharp;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace GitInfoExplorerExtension
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.AllFiles)]
    [COMServerAssociation(AssociationType.Directory)]
    public class GitInfoContextMenu : SharpContextMenu
    {
        private const string GitDirectoryName = ".git";
        private const string GitInfoMenuName = "GitInfo";

        private ContextMenuStrip _menu = new ContextMenuStrip();

        protected override bool CanShowMenu()
        {
            if (SelectedItemPaths.Count() == 1 && IsGitDirectory(SelectedItemPaths.First()))//选中一个目录时构建菜单并显示
            {
                this.UpdateMenu();
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void Log(string msg) => Debug.WriteLine(msg);

        public static bool IsGitDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            if (!IsDirectory(path)) path = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(path)) return false;
            return !string.IsNullOrEmpty(GetGitDirectory(path));
        }

        private static string GetGitDirectory(string path)
        {
            if (!IsDirectory(path)) path = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(path)) return String.Empty;
            var directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.GetDirectories(GitDirectoryName, SearchOption.TopDirectoryOnly).Length == 1) return directoryInfo.FullName;
            return GetGitDirectory(directoryInfo.Parent?.FullName);
        }

        protected override ContextMenuStrip CreateMenu()
        {
            BuildGitInfoContextMenus();
            return _menu;
        }

        private void UpdateMenu()
        {
            _menu.Dispose();
            _menu = CreateMenu();
        }

        private static bool IsDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            var attr = File.GetAttributes(path);
            return attr.HasFlag(FileAttributes.Directory);
        }

        protected void BuildGitInfoContextMenus()
        {
            _menu.Items.Clear();
            var gitDir = GetGitDirectory(SelectedItemPaths.First());
            if (string.IsNullOrEmpty(gitDir) || !Directory.Exists(gitDir)) return;
            var repo = new Repository(gitDir);

            var lastCommit = repo.Head.Tip;
            var mainMenu = new ToolStripMenuItem
            {
                Text = GitInfoMenuName,
                Image = Properties.Resources.Git
            };
            mainMenu.DropDownItems.Add(new ToolStripMenuItem { Text = $@"Branch Name: {repo.Head.FriendlyName} <-> { repo.Head.RemoteName}:{repo.Head.TrackedBranch?.FriendlyName}" });
            mainMenu.DropDownItems.Add(new ToolStripMenuItem { Text = @"Branches Count: " + repo.Branches.Count().ToString() });
            mainMenu.DropDownItems.Add(new ToolStripMenuItem { Text = @"Tags Count: " + repo.Tags.Count().ToString() });
            mainMenu.DropDownItems.Add(new ToolStripSeparator());
            mainMenu.DropDownItems.Add(new ToolStripMenuItem { Text = $@"Last Commit : {lastCommit.Message} {lastCommit.Author} {lastCommit.Author.When.ToString()}" });
            _menu.Items.Clear();
            _menu.Items.Add(mainMenu);
            repo.Dispose();
        }
    }
}