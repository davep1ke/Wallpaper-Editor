using System.Diagnostics;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using WinForms = System.Windows.Forms;
//using WinControl = System.Windows.Forms.Control;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using System.Net;

namespace WallpaperEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //class var to prevent memory leakage
        private BitmapImage imageBitmap = new BitmapImage();

        //various directories 
        public DirectoryInfo backupDirectory = null;
        public DirectoryInfo scanDirectory = null;
        public List<DirectoryInfo> destinationDirectories = new List<DirectoryInfo>();

        public MainWindow()
        {
            //TODO: Error handling in here. 
            backupDirectory = new DirectoryInfo(Properties.Settings.Default.backupDirectoryPath);
            scanDirectory = new DirectoryInfo(Properties.Settings.Default.scanDirectoryPath);
            

            int pos = 1;
                
            InitializeComponent();

            //Loop through our desination dirs. 
            foreach (String s in Properties.Settings.Default.destinationDirectories)
            {

                DirectoryInfo di = new DirectoryInfo(s);
                destinationDirectories.Add(di);

                //create a new button, add it to form. Try line up below the "Save&Next" button

                Button button = new Button();
                button.Content = "_" + pos + " - " + di.Name;
                button.Margin = new Thickness(1);
                button.Padding = new Thickness(10);

                button.SetValue(Grid.ColumnProperty, 0);
                button.SetValue(Grid.RowProperty, pos);

                button.Tag = di;

                button.Click += new RoutedEventHandler(btn_dynamic_export);

                //    Grid.SetRow(button, pos);
                //  Grid.SetColumn(button, 0);

                this.grid_destinations.Children.Add(button);

                pos++;

            }
        }

        public void postSetup()
        {
            populate_Treeview_Drives();

            //go to the scan folder
            if (scanDirectory != null)
            {
                expandFolder(scanDirectory, true);
            }

            if (backupDirectory != null)
            {
                txtBackupFolder.Text = backupDirectory.FullName;
            }

            this.Show();


        }

        #region FolderBrowser

        /// <summary>
        /// Try navigate to a folder recursively, from a lowest level folder updwards. Select it if neccessary
        /// </summary>
        /// <param name="di"></param>
        public void expandFolder(DirectoryInfo di, bool selectFolder)
        {
            //Expand the folder above, if there is one
            if (di.Parent != null)
            {
                expandFolder(di.Parent, false);
            }
            //now expand the actual folder, which should be exposed. Lat call should be =true(if passed in as original param) and the folder should be selected. 
            tryExpand(FolderBrowser.Items, di.Name, selectFolder);
        }

        public void tryExpand(ItemCollection rootCollection, string name, bool select = false)
        {

            //loop through the items and try and expand / populate the item if one exists
            foreach (TreeViewItem i in rootCollection)
            {
                if (i.Header.ToString().ToUpper() == name.ToUpper())
                {
                    if (select)
                    {

                        i.IsSelected = true;
                        SetSelected(FolderBrowser, i);

                    }
                    else { i.IsExpanded = true; }

                }
                //if it is just our "loading.." message, drop out
                if (!((i.Items.Count == 1) && (i.Items[0] is string)))
                {
                    tryExpand(i.Items, name, select);
                }
            }

            FolderBrowser.UpdateLayout();
        }


        static private bool SetSelected(ItemsControl parent, object child)
        {

            if (parent == null || child == null)
            {
                return false;
            }

            TreeViewItem childNode = parent.ItemContainerGenerator
                .ContainerFromItem(child) as TreeViewItem;

            if (childNode != null)
            {
                childNode.Focus();
                return childNode.IsSelected = true;
            }

            if (parent.Items.Count > 0)
            {
                foreach (object childItem in parent.Items)
                {
                    ItemsControl childControl = parent
                        .ItemContainerGenerator
                        .ContainerFromItem(childItem)
                        as ItemsControl;

                    if (SetSelected(childControl, child))
                    {
                        return true;
                    }
                }
            }

            return false;
        }




        public void populate_Treeview_Drives()
        {


            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo driveInfo in drives)
                FolderBrowser.Items.Add(CreateFolderTreeItem(driveInfo));

        }

        private TreeViewItem CreateFolderTreeItem(object o)
        {
            TreeViewItem item = new TreeViewItem();
            item.Expanded += new RoutedEventHandler(TreeViewItem_Expanded);
            item.Selected += new RoutedEventHandler(TreeViewItem_Selected);
            item.Header = o.ToString();
            item.Tag = o;
            item.Items.Add("Loading...");
            return item;
        }

        public void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.Source as TreeViewItem;
            if ((item.Items.Count == 1) && (item.Items[0] is string))
            {
                item.Items.Clear();

                DirectoryInfo expandedDir = null;
                if (item.Tag is DriveInfo)
                    expandedDir = (item.Tag as DriveInfo).RootDirectory;
                if (item.Tag is DirectoryInfo)
                    expandedDir = (item.Tag as DirectoryInfo);
                try
                {
                    foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
                        item.Items.Add(CreateFolderTreeItem(subDir));
                }
                catch { }
            }
        }


        public void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            populateFileList();
        }

        #endregion

        #region fileList
        public void populateFileList(bool keepPos = false)
        {
            int currentPos = FileList.SelectedIndex;

            TreeViewItem selectedItem = FolderBrowser.SelectedItem as TreeViewItem;
            if (selectedItem != null && selectedItem.Tag is DirectoryInfo)
            {
                DirectoryInfo di = selectedItem.Tag as DirectoryInfo;
                FileList.ItemsSource = di.EnumerateFiles("*");

            }

            if (keepPos && currentPos > 0 && currentPos < FileList.Items.Count)
            {
                FileList.SelectedItem = FileList.Items[currentPos];
                FileList.SelectedIndex = currentPos;
                FileList.ScrollIntoView(FileList.SelectedItem);
                FileList.SelectedValue = FileList.SelectedItem;
                
                FileList.UpdateLayout(); // Pre-generates item containers 

                var listBoxItem = (ListBoxItem)FileList
                    .ItemContainerGenerator
                    .ContainerFromItem(FileList.SelectedItem);

                listBoxItem.Focus();

            }

        }
        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //load the image into the local bitmap.
            if (FileList.SelectedItem != null && FolderBrowser.SelectedItem != null)
            {
                TreeViewItem selectedFolderItem = FolderBrowser.SelectedItem as TreeViewItem;
                DirectoryInfo selectedFolder = (DirectoryInfo)selectedFolderItem.Tag;
                string fullpath = selectedFolder.FullName + "\\" + FileList.SelectedItem.ToString();

                Uri uri = new Uri(fullpath);

                imageBitmap = new BitmapImage();
                imageBitmap.BeginInit();
                imageBitmap.UriSource = uri;
                imageBitmap.CacheOption = BitmapCacheOption.OnLoad;
                imageBitmap.EndInit();

                
                refreshImage();
            }

        }


        /// <summary>
        /// Refresh the image display, and update resolutions fields.
        /// </summary>
        private void refreshImage()
        {
            //TODO - display image. 
            if (imageBitmap != null)
            {
                Image_Preview.Source = imageBitmap;
                Tag_ImageDims.Text = imageBitmap.PixelWidth + "x" + imageBitmap.PixelHeight;
                float correctRatio = (float)Properties.Settings.Default.screenResXmultiplier / (float)Properties.Settings.Default.screenResYmultiplier;
                float currentRatio = (float)imageBitmap.PixelWidth / (float)imageBitmap.PixelHeight;
                float xRatio = (float)imageBitmap.PixelWidth / (float)Properties.Settings.Default.screenResXmultiplier;
                float yRatio = (float)imageBitmap.PixelHeight / (float)Properties.Settings.Default.screenResYmultiplier;

                
                if (xRatio != (int)xRatio)
                {
                    Tag_ImageDims_OK.Text = "Bad X Multiplier";
                    Tag_ImageDims_OK.Foreground = Brushes.Red;
                }
                else if (yRatio != (int)yRatio)
                {
                    Tag_ImageDims_OK.Text = "Bad Y Multiplier";
                    Tag_ImageDims_OK.Foreground = Brushes.Red;
                }
                else if (correctRatio != currentRatio)
                {
                    Tag_ImageDims_OK.Text = "Bad Ratio";
                    Tag_ImageDims_OK.Foreground = Brushes.Red;
                }
                else if (imageBitmap.PixelWidth < Properties.Settings.Default.screenResMinX )
                {
                    Tag_ImageDims_OK.Text = "Low Resolution";
                    Tag_ImageDims_OK.Foreground = Brushes.Red;
                }
                else
                {
                    Tag_ImageDims_OK.Text = "OK";
                    Tag_ImageDims_OK.Foreground = Brushes.Green;
                }

            }
        }

        /// <summary>
        /// runs the scan in the background, then pops open the window when it finds a file without an image. 
        /// </summary>
        public void backgroundScan()
        {
            if (FileList.Items.Count > 0)
            {
                //select our first file, and scan for a file without an image
                FileList.SelectedIndex = 0;
                if (selectNextItem(true, true))
                {
                    this.Show();
                }
                else
                {
                    //no file found
                    this.Close();
                }
            }
            //no files, close
            else
            {
                this.Close();
            }
        }

        /// <summary>
        /// Scans for the next file. Returns false if no files found
        /// </summary>
        /// <param name="missingArt">Only stops when we find a file with missing art</param>
        /// <param name="startAtOne">Check the current file as well</param>
        public bool selectNextItem(bool missingArt, bool startAtZero)
        {
            int currentIndex = FileList.SelectedIndex;


            if (currentIndex >= FileList.Items.Count - 1)
            {
                return false;
            }
            else
            {
                //TODO: Load next image.


            }
            //return true if we found a file, false if we didnt. 
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputPath">Diretory and filename to export to. Leave blank for temp location </param>
        private void saveImage(string outputPath = null)
        {

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = 100;
            encoder.Frames.Add(BitmapFrame.Create(imageBitmap));

            if (outputPath == null) { outputPath = Properties.Settings.Default.testImageLocation; }

            using (var fileStream = new System.IO.FileStream(outputPath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
            
        }


        private void testImage()
        {
            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = Properties.Settings.Default.setWallpaperExePath;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = Properties.Settings.Default.testImageLocation;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
                // Log error.
            }

        }



        #endregion



        #region backups

        private void btnPickFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fb = new System.Windows.Forms.FolderBrowserDialog();
            if (backupDirectory != null) { fb.SelectedPath = backupDirectory.FullName; }
            fb.Description = "Select a backup location";
            System.Windows.Forms.DialogResult r = fb.ShowDialog();
            if (fb.SelectedPath != null)
            {
                backupDirectory = new DirectoryInfo(fb.SelectedPath);
                txtBackupFolder.Text = backupDirectory.FullName;
            }

        }



        #endregion

        
        private void btn_Exit_Click(object sender, RoutedEventArgs e)
        {


            this.Close();
        }



        #region rightbuttons


        private void btn_SaveNextEmpty_Click(object sender, RoutedEventArgs e)
        {
            saveImage();
            selectNextItem(true, false);
        }

        



        #endregion

        #region sync settings

        //TODO - sync all settings changes from the UI to the settings object
        #endregion

        private void btn_Test_Click(object sender, RoutedEventArgs e)
        {
            saveImage();
            testImage();

        }

        private void btn_dynamic_export(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DirectoryInfo di = (DirectoryInfo)button.Tag;

            TreeViewItem selectedFolderItem = FolderBrowser.SelectedItem as TreeViewItem;
            DirectoryInfo selectedFolder = (DirectoryInfo)selectedFolderItem.Tag;
            string fullpath = selectedFolder.FullName + "\\" + FileList.SelectedItem.ToString();
            
            //save the new image
            saveImage(di.FullName + @"\" + FileList.SelectedItem.ToString());

            //move the original into the backup folder.
            FileInfo fi = new FileInfo(fullpath);
            fi.MoveTo(Properties.Settings.Default.backupDirectoryPath + @"\" + fi.Name);

            //Refresh cached filelist. Keep current posn.
            populateFileList(true);

        }

        private void btn_Crop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_External_Edit(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
