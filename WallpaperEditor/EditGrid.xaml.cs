using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WallpaperEditor
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class EditGrid : Window
    {
        public bool cancelled = false;
        public enum editMode {none, crop, stretch, stretchSplit, mirror};
        public enum selectedDirection { none, W,D,X,A};
        private editMode mode = editMode.none;
        private selectedDirection selectedDir = selectedDirection.none;
       
        private MainWindow parentWindow;
        

        public EditGrid(MainWindow parentWindow , editMode mode)
        {
            this.DataContext = this;
            this.mode = mode;
            this.parentWindow = parentWindow;
            
            InitializeComponent();

            //get the top left of the parent
            Point tl = parentWindow.PointToScreen(new Point(0, 0));

            this.Left = tl.X + parentWindow.ActualWidth - this.Width - 23;
            this.Top = tl.Y + parentWindow.ActualHeight - this.Height - 70;
            
            switch (mode)
            {
                case editMode.crop:
                    this.grid_stretch_mirror.Visibility = Visibility.Hidden;
                    this.btn_OK.IsEnabled = false;
                    break;
                //any of these, hide the diagonals
                case editMode.stretch:
                case editMode.mirror:
                case editMode.stretchSplit:
                    //hide the QEZCS
                    edit_Q.Visibility = Visibility.Hidden;
                    edit_E.Visibility = Visibility.Hidden;
                    edit_S.Visibility = Visibility.Hidden;
                    edit_Z.Visibility = Visibility.Hidden;
                    edit_C.Visibility = Visibility.Hidden;
                    break;
                default:
                    MessageBox.Show("Something wrong, no default value");
                    break;
            }
            

        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            switch (mode)
            {
                case editMode.stretch:
                    parentWindow.process_Stretch(selectedDir, (int)slider_source_px.Value, (int)slider_dest_px.Value);
                    break;
                case editMode.stretchSplit:
                    parentWindow.process_stretch_split(selectedDir, (int)slider_source_px.Value, (int)slider_dest_px.Value);
                    break;
                case editMode.mirror:
                    parentWindow.process_Mirror(selectedDir, (int)slider_source_px.Value, (int)slider_dest_px.Value);
                    break;
            }

            this.Close();


        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            cancelled = true;
            this.Close();
            parentWindow.editorClosed();
        }

        private void btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            String buttonName = button.Name;

            switch (mode)
            {
                case editMode.crop:
                    //Work out starting and finishing V pos
                    parentWindow.process_Crop(buttonName);
                    this.Close();
                    break;
                case editMode.stretch:
                case editMode.mirror:
                case editMode.stretchSplit:
                    //flag the direction as selected, deselect others
                    edit_W.Background = Brushes.LightGray;
                    edit_A.Background = Brushes.LightGray;
                    edit_D.Background = Brushes.LightGray;
                    edit_X.Background = Brushes.LightGray;

                    btn_OK.Focus();

                    switch  (buttonName)
                    {
                        //TODO - if we have selected something that doesnt need stretching, 
                        case "edit_W":
                            edit_W.Background = Brushes.Green;
                            selectedDir = selectedDirection.W;
                            slider_source_px.Maximum = parentWindow.res_currentY;
                            slider_source_px.Value = parentWindow.res_targetYresolution_Resize - parentWindow.res_currentY; //i.e. copy how much we need to fill by default
                            slider_dest_px.Maximum = parentWindow.res_targetYresolution_Resize - parentWindow.res_currentY;
                            slider_dest_px.Value = parentWindow.res_targetYresolution_Resize - parentWindow.res_currentY; //i.e. copy how much we need to fill by default

                            break;

                        case "edit_A":
                            edit_A.Background = Brushes.Green;
                            selectedDir = selectedDirection.A;

                            slider_source_px.Maximum = parentWindow.res_currentX;
                            slider_source_px.Value = parentWindow.res_targetXresolution_Resize - parentWindow.res_currentX; //i.e. copy how much we need to fill by default
                            slider_dest_px.Maximum = parentWindow.res_targetXresolution_Resize - parentWindow.res_currentX;
                            slider_dest_px.Value = parentWindow.res_targetXresolution_Resize - parentWindow.res_currentX; //i.e. copy how much we need to fill by default

                            break;

                        case "edit_D":
                            edit_D.Background = Brushes.Green;
                            selectedDir = selectedDirection.D;

                            slider_source_px.Maximum = parentWindow.res_currentX;
                            slider_source_px.Value = parentWindow.res_targetXresolution_Resize - parentWindow.res_currentX; //i.e. copy how much we need to fill by default
                            slider_dest_px.Maximum = parentWindow.res_targetXresolution_Resize - parentWindow.res_currentX;
                            slider_dest_px.Value = parentWindow.res_targetXresolution_Resize - parentWindow.res_currentX; //i.e. copy how much we need to fill by default

                            break;

                        case "edit_X":
                            edit_X.Background = Brushes.Green;
                            selectedDir = selectedDirection.X;

                            slider_source_px.Maximum = parentWindow.res_currentY;
                            slider_source_px.Value = parentWindow.res_targetYresolution_Resize - parentWindow.res_currentY; //i.e. copy how much we need to fill by default
                            slider_dest_px.Maximum = parentWindow.res_targetYresolution_Resize - parentWindow.res_currentY;
                            slider_dest_px.Value = parentWindow.res_targetYresolution_Resize - parentWindow.res_currentY; //i.e. copy how much we need to fill by default

                            break;


                    }
                    
                    break;

            }
        }


        private void Window_Activated(object sender, EventArgs e)
        {
            this.Focus();
            edit_W.Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.InputBindings.Clear();
            parentWindow.editorClosed();
        }
    }
}
