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
        static BitmapImage noFilterImage;
        static BitmapImage negativeFilterImage;
        static BitmapImage blurFilterImage;
        static BitmapImage blacknWhiteFilterImage;

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
                DefaultExt = ".bmp",
                Filter = "BMP Files (*.bmp)|*.bmp"
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                openedFileUri = openFileDialog.FileName;

                BitmapImage _bmpi = new BitmapImage();
                _bmpi.BeginInit();
                _bmpi.CacheOption = BitmapCacheOption.OnLoad;
                _bmpi.UriSource = new Uri(openedFileUri);
                _bmpi.EndInit();
                
                noFilterImage = _bmpi;
                //MessageBox.Show(GetBitArrayFromImage().ToString(), "Bits Array");
                byte[] array = GetBitArrayFromImage();
                noFilterImage = FromArrayToImage(array);


                display_image.Source = noFilterImage;
            }
        }

        private byte[] GetBitArrayFromImage()
        {
            int stride = noFilterImage.PixelWidth * 3;
            if (stride < 1092)
                stride = 1092;
            int size = noFilterImage.PixelHeight * stride;
            byte[] bitsArray = new byte[size];
            noFilterImage.CopyPixels(bitsArray, stride, 00);
            //DisplayBits(bitsArray);


            byte[] goodSizeArray = new byte[noFilterImage.PixelWidth * 3 * noFilterImage.PixelHeight];
            for (int loop = 0; loop < goodSizeArray.Length - 1; loop++)
            {
                goodSizeArray[loop] = (byte)(255 - bitsArray[loop]);
            }

            return goodSizeArray;
        }

        public BitmapImage FromArrayToImage(byte[] array)
        {
            var ms = new MemoryStream(array);          
            BitmapImage biImg = new BitmapImage();
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();
            
            return biImg;
            
        }

        private byte[,] DisplayBits(byte[] bitArray)
        {
            int arrayWidth = noFilterImage.PixelWidth * 3,
                arrayHeight = noFilterImage.PixelHeight,
                index = 0;
            byte[,] twoDimensionnalArray = new byte[arrayWidth, arrayHeight];
            byte one, two, three;


            for(int b = 0; b < arrayHeight - 1; b++)
            {
                for (int a = 0; a < arrayWidth - 1; a+=3)
                {
                    twoDimensionnalArray[a,b] = bitArray[index];
                    twoDimensionnalArray[a + 1, b] = bitArray[index+1];
                    twoDimensionnalArray[a + 2, b] = bitArray[index+2];
                    one = twoDimensionnalArray[a, b];
                    two = twoDimensionnalArray[a + 1, b];
                    three = twoDimensionnalArray[a + 2, b];
                    index += 3;
                    MessageBox.Show(one.ToString() +","+ two.ToString() + ","+ three.ToString(), a+","+b);
                }
            }

            return twoDimensionnalArray;
        }

        private void CloseImage(object sender, RoutedEventArgs e)
        {
            display_image.Source = null;
            openedFileUri = "";
            openedFileName = null;
            noFilterImage = null;
            negativeFilterImage = null;
            blurFilterImage = null;
            blacknWhiteFilterImage = null;
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
                DefaultExt = ".bmp",
                Filter = "BMP Files (*.bmp)|*.bmp"
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
