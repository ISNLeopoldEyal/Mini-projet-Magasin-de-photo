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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Magasin_De_Photo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ChangeOrientationOfFiltersTlb();
        }

        private void ChangeOrientationOfFiltersTlb()
        {
            RotateTransform rotate = new RotateTransform(90);
            RotateTransform rotate1 = new RotateTransform(-90);
            tlb_filters.LayoutTransform = rotate;
            filter1.LayoutTransform = rotate1;
            filter2.LayoutTransform = rotate1;
            filter3.LayoutTransform = rotate1;
            filter4.LayoutTransform = rotate1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button _btn = (Button)sender;
            //MessageBox.Show("You clicked the "+_btn.Name+" button", "Clicked a tabbed button");
        }
    }
}
