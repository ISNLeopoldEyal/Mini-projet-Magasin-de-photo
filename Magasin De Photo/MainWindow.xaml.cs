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
        static string openedFileUri;
        static string openedFileName;
        static BitmapSource noFilterImage = null;
        static BitmapSource negativeFilterImage = null;
        static BitmapSource blurFilterImage = null;
        static BitmapSource blacknWhiteFilterImage = null;
        static byte[] allPixels;

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
            int stride = (int)noFilterImage.PixelWidth * (noFilterImage.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[(int)noFilterImage.PixelHeight * stride];

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

        private BitmapSource FromArrayToImage(byte[] pixels)
        {
            int width = noFilterImage.PixelWidth,
                height = noFilterImage.PixelHeight,
                stride = (int)noFilterImage.PixelWidth * (noFilterImage.Format.BitsPerPixel + 7) / 8;

            double dpiX = noFilterImage.DpiX,
                   dpiY = noFilterImage.DpiY;

            PixelFormat format = noFilterImage.Format;
            BitmapPalette palette = noFilterImage.Palette;

            BitmapSource bitmap = BitmapSource.Create(width, height, dpiX, dpiY, format, palette, pixels, stride);
                       
            return bitmap;
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

        private void SetLayoutPreviews()
        {
            no_filter_preview.Source = noFilterImage;
            negative_filter_preview.Source = negativeFilterImage;
            blur_filter_preview.Source = blurFilterImage;
            blacknwhite_filter_preview.Source = blacknWhiteFilterImage;
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
                openedFileName = openFileDialog.SafeFileName;

                BitmapImage _bmpi = new BitmapImage();
                _bmpi.BeginInit();
                _bmpi.CacheOption = BitmapCacheOption.OnLoad;
                _bmpi.UriSource = new Uri(openedFileUri);
                _bmpi.EndInit();

                noFilterImage = _bmpi;
                display_image.Source = noFilterImage;
                allPixels = GetBitArrayFromImage();

                CreateAllFilters();
                SetLayoutPreviews();
            }
        }

        private void CreateAllFilters()
        {
            CreateFilter_Negative();
            CreateFilter_BlacknWhite();
            CreateFilter_Blur();
        }

        private void CreateFilter_Negative()
        {
            byte[] array = (byte[])allPixels.Clone();

            int offset = OffsetComputation(allPixels),
                reelWidthNoOffset = noFilterImage.PixelWidth * 4,
                reelWidth = noFilterImage.PixelWidth * 4 + offset;

            for (int index = 0; index < array.Length; index+=4)
            {
               array[index] = (byte)(255 - array[index]);
               array[index + 1] = (byte)(255 - array[index + 1]);
               array[index + 2] = (byte)(255 - array[index + 2]);

                if (index % reelWidth == reelWidthNoOffset)
                    index += (offset - 4);
            }

            //DebugDisplayBits(array);
            negativeFilterImage = FromArrayToImage(array);
        }

        private void CreateFilter_BlacknWhite()
        {
            float BlueRatio = 0.114f, GreenRatio = 0.587f, RedRatio = 0.299f;

            int offset = OffsetComputation(allPixels),
                reelWidthNoOffset = noFilterImage.PixelWidth * 4,
                reelWidth = noFilterImage.PixelWidth * 4 + offset;

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

            blacknWhiteFilterImage = FromArrayToImage(array);
        }


        private void CreateFilter_Blur()
        {
            int average,
                offset = OffsetComputation(allPixels),
                reelWidthNoOffset = noFilterImage.PixelWidth * 4,
                reelWidth = noFilterImage.PixelWidth * 4 + offset;

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

            blurFilterImage = FromArrayToImage(array);
        }

        private int GetNeighborAverage(byte[] array, int index)
        {
            double numbersInSum = 1,
                sum = array[index],
                average;
            
            int offset = OffsetComputation(allPixels),
                reelWidthNoOffset = noFilterImage.PixelWidth * 4,
                reelWidth = noFilterImage.PixelWidth * 4 + offset,
                indexMax = array.Length - 1;

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
                sum += array[index-reelWidth];
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

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            if (display_image.Source != null)
            {
                SaveActualFile();
            }
            else { return; }
        }

        private void SaveActualFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = openedFileUri
            };

            BitmapEncoder encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)display_image.Source));
            using (var stream = saveFileDialog.OpenFile())
            {
                encoder.Save(stream);
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

        private void SaveAsFileFromDialog(object sender, RoutedEventArgs e)
        {
            if (display_image.Source != null)
            {
                SaveAsFile();
            }
            else
                return;
        }

        private void SaveAsFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                DefaultExt = ".bmp",
                Filter = "BMP Files (*.bmp)|*.bmp",
                FileName = ChangeName()
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)display_image.Source));
                using (var stream = saveFileDialog.OpenFile())
                {
                    encoder.Save(stream);
                }
            }
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
