using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        public byte[] allPixels;
        public BitmapSource noFilterImage = null;
        public BitmapSource negativeFilterImage = null;
        public BitmapSource blurFilterImage = null;
        public BitmapSource blacknWhiteFilterImage = null;

        public int offset, reelWidthNoOffset, reelWidth, width, height, stride;
        public double dpiX, dpiY;
        public string openedFileUri;
        public string openedFileName;
        public BitmapPalette palette;
        public PixelFormat format;

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
        }

        private byte[] GetBitArrayFromImage()
        {
            byte[] pixels = new byte[height * stride];
            noFilterImage.CopyPixels(pixels, stride, 0);
            //DebugDisplayBits(pixels);
            //DEBUG_ShowCleanArray(pixels);

            return pixels;
        }

        public void DEBUG_ShowCleanArray (byte[] array)
        {
            string final = "";

            for (int x = 0; x < array.Length; x++)
                final += array[x] + ".";

            MessageBox.Show(final);
        }

        private void FromArrayToImage(byte[] pixels)
        {
            //int width = noFilterImage.PixelWidth,
            //    height = noFilterImage.PixelHeight,
            //    stride = (int)noFilterImage.PixelWidth * (noFilterImage.Format.BitsPerPixel + 7) / 8;

            //double dpiX = noFilterImage.DpiX,
            //       dpiY = noFilterImage.DpiY;

            //PixelFormat format = noFilterImage.Format;
            //BitmapPalette palette = noFilterImage.Palette;

            //BitmapSource bitmap = BitmapSource.Create(width, height, dpiX, dpiY, format, palette, pixels, stride);
                       
            //return bitmap;
        }
        
        private void DisplayNoFilter(object sender, RoutedEventArgs e)
        {
            if(noFilterImage != null)
                display_image.Source = noFilterImage;
        }

        private void DisplayNegative(object sender, RoutedEventArgs e)
        {
            if (negativeFilterImage != null)
                display_image.Source = negativeFilterImage;
        }

        private void DisplayBlur(object sender, RoutedEventArgs e)
        {
            if (blurFilterImage != null)
                display_image.Source = blurFilterImage;
        }

        private void DisplayBlacknWhite(object sender, RoutedEventArgs e)
        {
            if (blacknWhiteFilterImage != null)
                display_image.Source = blacknWhiteFilterImage;
        }
        
        private void CreateAllFilters()
        {
            Thread negatiVeFilterThread = new Thread(CreateFilter_Negative) { Name = "Thread : Negatif" };
            Thread blacknWhiteFilterThread = new Thread(CreateFilter_BlacknWhite) { Name = "Thread : Niveau de gris" };
            Thread blurFilterThread = new Thread(CreateFilter_Blur) { Name = "Thread : Flou" };
            
            negatiVeFilterThread.Start();
            blacknWhiteFilterThread.Start();
            blurFilterThread.Start();
        }

        private void CreateFilter_Negative()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                byte[] array = (byte[])allPixels.Clone();
                
                for (int index = 0; index < array.Length; index+=4)
                {
                   array[index] = (byte)(255 - array[index]);
                   array[index + 1] = (byte)(255 - array[index + 1]);
                   array[index + 2] = (byte)(255 - array[index + 2]);

                    if (index % reelWidth == reelWidthNoOffset)
                        index += (offset - 4);
                }

                //DebugDisplayBits(array);
                BitmapSource bitmap = BitmapSource.Create(width, height, dpiX, dpiY, format, palette, array, stride);
                negativeFilterImage = bitmap;
                negative_filter_preview.Source = negativeFilterImage;
            }));
        }

        private void CreateFilter_BlacknWhite()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                float BlueRatio = 0.114f, GreenRatio = 0.587f, RedRatio = 0.299f;

                byte[] array = (byte[])allPixels.Clone();
                byte greyScale;
                
                for (int index = 0; index < array.Length; index+=4)
                {
                    greyScale = (byte)(RedRatio * array[index] + GreenRatio * array[index+1] + BlueRatio * array[index+2]);

                    array[index] = greyScale;
                    array[index + 1] = greyScale;
                    array[index + 2] = greyScale;

                    if (index % reelWidth == reelWidthNoOffset)
                        index += (offset - 4);
                }

                BitmapSource bitmap = BitmapSource.Create(width, height, dpiX, dpiY, format, palette, array, stride);
                blacknWhiteFilterImage = bitmap;
                blacknwhite_filter_preview.Source = blacknWhiteFilterImage;
            }));
        }


        private void CreateFilter_Blur()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                int average;

                byte[] array = (byte[])allPixels.Clone();

                for (int index = 0; index < array.Length; index+=4)
                {
                    average = GetNeighborAverage(array, index);
                    array[index] = (byte)average;

                    average = GetNeighborAverage(array, index + 1);
                    array[index + 1] = (byte)average;
                    
                    average = GetNeighborAverage(array, index + 2);
                    array[index + 2] = (byte)average;

                    if (index % reelWidth == reelWidthNoOffset)
                        index += (offset - 4);
                }

                BitmapSource bitmap = BitmapSource.Create(width, height, dpiX, dpiY, format, palette, array, stride);
                blurFilterImage = bitmap;
                
                blur_filter_preview.Source = blurFilterImage;
            }));
        }

        private int GetNeighborAverage(byte[] array, int index)
        {
            double numbersInSum = 1,
                sum = array[index],
                average;
            
            int indexMax = array.Length - 1;

            bool leftClose     = index % reelWidth > 4,
                 upClose       = index - reelWidth > 0,
                 rightClose    = index % reelWidth < reelWidthNoOffset - 4,
                 downClose     = index + reelWidth < indexMax,
                 upLeftNext    = upClose && leftClose,
                 upRightNext   = upClose && rightClose,
                 downLeftNext  = downClose && leftClose,
                 downRightNext = downClose && rightClose,
                 leftNext      = index % reelWidth > 8,
                 rightNext     = index % reelWidth < reelWidthNoOffset - 8,
                 upNext        = index - 2*reelWidth > 0,
                 downNext      = index + 2*reelWidth < indexMax;

            if (leftClose)
            {
                sum += array[index - 4];
                numbersInSum++;
            }
            if (upClose)
            {
                sum += array[index - reelWidth];
                numbersInSum++;
            }
            if (rightClose)
            {
                sum += array[index + 4];
                numbersInSum++;
            }
            if (downClose)
            {
                sum += array[index + reelWidth];
                numbersInSum++;
            }
            if (upLeftNext)
            {
                sum += array[index - reelWidth - 4];
                numbersInSum++;
            }
            if (upRightNext)
            {
                sum += array[index - reelWidth + 4];
                numbersInSum++;
            }
            if (downLeftNext)
            {
                sum += array[index + reelWidth - 4];
                numbersInSum++;
            }
            if (downRightNext)
            {
                sum += array[index + reelWidth + 4];
                numbersInSum++;
            }
            if (leftNext)
            {
                sum += array[index - 8];
                numbersInSum++;
            }
            if (upNext)
            {
                sum += array[index - 2*reelWidth];
                numbersInSum++;
            }
            if (rightNext)
            {
                sum += array[index + 8];
                numbersInSum++;
            }
            if (downNext)
            {
                sum += array[index + 2*reelWidth];
                numbersInSum++;
            }

            average = Math.Round(sum / numbersInSum);
            return (int)average;
        }

        private void SetGlobalVariables()
        {
            stride = noFilterImage.PixelWidth * (noFilterImage.Format.BitsPerPixel + 7) / 8;
            width = noFilterImage.PixelWidth;
            height = noFilterImage.PixelHeight;
            allPixels = GetBitArrayFromImage();

            offset = OffsetComputation(allPixels);
            reelWidthNoOffset = noFilterImage.PixelWidth * 4;
            reelWidth = noFilterImage.PixelWidth * 4 + offset;
            dpiX = noFilterImage.DpiX;
            dpiY = noFilterImage.DpiY;
            format = noFilterImage.Format;
            palette = noFilterImage.Palette;
        }

        private int OffsetComputation(byte[] array)
        {
            int offsetLength = 0;
            for (int x = array.Length - 1; x > 0; x--)
            {
                if (array[x] != 0)
                    break;

                offsetLength++;
            }
            
            return offsetLength;
        }

        private void OpenImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                DefaultExt = ".bmp",
                Filter = "BMP Files (*.bmp)|*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                openedFileUri = openFileDialog.FileName;
                openedFileName = openFileDialog.SafeFileName;

                InitImage();
            }
        }

        private void InitImage()
        {
            BitmapImage _bmpi = new BitmapImage();
            _bmpi.BeginInit();
            _bmpi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            _bmpi.CacheOption = BitmapCacheOption.OnLoad;
            _bmpi.UriSource = new Uri(openedFileUri);
            _bmpi.EndInit();

            noFilterImage = _bmpi;
            display_image.Source = noFilterImage;
            no_filter_preview.Source = noFilterImage;

            SetGlobalVariables();
            CreateAllFilters();
        }

        private void SaveImage(object sender, RoutedEventArgs e)
        {
            if (display_image.Source != null)
                SaveActualImage();
        }

        private void SaveActualImage()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = openedFileUri
            };

            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)display_image.Source));
            using (var stream = saveFileDialog.OpenFile())
            {
                encoder.Save(stream);
            }

            InitImage();
        }

        private void SaveAsImage(object sender, RoutedEventArgs e)
        {
            if (display_image.Source != null)
                SaveAsImageWithDialog();
            else
                return;
        }

        private void SaveAsImageWithDialog()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                DefaultExt = ".bmp",
                Filter = "BMP Files (*.bmp)|*.bmp",
                FileName = ChangeName()
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)display_image.Source));
                using (var stream = saveFileDialog.OpenFile())
                {
                    encoder.Save(stream);
                }
            }
        }

        private string ChangeName()
        {
            string fileName = openedFileName;
            int extensionStart = openedFileName.Length - 4;

            if (display_image.Source == negativeFilterImage)
                fileName = fileName.Insert(extensionStart, " - Negatif");
            if (display_image.Source == blacknWhiteFilterImage)
                fileName = fileName.Insert(extensionStart, " - Niveau De Gris");
            if (display_image.Source == blurFilterImage)
                fileName = fileName.Insert(extensionStart, " - FLou");

            return fileName;
        }

        private void CloseImage(object sender, RoutedEventArgs e)
        {
            display_image.Source = null;
            no_filter_preview.Source = null;
            negative_filter_preview.Source = null;
            blur_filter_preview.Source = null;
            blacknwhite_filter_preview.Source = null;
            openedFileUri = "";
            openedFileName = null;
            noFilterImage = null;
            negativeFilterImage = null;
            blurFilterImage = null;
            blacknWhiteFilterImage = null;
        }

        //private byte[,] FromOneToTwoDimensionsArray()
        //{
        //    int width = noFilterImage.PixelWidth*3,
        //        height = noFilterImage.PixelHeight,
        //        index = 0,
        //        offset = OffsetComputation(allPixels),
        //        reelWidthNoOffset = noFilterImage.PixelWidth * 4,
        //        reelWidth = noFilterImage.PixelWidth * 4 + offset;
        //
        //    byte[,] twoDimensions = new byte[width,height];
        //
        //    for(int y = 0; y < height; y++)
        //    {
        //        for(int x = 0; x < width; x++)
        //        {
        //            twoDimensions[x, y] = allPixels[index];
        //
        //            index++;
        //
        //            if ((index + 1) % 4 == 0)
        //                index++;
        //            if (index == reelWidthNoOffset+(reelWidth*y))
        //                index += offset;
        //            //MessageBox.Show(x + "," + y +" : "+ twoDimensions[x, y], "On index : " + index);
        //        }
        //    }
        //
        //    return twoDimensions;
        //}
        //
        //private byte[] FromTwoToOneDimension (byte[,] twoDimensions)
        //{
        //    int width = twoDimensions.GetLength(0),
        //        height = twoDimensions.GetLength(1),
        //        index = 0,
        //        offset = OffsetComputation(allPixels),
        //        reelWidthNoOffset = noFilterImage.PixelWidth * 4,
        //        reelWidth = noFilterImage.PixelWidth * 4 + offset;
        //
        //    byte[] array = (byte[])allPixels.Clone();
        //
        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            array[index] = twoDimensions[x, y];
        //
        //            index++;
        //
        //            if ((index + 1) % 4 == 0)
        //                index++;
        //            if (x == width - 1)
        //                index += offset;
        //            //MessageBox.Show(index + " : " + original[index], "On index : "+ x + "," + y);
        //        }
        //    }
        //
        //    return array;
        //}
        //
        //private void DebugDisplayBits(byte[] bitArray)
        //{
        //    int width = noFilterImage.PixelWidth * 3,
        //        height = noFilterImage.PixelHeight,
        //        offset = OffsetComputation(allPixels),
        //        reelWidthNoOffset = noFilterImage.PixelWidth * 4,
        //        reelWidth = noFilterImage.PixelWidth * 4 + offset;
        //
        //    string output = "";
        //
        //    for(int index = 3; index < bitArray.Length; index += 4)
        //    {
        //        if (index % reelWidth == reelWidthNoOffset)
        //        {
        //            index += offset;
        //            if (index >= bitArray.Length)
        //                break;
        //        }
        //        output += bitArray[index] + " - ";
        //    }
        //    MessageBox.Show(output);
        //}
    }
}
