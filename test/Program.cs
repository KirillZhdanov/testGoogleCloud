using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = GetFilePath("data");

                watcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName;


                watcher.Created += OnChanged;
                watcher.EnableRaisingEvents = true;
                watcher.IncludeSubdirectories = true;

                while (true) ;
            }

        }
        /// <summary>
        /// Get  changes in folder
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"File: {e.FullPath} \n {e.ChangeType} \n NAME:{e.Name}");
            try
            {
                string bucketName = "ecommerce.imagetemplates.com";
                string sharedkeyFilePath = GetFilePath("imagetemplates-marketing-fd47d29b0ca7.json");
                GoogleCredential credential = null;
                using (var jsonStream = new FileStream(sharedkeyFilePath, FileMode.Open,
                    FileAccess.Read, FileShare.Read))
                {
                    credential = GoogleCredential.FromStream(jsonStream);
                }

                var storageClient = StorageClient.Create(credential);
                string filetoUpload = GetFilePath("data");
                Console.WriteLine($"FILETOUPLOAD: { filetoUpload}");
                List<string> str = DirSearch(filetoUpload);
                FileAttributes attr = File.GetAttributes(e.FullPath);

                if (!attr.HasFlag(FileAttributes.Directory))
                {
                    using (var fileStream = new FileStream(e.FullPath, FileMode.Open,
                   FileAccess.Read, FileShare.Read))
                    {

                        storageClient.UploadObject(bucketName, $"{e.Name.Replace("\\","/")}", "Folder", fileStream);
                    }
                }
                else
                {
                    var content = Encoding.UTF8.GetBytes("");
                    Console.WriteLine($"DIRECTORY{ e.Name}");
                    storageClient.UploadObject(bucketName, $"{e.Name.Replace("\\", "/")}/", "Folder", new MemoryStream(content));
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static List<String> DirSearch(string sDir)
        {
            List<String> files = new List<String>();
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(DirSearch(d));
                }
            }
            catch (System.Exception excpt)
            {
                throw excpt;
            }

            return files;
        }
        public static string GetFilePath(string filename)
        {
            return GetFilePath(Assembly.GetCallingAssembly().Location, filename);
        }
        public static string GetFilePath(string path, string filename)
        {
            return path.Substring(0, path.LastIndexOf("\\")) + @"\" + filename;
        }
    }
}
