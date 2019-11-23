using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WallpaperEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        enum nextArg { blank, scan_folder, backup_folder, destination_folder, background };

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow wnd = new MainWindow();

            //parse the command line args
            bool cancel = false;
            nextArg arg = nextArg.blank;
            foreach (String next in e.Args)
            {
                //remove any "'s 
                String thisStr = next.Replace("\"", "");

                //modes
                if (arg == nextArg.blank && thisStr == nextArg.backup_folder.ToString())
                {
                    arg = nextArg.backup_folder;
                }
                else if (arg == nextArg.blank && thisStr == nextArg.destination_folder.ToString())
                {
                    arg = nextArg.destination_folder;
                }
                else if (arg == nextArg.blank && thisStr == nextArg.scan_folder.ToString())
                {
                    arg = nextArg.scan_folder;
                }

                //direct
                else if (arg == nextArg.blank && thisStr == nextArg.background.ToString())
                {
                    wnd.runInBackground = true;
                }

                //Other, unexpected arg
                else if (arg == nextArg.blank)
                {
                    MessageBox.Show("wrong argument " + thisStr + ". scan_folder, backup_folder, destination_folder, recurse, background"); //TODO - proper help
                    cancel = true;

                }
                //mode values
                else if (arg == nextArg.backup_folder)
                {
                    wnd.backupDirectory = new System.IO.DirectoryInfo(thisStr);
                    arg = nextArg.blank;
                }
                else if (arg == nextArg.destination_folder)
                {
                    wnd.destinationDirectories.Add (new System.IO.DirectoryInfo(thisStr));
                    arg = nextArg.blank;
                }
                else if (arg == nextArg.scan_folder)
                {
                    wnd.scanDirectory = new System.IO.DirectoryInfo(thisStr);
                    arg = nextArg.blank;
                }


            }
            if (cancel)
            {
                this.Shutdown();
            }
            else
            {
                wnd.postSetup();
            }

        }


    }
}