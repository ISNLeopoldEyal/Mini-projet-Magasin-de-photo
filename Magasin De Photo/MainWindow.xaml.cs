using Microsoft.Win32;
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
        static string openedFileUri = "";

        public MainWindow()
        {
            InitializeComponent();
            ChangeOrientationOfFiltersTlb();
            //display_image.Source = new BitmapImage(new Uri("D:\\All Visual Studio Projects\\Magasin De Photo\\Magasin De Photo\\Images\\open file.png"));
            //openedFileUri = "D:\\All Visual Studio Projects\\Magasin De Photo\\Magasin De Photo\\Images\\open file.png";
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

        private void OpenImageFromDialog(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                DefaultExt = ".png",
                Filter = "PNG Files (*.png)|*.png"
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                // Open document 
                string filename = openFileDialog.FileName;
                openedFileUri = filename;
                display_image.Source = new BitmapImage(new Uri(filename));
            }
        }

        private void CloseImage(object sender, RoutedEventArgs e)
        {
            display_image.Source = null;
        }

        private void SaveFileFromDialog(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                DefaultExt = ".png",
                Filter = "PNG Files (*.png)|*.png",
            };

            if(saveFileDialog.ShowDialog() == true)
            {
                var encoder = new PngBitmapEncoder();
                string path = saveFileDialog.FileName;
                char character = path[path.Length - 1];
                while(character != '\\')
                {
                    path = path.Remove(path.Length - 1);
                    character = path[path.Length - 1];
                }

                path.Remove(path.Length - 1);
                path.Remove(path.Length - 1);

                encoder.Frames.Add(BitmapFrame.Create(new Uri(openedFileUri)));
                using (var stream = saveFileDialog.OpenFile())
                {
                    encoder.Save(stream);
                }
            }
        }
    }
}
