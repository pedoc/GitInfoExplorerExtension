using System.IO;

namespace GitInfoExplorerExtension
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var result= GitInfoContextMenu.IsGitDirectory(@"E:\RNSS\dashboard\entrypoint.sh");
        }
    }
}