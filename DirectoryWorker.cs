using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace mobafireClient
{
    static class DirectoryWorker
    {
        static public string DownloadImage(string folder, string url, string fileName)
        {
            if (!CreateFolder("Images/" + folder)) return "";
            var downloadDir = Directory.GetCurrentDirectory() + "/Images/" + folder + "/" + fileName;
            if (File.Exists(downloadDir)) return downloadDir;
            var imgUrl = "http://www.mobafire.com" + url;
            using (var downloader = new WebClient())
            {
                downloader.DownloadFile(imgUrl, downloadDir);
            }
            return downloadDir;
        }

        static public bool CreateFolder(string path)
        {
            var folders = path.Split('/');
            path = "";
            try
            {
                foreach (var folder in folders)
                {
                    path += folder + "/";
                    bool exists = Directory.Exists(path);
                    if (!exists)
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
