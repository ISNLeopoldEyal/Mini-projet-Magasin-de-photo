﻿using Microsoft.Win32;
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

                BitmapImage _bmpi = new BitmapImage();
                _bmpi.BeginInit();
                _bmpi.CacheOption = BitmapCacheOption.OnLoad;
                _bmpi.UriSource = new Uri(openedFileUri);
                _bmpi.EndInit();

                noFilterImage = _bmpi;
                display_image.Source = noFilterImage;
                byte[] allPixels = GetBitArrayFromImage();

                CreateAllFilters(allPixels);
                SetLayoutPreviews();
            }
        }

        private void CreateAllFilters (byte[] array)
        {
            byte[,] twoDim = FromOneToTwoDimensionsArray(array);
            array = FromTwoToOneDimension(twoDim, array);

            ApplyFilterToArray((byte[])array.Clone() , negativeFilterImage);
            ApplyFilterToArray((byte[])array.Clone() , blacknWhiteFilterImage);

            CreateFilter_Blur((byte[,])twoDim.Clone(), (byte[])array.Clone());
        }
        
        private void ApplyFilterToArray (byte[] array, BitmapSource _bitmapSource)
        {
            int colorState = 0;
            int rowState = 0;
            int waitState = 0;

            int offsetLength = 0;
            
            bool WaitingForOffset = false;


            // NEGATIVE FILTER
            byte filtersPixelColor_Negative = 0;

            // BLACK AND WHITE FILTER
            byte filtersPixelColor_BlacknWhite= 0;

            float BlueRatio = 0.114f;
            float GreenRatio = 0.587f;
            float RedRatio = 0.229f;


            //string final = "";

            for (int x = array.Length - 1; x > 0; x--)
            {
                if (array[x] != 0)
                    break;

                offsetLength++;
            }

            for (int x = 0; x < array.Length; x++)
            {
                if (WaitingForOffset == true)
                {
                    //final += array[x] + ".";

                    waitState++;

                    if (waitState >= offsetLength)
                    {
                        colorState = 0;
                        rowState = 0;
                        waitState = 0;

                        WaitingForOffset = false;
                    }
                }
                else
                {
                    // WORK OUT COLOR
                    filtersPixelColor_Negative = (byte)(255 - array[x]);

                    if (colorState == 0)
                        filtersPixelColor_BlacknWhite = (byte)(RedRatio * array[x] + GreenRatio * array[x+1] + BlueRatio * array[x+2]);

                    // APPLY
                    if (_bitmapSource == negativeFilterImage)  // negative
                        array[x] = filtersPixelColor_Negative;

                    else if (_bitmapSource == blacknWhiteFilterImage)  // black and white
                        array[x] = filtersPixelColor_BlacknWhite;
                    

                    if (colorState == 3)
                        array[x] = 255;

                    colorState++;
                    rowState++;

                    //final += array[x] + ".";
                    
                    if (colorState > 3)
                        colorState = 0;

                    if (rowState >= noFilterImage.PixelWidth * 4)
                        WaitingForOffset = true;
                }

            }

            //MessageBox.Show(final);

            if (_bitmapSource == negativeFilterImage)  // negative
                negativeFilterImage = FromArrayToImage(array);

            else if (_bitmapSource == blacknWhiteFilterImage)  // black and white
                blacknWhiteFilterImage = FromArrayToImage(array);
        }


        // BLUR
        private void CreateFilter_Blur(byte[,] array, byte[] original)
        {
            byte neighborAverage = 0;
            for(int y = 0; y < array.GetLength(1); y++)
            {
                for(int x = 0; x < array.GetLength(0); x++)
                {
                    neighborAverage = (byte)GetNeighborAverage(array, x, y);
                    array[x, y] = neighborAverage;
                }
            }

            original = FromTwoToOneDimension(array, original);
            blurFilterImage = FromArrayToImage(original);
        }

        private int GetNeighborAverage(byte[,] array, int x, int y)
        {
            double numbersInSum = 0,
                sum = array[x,y],
                xMax = array.GetLength(0) - 1,
                yMax = array.GetLength(1) - 1,
                average;

            if (x > 0)
            {
                sum += array[x - 1, y];
                numbersInSum++;
            }
            if (y > 0)
            {
                sum += array[x, y - 1];
                numbersInSum++;
            }
            if (x > 0 && y > 0)
            {
                sum += array[x - 1, y - 1];
                numbersInSum++;
            }
            if (x < xMax)
            {
                sum += array[x + 1, y];
                numbersInSum++;
            }
            if (y < yMax)
            {
                sum += array[x, y + 1];
                numbersInSum++;
            }
            if (x < xMax && y < yMax)
            {
                sum += array[x + 1, y + 1];
                numbersInSum++;
            }

            average = Math.Round(sum / numbersInSum);
            return (int)average;
        }

        private byte[,] FromOneToTwoDimensionsArray(byte[] oneDimension)
        {
            int width = noFilterImage.PixelWidth*3,
                height = noFilterImage.PixelHeight,
                index = 0,
                offset = OffsetComputation(oneDimension),
                reelWidthNoOffset = noFilterImage.PixelWidth * 4,
                reelWidth = noFilterImage.PixelWidth * 4 + offset;

            byte[,] twoDimensions = new byte[width,height];

            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    twoDimensions[x, y] = oneDimension[index];

                    index++;

                    if ((index + 1) % 4 == 0)
                        index++;
                    if (index == reelWidthNoOffset+(reelWidth*y))
                        index += offset;
                    //MessageBox.Show(x + "," + y +" : "+ twoDimensions[x, y], "On index : " + index);
                }
            }

            return twoDimensions;
        }

        private byte[] FromTwoToOneDimension (byte[,] twoDimensions, byte[] original)
        {
            int width = noFilterImage.PixelWidth*3,
                height = noFilterImage.PixelHeight,
                index = 0,
                offset = OffsetComputation(original),
                reelWidthNoOffset = noFilterImage.PixelWidth * 4,
                reelWidth = noFilterImage.PixelWidth * 4 + offset;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    original[index] = twoDimensions[x, y];

                    index++;

                    if ((index + 1) % 4 == 0)
                        index++;
                    if (index == reelWidthNoOffset + (reelWidth * y))
                        index += offset;
                    //MessageBox.Show(index + " : " + original[index], "On index : "+ x + "," + y);
                }
            }

            return original;
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
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)display_image.Source));
                ///encoder.Frames.Add(BitmapFrame.Create(new Uri(openedFileUri)));
                using (var stream = saveFileDialog.OpenFile())
                {
                    encoder.Save(stream);
                }
            }
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

        private byte[,] DebugDisplayBits(byte[] bitArray)
        {
            int arrayWidth = noFilterImage.PixelWidth * 3,
                arrayHeight = noFilterImage.PixelHeight,
                index = 0;
            byte[,] twoDimensionnalArray = new byte[arrayWidth, arrayHeight];
            byte one, two, three;


            for (int b = 0; b < arrayHeight - 1; b++)
            {
                for (int a = 0; a < arrayWidth - 1; a += 3)
                {
                    twoDimensionnalArray[a, b] = bitArray[index];
                    twoDimensionnalArray[a + 1, b] = bitArray[index + 1];
                    twoDimensionnalArray[a + 2, b] = bitArray[index + 2];
                    one = twoDimensionnalArray[a, b];
                    two = twoDimensionnalArray[a + 1, b];
                    three = twoDimensionnalArray[a + 2, b];
                    index += 3;
                    MessageBox.Show(one.ToString() + "," + two.ToString() + "," + three.ToString(), a + "," + b);
                }
            }

            return twoDimensionnalArray;
        }
    }
}
