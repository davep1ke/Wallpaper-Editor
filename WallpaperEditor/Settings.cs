using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WallpaperEditor
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = Properties.Settings.Default;
        }
    }
}
