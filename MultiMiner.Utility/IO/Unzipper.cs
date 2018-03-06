using MultiMiner.Utility.OS;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace MultiMiner.Utility.IO
{
    public static class Unzipper
    {
        public static void UnzipFileToFolder(string zipFilePath, string destionationFolder)
        {
            Directory.CreateDirectory(destionationFolder);

            if (OSVersionPlatform.GetGenericPlatform() == PlatformID.Unix)
                UnzipFileToFolderUnix(zipFilePath, destionationFolder);
#if !__MonoCS__
            else
	            UnzipFileToFolderUwp(zipFilePath, destionationFolder);
			//                UnzipFileToFolderWindows(zipFilePath, destionationFolder);
#endif
		}

        private static void UnzipFileToFolderUnix(string zipFilePath, string destinationFolder)
        {

            string temporaryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(temporaryPath);
            
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "tar";
            startInfo.Arguments = String.Format("-xzf \"{0}\" -C \"{1}\"", zipFilePath, temporaryPath);
            
            Process process = Process.Start(startInfo);
            process.WaitForExit();

            DirectoryInfo directoryInfo = new DirectoryInfo(temporaryPath);
            FileInfo[] files = directoryInfo.GetFiles();
            DirectoryInfo[] directories = directoryInfo.GetDirectories();

            //if the zip file contains a single directory, extract that directory's contents
            if ((files.Length == 0) && (directories.Length == 1))
            {
                CopyDirectoryContents(directories[0].FullName, destinationFolder);
            }
            else
            {
                CopyDirectoryContents(temporaryPath, destinationFolder);
            }
            
            Directory.Delete(temporaryPath, true);
        }

        private static void CopyDirectoryContents(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);

            foreach (var directory in Directory.GetDirectories(sourceDir))
                CopyDirectoryContents(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
        }

	    private static void UnzipFileToFolderUwp(string zipFilePath, string destinationFolder)
	    {
		    var temporaryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		    using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Read))
		    {
			    archive.ExtractToDirectory(temporaryPath);
		    }
		    var directoryInfo = new DirectoryInfo(temporaryPath);
		    var files = directoryInfo.GetFiles();
		    var directories = directoryInfo.GetDirectories();

		    if ((files.Length == 0) && (directories.Length == 1))
		    {
			    CopyDirectoryContents(directories[0].FullName, destinationFolder);
		    }
		    else
		    {
			    CopyDirectoryContents(temporaryPath, destinationFolder);
		    }

		    Directory.Delete(temporaryPath, true);
	    }

    }
}
