using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;

namespace FileProcessor.WorkerService
{
    public class Worker : BackgroundService
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string _path;
        private readonly List<string> _watchedExtensions;
        public Worker( string filePath, string watchedExtensions)
        {
            _path = filePath ?? throw new ArgumentNullException("Worker:filePath cannot be null.");
            _watchedExtensions = watchedExtensions.Split(';').ToList() ?? throw new ArgumentNullException("Worker:watchedExtensions cannot be null.");
            string datapath = ConfigurationManager.AppSettings["datapath"];
            string dest = ConfigurationManager.AppSettings["dest"];
            if (!Directory.Exists(datapath))
            {
                DirectoryInfo di = Directory.CreateDirectory(datapath);
            }
            if (!Directory.Exists(dest))
            {
                DirectoryInfo di = Directory.CreateDirectory(dest);
            }
        }



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (FileSystemWatcher dirWatcher = new FileSystemWatcher())
            {
                dirWatcher.Path = _path;
                dirWatcher.NotifyFilter = NotifyFilters.FileName;
                dirWatcher.Filter = "*.*";
                dirWatcher.Created += OnChanged;
                dirWatcher.EnableRaisingEvents = true;

                while (!stoppingToken.IsCancellationRequested)
                {
                    Thread.Sleep(100);
                }
            }
        }
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            logger.Info($"OnChange event start: {e.FullPath} {DateTime.Now}");
            
            if (_watchedExtensions.Any(ext => e.Name.EndsWith(ext)))
            {
                try
                {
                    if (IsFileClosed(e.FullPath, true))
                    {
                        DataReader.ProcessFile(e.FullPath);

                        var destinationFilePath = ConfigurationManager.AppSettings["dest"];
                        var destinationFullpath = destinationFilePath + Path.GetFileName(e.FullPath);
                        //if (File.Exists(destinationFile))
                        //{
                        //    File.Delete(destinationFile);

                        //}
                        destinationFullpath = GetNewPath(destinationFullpath);
                        File.Move(e.FullPath, destinationFullpath);
                        logger.Info($"File moved to {destinationFullpath}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Exception:{ex.Message}");
                }
            }
            logger.Info($"OnChange event end: {e.FullPath} {DateTime.Now}");
        }
        private bool IsFileClosed(string filepath, bool wait)
        {
            bool fileClosed = false;
            int retries = 20;
            const int delayMS = 100;

            if (!File.Exists(filepath))
                return false;
            do
            {
                try
                {
                    FileStream fs = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    fs.Close();
                    fileClosed = true; // success
                }
                catch (IOException) { }

                if (!wait) break;

                retries--;

                if (!fileClosed)
                    Thread.Sleep(delayMS);
            }
            while (!fileClosed && retries > 0);
            return fileClosed;
        }
        private string GetNewPath(string fullPath)
        {
            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while (File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }
            return newFullPath;
        }
    }
}
