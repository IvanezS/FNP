using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;


namespace FNP
{
    class Program
    {
        static void Main(string[] args)
        {
            Run();
        }


        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static void Run()
        {
            string[] args = Environment.GetCommandLineArgs();

            // If a directory is not specified, exit program.
            if (args.Length != 2)
            {
                // Display the proper way to call the program.
                Console.WriteLine("FNP.exe (path to printed directory) ");
                return;
            }

            // Create a new FileSystemWatcher and set its properties.
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = args[1];

                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName;

                // Only watch text files.
                watcher.Filter = "*.pdf";

                // Add event handlers.
                //watcher.Changed += OnChanged;
                watcher.Created += OnChanged;
                //watcher.Deleted += OnChanged;
                //watcher.Renamed += OnRenamed;

                // Begin watching.
                watcher.EnableRaisingEvents = true;

                // Wait for the user to quit the program.
                Console.WriteLine("Press 'q' to quit.");
                while (Console.Read() != 'q') ;
            }
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
            //for(int i = 0; i < 4; i++)
                pdfPrint(e.FullPath);

            Console.WriteLine("File printed");
            System.Threading.Thread.Sleep(5000);
            File.Delete(e.FullPath);
            Console.WriteLine("File deleted");

        }
        //private static void OnRenamed(object source, RenamedEventArgs e) =>
        // Specify what is done when a file is renamed.
        //   Console.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath}");


        public static void pdfPrint(string pdfFileName)
        {
            string processFilename = Microsoft.Win32.Registry.LocalMachine
                 .OpenSubKey("Software")
                 .OpenSubKey("Microsoft")
                 .OpenSubKey("Windows")
                 .OpenSubKey("CurrentVersion")
                 .OpenSubKey("App Paths")
                 .OpenSubKey("AcroRd32.exe")
                 .GetValue(String.Empty).ToString();

            ProcessStartInfo info = new ProcessStartInfo();
            info.Verb = "print";
            info.FileName = processFilename;
            info.Arguments = String.Format("/p /h {0}", pdfFileName);
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            //(It won't be hidden anyway... thanks Adobe!)
            info.UseShellExecute = false;

            Process p = Process.Start(info);
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            int counter = 0;
            while (!p.HasExited)
            {
                System.Threading.Thread.Sleep(1000);
                counter += 1;
                if (counter == 5) break;
            }
            if (!p.HasExited)
            {
                p.CloseMainWindow();
                p.Kill();
            }
        }



    }
}
