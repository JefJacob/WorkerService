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
            logger.Info($"File Extension: { _watchedExtensions.FirstOrDefault()}");
            if (_watchedExtensions.Any(ext => e.Name.EndsWith(ext)))
            {
                try
                {
                    if (IsFileClosed(e.FullPath, true))
                    {
                        logger.Info("Calling File Processor.exe");
                        //string strCmdText;
                        //strCmdText = @"/C ipconfig /all > J:\Courses\Capstone\FileUpload\network_info_new.txt";
                        //System.Diagnostics.Process.Start("CMD.exe", strCmdText);
                        DataReader.ProcessFile(e.FullPath);
                        var destinationFilePath = ConfigurationManager.AppSettings["dest"];
                        var destinationFile = destinationFilePath + Path.GetFileName(e.FullPath);
                        if (File.Exists(destinationFile))
                        {
                            File.Delete(destinationFile);
                            
                        }
                        
                        File.Move(e.FullPath, destinationFile);
                        logger.Info($"File moved to {destinationFile}");
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
    }
}
