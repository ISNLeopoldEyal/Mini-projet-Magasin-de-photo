using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
        static string openedFileName = "";

        public MainWindow()
        {
            InitializeComponent();
            ChangeOrientationOfFiltersTlb();
            //display_image.Source = new BitmapImage(new Uri("D:\\All Visual Studio Projects\\Magasin De Photo\\Magasin De Photo\\Images\\colette.png"));
            //openedFileUri = "D:\\All Visual Studio Projects\\Magasin De Photo\\Magasin De Photo\\Images\\colette.png";
            //openedFileName = "colette.png";
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
            filter5.LayoutTransform = rotate1;

            filter4.Content = "N&B";
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
                Filter = "PNG Files (*.png)|*.png|JEG Files (*.jpg)|*.jpg"
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                openedFileUri = openFileDialog.FileName;

                BitmapImage _bmpi = new BitmapImage();
                _bmpi.BeginInit();
                _bmpi.CacheOption = BitmapCacheOption.OnLoad;
                _bmpi.UriSource = new Uri(openedFileUri);
                _bmpi.EndInit();


                display_image.Source = _bmpi;

                //display_image.Source = new BitmapImage(new Uri(openedFileUri));
            }
        }

        private void CloseImage(object sender, RoutedEventArgs e)
        {
            display_image.Source = null;
            openedFileUri = "";
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            //if (display_image.Source != null)
            //{
            //    SaveActualFile();
            //}
            //else { return; }
        }

        private void SaveActualFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = openedFileUri
            };

            File.Replace(openedFileUri, openedFileUri, null);

            //PngBitmapEncoder encoder = new PngBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(new Uri(openedFileUri)));
            //using (var stream = File.(openedFileUri))
            //{
            //    encoder.Save(stream);
            //}
        }

        private void SaveAsFileFromDialog(object sender, RoutedEventArgs e)
        {
            if (display_image.Source != null)
            {
                SaveAsFile();
            }
            else { return; }
        }

        private void SaveAsFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                DefaultExt = ".png",
                Filter = "PNG Files (*.png)|*.png|JEG Files (*.jpg)|*.jpg"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(new Uri(openedFileUri)));
                using (var stream = saveFileDialog.OpenFile())
                {
                    encoder.Save(stream);
                }
            }
        }
    }
}
