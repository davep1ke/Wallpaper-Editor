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
        double allowableDifference = .00001;

        private WriteableBitmap imageBitmap = BitmapOperations.createDummyBitmap();

        //hold this in case we want to send keypresses down. 
        //private EditGrid editor = null;
        private bool editorOpen = false;

        //various directories 
        public DirectoryInfo backupDirectory = null;
        public DirectoryInfo scanDirectory = null;
        public List<DirectoryInfo> destinationDirectories = new List<DirectoryInfo>();

        public MainWindow()
        {
            //TODO: Error handling in here. 
            
            InitializeComponent();
            syncLocalSettings();

            //Loop through our desination dirs. 

        }

        private void syncLocalSettings()
        {
            backupDirectory = new DirectoryInfo(Properties.Settings.Default.backupDirectoryPath);
            scanDirectory = new DirectoryInfo(Properties.Settings.Default.scanDirectoryPath);
            int pos = 1;

            this.grid_destinations.Children.Clear();

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


        public void process_stretch_split(EditGrid.selectedDirection selectedDir, int sourceAmount, int destAmount)
        {
            //work out how much we should stretch / take from each side
            int newSourceAmount = sourceAmount / 2;
            int newDestAmount = destAmount / 2;

            //make sure if we are a pixel under, we go over instead. Can be cropped easily after? 
            if (newSourceAmount *2 < sourceAmount) { newSourceAmount++; }
            if (newDestAmount * 2 < destAmount) { newDestAmount++; }

            var oppositeDir = selectedDir;
            switch (selectedDir)
            {
                case EditGrid.selectedDirection.A: oppositeDir = EditGrid.selectedDirection.D; break;
                case EditGrid.selectedDirection.D: oppositeDir = EditGrid.selectedDirection.A; break;
                case EditGrid.selectedDirection.X: oppositeDir = EditGrid.selectedDirection.W; break;
                case EditGrid.selectedDirection.W: oppositeDir = EditGrid.selectedDirection.X; break;

            }


            //split the source and dest amounts, call process_Stretch twice. 
            process_Stretch(selectedDir, newSourceAmount, newDestAmount);
            process_Stretch(oppositeDir, newSourceAmount, newDestAmount);
            

        }



        public void process_Stretch(EditGrid.selectedDirection selectedDir, int sourceAmount, int destAmount)
        {

            //create a new writeable bitmap with the appropriate dims (i.e. orig + destamount)
            int newY = imageBitmap.PixelHeight;
            int newX = imageBitmap.PixelWidth;

            
            if (selectedDir == EditGrid.selectedDirection.W || selectedDir == EditGrid.selectedDirection.X) { newY += destAmount; }
            else { newX += destAmount; }

            WriteableBitmap newCanvas = BitmapOperations.createCanvas(newX, newY, imageBitmap);

            //place this on our canvas based on the direction
            Int32Rect sourceDestPositon = new Int32Rect(0, 0, imageBitmap.PixelWidth, imageBitmap.PixelHeight); //where are we placing our source image?
            Int32Rect copySourceDims = new Int32Rect(0, 0, imageBitmap.PixelWidth, imageBitmap.PixelHeight); //where we are taking pixels from on our original image
            Int32Rect copyDestDims = new Int32Rect(0, 0, newX, newY); //where we are stetch/placing the to

            //set the various source and destination rects
            switch (selectedDir)
            {
                case EditGrid.selectedDirection.A:
                    sourceDestPositon.X = destAmount;
                    copySourceDims.Width = sourceAmount;
                    copyDestDims.Width = destAmount + sourceAmount;
                break;
                case EditGrid.selectedDirection.D:
                    //sourceDestPositon.Width -= destAmount;
                    copySourceDims.X = imageBitmap.PixelWidth - sourceAmount;
                    copySourceDims.Width = sourceAmount;
                    copyDestDims.X = imageBitmap.PixelWidth - sourceAmount;
                    copyDestDims.Width = destAmount +sourceAmount;

                    break;
                case EditGrid.selectedDirection.W:
                    sourceDestPositon.Y = destAmount;
                    copySourceDims.Height = sourceAmount;
                    copyDestDims.Height = destAmount + sourceAmount;
                    break;
                case EditGrid.selectedDirection.X:
                    //sourceDestPositon.Height -= destAmount;
                    copySourceDims.Y = imageBitmap.PixelHeight - sourceAmount;
                    copySourceDims.Height = sourceAmount;
                    copyDestDims.Y = imageBitmap.PixelHeight - sourceAmount;
                    copyDestDims.Height = destAmount + sourceAmount;
                    break;
                    
            }
            
            //copy the original, unaltered, to the right place in our target
            newCanvas = BitmapOperations.copyArea(imageBitmap, newCanvas, null, sourceDestPositon);
            //now copy/stretch the appropriate section from our source and paste over the top
            newCanvas = BitmapOperations.copyArea(imageBitmap, newCanvas, copySourceDims, copyDestDims);

            
            refreshImage(newCanvas);

        }

        public void editorClosed()
        {
            editorOpen = false;
        }

        public void postSetup()
        {
            populate_Treeview_Drives();

            //go to the scan folder
            if (scanDirectory != null)
            {
                expandFolder(scanDirectory, true);
            }

            /*if (backupDirectory != null)
            {
                txtBackupFolder.Text = backupDirectory.FullName;
            }
            */
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
            try
            {
                //load the image into the local bitmap.
                if (FileList.SelectedItem != null && FolderBrowser.SelectedItem != null)
                {
                    TreeViewItem selectedFolderItem = FolderBrowser.SelectedItem as TreeViewItem;
                    DirectoryInfo selectedFolder = (DirectoryInfo)selectedFolderItem.Tag;
                    string fullpath = selectedFolder.FullName + "\\" + FileList.SelectedItem.ToString();

                    Uri uri = new Uri(fullpath);

                    LoadImage(uri);

                   
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }

        private void LoadImage(Uri uri)
        {
            BitmapImage bitmapImageSource = new BitmapImage();
            bitmapImageSource.BeginInit();
            bitmapImageSource.UriSource = uri;
            bitmapImageSource.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImageSource.EndInit();
            try
            {
                WriteableBitmap newImageBitmap = new WriteableBitmap(bitmapImageSource);
                refreshImage(newImageBitmap);
            }
            catch
            {
                refreshImage(null);
                setStatus("Error loading image " + uri.AbsolutePath);
            }

            
        }



        /// <summary>
        /// Refresh the image display, and update resolutions fields.
        /// </summary>
        private void refreshImage(WriteableBitmap newImage)
        {

            //TODO - display image. 
            if (imageBitmap != null)
            {
                //try force it to Gfree up memory. This bit should do nothing.  
                imageBitmap.Freeze();
                imageBitmap = null;   
            }
            if (newImage != null)
            { 
                newImage.Freeze();
                imageBitmap = newImage;
                Image_Preview.Source = imageBitmap;

                Tag_ImageDims.Text = imageBitmap.PixelWidth + "x" + imageBitmap.PixelHeight;
                
                //Crop Target
                Tag_CropTarget.Text = res_targetXresolution_Crop + " x " + res_targetYresolution_Crop;
                if (imageBitmap.PixelWidth == res_targetXresolution_Crop && imageBitmap.PixelHeight == res_targetYresolution_Crop)
                {
                    Tag_CropTarget.Foreground = Brushes.Green;
                }
                else if (imageBitmap.PixelWidth == res_targetXresolution_Crop || imageBitmap.PixelHeight == res_targetYresolution_Crop)
                {
                    Tag_CropTarget.Foreground = Brushes.Orange;
                }
                else
                {
                    Tag_CropTarget.Foreground = Brushes.Red;
                }

                
                //Expand Target
                Tag_ExpandTarget.Text = res_targetXresolution_Resize + " x " + res_targetYresolution_Resize;
                if (imageBitmap.PixelWidth == res_targetXresolution_Resize && imageBitmap.PixelHeight == res_targetYresolution_Resize)
                {
                    Tag_ExpandTarget.Foreground = Brushes.Green;
                }
                else if (imageBitmap.PixelWidth == res_targetXresolution_Resize || imageBitmap.PixelHeight == res_targetYresolution_Resize)
                {
                    Tag_ExpandTarget.Foreground = Brushes.Orange;
                }
                else
                {
                    Tag_ExpandTarget.Foreground = Brushes.Red;
                }


                if (res_xRatio != (int)res_xRatio)
                {
                    Tag_ImageDims_OK.Text = "Bad X Multiplier";
                    Tag_ImageDims_OK.Foreground = Brushes.Red;
                }
                else if (res_yRatio != (int)res_yRatio)
                {
                    Tag_ImageDims_OK.Text = "Bad Y Multiplier";
                    Tag_ImageDims_OK.Foreground = Brushes.Red;
                }
                else if (Math.Abs( res_correctRatio - res_currentRatio) >= allowableDifference) //doubles - can be slightly off due to floating point
                {
                    Tag_ImageDims_OK.Text = "Bad Ratio";
                    Tag_ImageDims_OK.Foreground = Brushes.Red;
                }
                else if (imageBitmap.PixelWidth < Properties.Settings.Default.screenResMinX)
                {
                    Tag_ImageDims_OK.Text = "Low Resolution";
                    Tag_ImageDims_OK.Foreground = Brushes.Red;
                }
                else
                {
                    Tag_ImageDims_OK.Text = "OK";
                    Tag_ImageDims_OK.Foreground = Brushes.Green;
                }

                //set the target crop display


            }
        }

        #endregion


        #region image Resolution vals
        public double res_currentRatio
        {
            get
            {
                return (double)imageBitmap.PixelWidth / (double)imageBitmap.PixelHeight;
            }
        }
        public double res_xRatio
        {
            get
            {
                return (double)imageBitmap.PixelWidth / (double)Properties.Settings.Default.screenResXmultiplier;
            }
        }
        public double res_yRatio
        {
            get
            {
                return (double)imageBitmap.PixelHeight / (double)Properties.Settings.Default.screenResYmultiplier;
            }
        }

        public double res_correctRatio
        {
            get
            {
                return (double)Properties.Settings.Default.screenResXmultiplier / (double)Properties.Settings.Default.screenResYmultiplier;
            }
        }
        /// <summary>
        /// work out if we should treat the X resolution as the master, or the Y. The "master" is the one closest to the right dims, that we will base this off of. 
        /// </summary>
        public bool res_masterIsWidth
        {
            get
            {
                if (res_currentRatio > res_correctRatio)
                { return true; } //X / width is master
                else { return false; } //Y / height is master

            }
        }



        public int res_targetXresolution_Resize
        {
            get
            {
                if (res_masterIsWidth) //i.e. this is the master
                {
                    //should be current X size, rounded down to the nearest dimension multiplier (e.g. x16)
                    int baseX = (int)( (double)imageBitmap.PixelWidth / (double)Properties.Settings.Default.screenResXmultiplier);
                    return baseX * Properties.Settings.Default.screenResXmultiplier;
                }
                else
                {
                    //Y is master - so work that out first. 
                    int masterRes = res_targetYresolution_Resize;
                    return (masterRes / Properties.Settings.Default.screenResYmultiplier) * Properties.Settings.Default.screenResXmultiplier;
                }
            }
        }

        public int res_targetYresolution_Resize
        {
            get
            {
                if (!res_masterIsWidth) //i.e. this is the master
                {
                    //should be current Y size, rounded down to the nearest dimension multiplier (e.g. x16)
                    int baseX = (int)((double)imageBitmap.PixelHeight / (double)Properties.Settings.Default.screenResYmultiplier);
                    return baseX * Properties.Settings.Default.screenResYmultiplier;
                }
                else
                {
                    //Y is master - so work that out first. 
                    int masterRes = res_targetXresolution_Resize;
                    return (masterRes / Properties.Settings.Default.screenResXmultiplier) * Properties.Settings.Default.screenResYmultiplier;
                }
            }
        }

        public int res_currentX
        {
            get
            {
                return imageBitmap.PixelWidth;
            }
        }

        public int res_currentY
        {
            get
            {
                return imageBitmap.PixelHeight;
            }
        }

        //For cropping, swap around the master (as we want to crop against the shortest side). 
        public int res_targetXresolution_Crop
        {
            get
            {
                if (!res_masterIsWidth) //i.e. this is the master
                {
                    //should be current X size, rounded down to the nearest dimension multiplier (e.g. x16)
                    int baseX = (int)((double)imageBitmap.PixelWidth / (double)Properties.Settings.Default.screenResXmultiplier);
                    return baseX * Properties.Settings.Default.screenResXmultiplier;
                }
                else
                {
                    //Y is master - so work that out first. 
                    int masterRes = res_targetYresolution_Crop;
                    return (masterRes / Properties.Settings.Default.screenResYmultiplier) * Properties.Settings.Default.screenResXmultiplier;
                }
            }
        }


        public int res_targetYresolution_Crop
        {
            get
            {
                if (res_masterIsWidth) //i.e. this is the master
                {
                    //should be current Y size, rounded down to the nearest dimension multiplier (e.g. x16)
                    int baseX = (int)((double)imageBitmap.PixelHeight / (double)Properties.Settings.Default.screenResYmultiplier);
                    return baseX * Properties.Settings.Default.screenResYmultiplier;
                }
                else
                {
                    //Y is master - so work that out first. 
                    int masterRes = res_targetXresolution_Crop;
                    return (masterRes / Properties.Settings.Default.screenResXmultiplier) * Properties.Settings.Default.screenResYmultiplier;
                }
            }
        }



       

        /*// <summary>
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
        }*/

        /*// <summary>
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
        */


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


        /*#region backups

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
            */
        
        #region rightbuttons


/*        private void btn_SaveNextEmpty_Click(object sender, RoutedEventArgs e)
        {
            saveImage();
            selectNextItem(true, false);
        }*/

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

        private void setStatus(string text)
        {
            Tag_status.Text = text;
        }

        private void btn_Crop_Click(object sender, RoutedEventArgs e)
        {
            if (editorOpen ==false)
            {
                //show the help
                setStatus("Crop using dialog");
                //InputBindingCollection defaultBindings = mainWindowGrid.InputBindings;
                //mainWindowGrid.InputBindings.Clear();
                EditGrid editor = new EditGrid(this, EditGrid.editMode.crop);
                editor.Show();
                editor.Focus();
                editorOpen = true;
            }

        }

        private void btn_Stretch_Click(object sender, RoutedEventArgs e)
        {
            //was this x1 or x2
            EditGrid.editMode stretchmode = EditGrid.editMode.none;
            Button button = (Button)sender;
            if (button == btn_Stretch_x1)
            {
                stretchmode = EditGrid.editMode.stretch;
            }
            else if (button == btn_Stretch_x2)
            {
                stretchmode = EditGrid.editMode.stretchSplit;
            }
            else
            {
                MessageBox.Show("Error!");
            }

            if (editorOpen == false)
            {
                //show the help
                setStatus("Stretch using dialog");
                EditGrid editor = new EditGrid(this, stretchmode);
                editor.Show();
                editor.Focus();
                editorOpen = true;
            }
        }

        private void btn_Mirror_Click(object sender, RoutedEventArgs e)
        {
            if (editorOpen == false)
            {
                //show the help
                setStatus("Mirror using dialog");
                EditGrid editor = new EditGrid(this, EditGrid.editMode.mirror);
                editor.Show();
                editor.Focus();
                editorOpen = true;

            }
        }
        private void btn_External_Edit(object sender, RoutedEventArgs e)
        {
            //write file to temp
            saveImage();

            //open editor
            ProcessStartInfo StartInfo = new ProcessStartInfo(Properties.Settings.Default.externalEditor);
            StartInfo.Arguments = Properties.Settings.Default.testImageLocation;
            Process myProcess = new Process();
            myProcess.StartInfo = StartInfo;
            myProcess.Start();
            myProcess.WaitForExit();


            //reload from temp
            LoadImage(new Uri(Properties.Settings.Default.testImageLocation));


        }


        /*private int res_stride
        {
            get
            {
                return imageBitmap.PixelWidth * (imageBitmap.Format.BitsPerPixel + 7) / 8;
            }
        }

        private int res_new_stride_crop
        {
            get
            {
                return (res_targetXresolution_Crop * (imageBitmap.Format.BitsPerPixel + 7) )/ 8;
            }
        }*/





        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void btn_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        #endregion

        public void process_Crop(String buttonName)
        {
            

            int pos_T = 0;
            //int pos_B = imageBitmap.PixelHeight;
            int difY = imageBitmap.PixelHeight - res_targetYresolution_Crop;

            //top aligned
            if (buttonName == "edit_Q" || buttonName == "edit_W" || buttonName == "edit_E")
            {
                //pos_B = res_targetYresolution_Crop;
            }
            //middle aligned
            else if (buttonName == "edit_A" || buttonName == "edit_S" || buttonName == "edit_D")
            {

                int difPerSide = difY / 2;
                pos_T = difPerSide;
            }
            //bottom aligned
            else { pos_T = difY; }


            //now do same for horizontal

            int pos_L = 0;
           // int pos_R = imageBitmap.PixelWidth;
            int difX = imageBitmap.PixelWidth - res_targetXresolution_Crop;

            //left aligned
            if (buttonName == "edit_Q" || buttonName == "edit_A" || buttonName == "edit_Z")
            {
//pos_R = res_targetXresolution_Crop;
            }
            //middle aligned
            else if (buttonName == "edit_W" || buttonName == "edit_S" || buttonName == "edit_X")
            {

                int difPerSide = difX / 2;
                pos_L = difPerSide;

            }
            //right aligned
            else { pos_L = difX; }

            CroppedBitmap cb = new CroppedBitmap(imageBitmap, new Int32Rect(pos_L, pos_T, res_targetXresolution_Crop, res_targetYresolution_Crop));

            WriteableBitmap target = new WriteableBitmap(cb);

            /*
            

            // Create WriteableBitmap to copy the pixel data to.      
            WriteableBitmap target = new WriteableBitmap(
              res_targetXresolution_Crop,
              res_targetYresolution_Crop,
              imageBitmap.DpiX, imageBitmap.DpiY,
              imageBitmap.Format, null);


            byte[] bitmapData = new byte[res_new_stride_crop * res_targetYresolution_Crop];

            // Copy source image pixels to the data array
            imageBitmap.CopyPixels(new Int32Rect(pos_L, pos_T, pos_R, pos_B), bitmapData, res_stride, 0);

            
            // Write the pixel data to the WriteableBitmap.
            target.WritePixels(
              new Int32Rect(0, 0, res_targetXresolution_Crop, res_targetYresolution_Crop),
              bitmapData, res_new_stride_crop, 0); //res_new_stride_crop

            //            target.WritePixels(            new Int32Rect(pos_L, pos_T, pos_R, pos_B),              getBitmapArray(), res_stride, 0);

    */
           
            refreshImage(target);
            
            setStatus("Applied crop operation");         

        }

        private void btn_Options_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsform = new Settings();
            settingsform.ShowDialog();
            syncLocalSettings();
        }

        private void btn_Discard_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedFolderItem = FolderBrowser.SelectedItem as TreeViewItem;
            DirectoryInfo selectedFolder = (DirectoryInfo)selectedFolderItem.Tag;
            string fullpath = selectedFolder.FullName + "\\" + FileList.SelectedItem.ToString();

            //move the original into the discard folder.
            FileInfo fi = new FileInfo(fullpath);
            fi.MoveTo(Properties.Settings.Default.throwawayFolder + @"\" + fi.Name);

            //Refresh cached filelist. Keep current posn.
            populateFileList(true);
        }


    }
}
