﻿using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml;
using Typerr.ViewModel;
using System.Drawing.Imaging;
using Typerr.Service;

namespace Typerr.Commands
{
    public class CreateCommand : CommandBase
    {
        private readonly CreateTestViewModel _createTestViewModel;
        private readonly HomeViewModel _homeViewModel;

        public CreateCommand(CreateTestViewModel createTestViewModel, HomeViewModel homeViewModel)
        {
            _createTestViewModel = createTestViewModel;
            _homeViewModel = homeViewModel;
        }

        public override void Execute(object parameter)
        {
            _createTestViewModel.TestModel.WordCount = FormatService.GetWordCount(_createTestViewModel.TestModel.article.text);

            if (_createTestViewModel.Image != null)
            {
                _createTestViewModel.TestModel.Image = _createTestViewModel.Image;
                _createTestViewModel.TestModel.Base64Image = CompressImage();
            }
            else
            {
                _createTestViewModel.TestModel.Base64Image = "NULL";
            }
            _createTestViewModel.TestModel.article.text = FormatService.FormatText(_createTestViewModel.TestModel.article.text);
            TestService.Write(_createTestViewModel.TestModel, GenerateFileName());

            _homeViewModel.MainViewModel.AddLibTile(_createTestViewModel.TestModel, _homeViewModel);
            _homeViewModel.RefreshLibrary();

            _createTestViewModel.CreateTestCloseCommand.Execute(parameter);
            _createTestViewModel.Reset();

        }

        private string CompressImage()
        {
            const string temp = "img";
            const int width = 500;
            const int height = 300;

            if (File.Exists(temp))
            {
                try
                {
                    File.Delete(temp);
                }
                catch (Exception)
                {

                    throw;
                }
            }

            File.Create(temp).Close();

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(_createTestViewModel.Image));

            using (var fileStream = new FileStream(temp, FileMode.Create))
            {
                encoder.Save(fileStream);
            }

            Bitmap oldImage = (Bitmap)Bitmap.FromFile(temp);


            float aspectWidth = 5;
            float aspectHeight = 3;
            
            RectangleF rect = new RectangleF(0, 0, oldImage.Width, oldImage.Height);

            if (rect.Width / rect.Height > (aspectWidth / aspectHeight) + 0.005f)
            {
                // Picture is landscape
                rect.Width = (float)rect.Height * (aspectWidth / aspectHeight);

            }
            else if (rect.Width / rect.Height < (aspectWidth / aspectHeight) - 0.005f)
            {
                // Picture is Portrait
                rect.Height = rect.Width / (aspectWidth / aspectHeight);

            }

            Image image = oldImage.Clone(rect, oldImage.PixelFormat);

            oldImage.Dispose();
            File.Delete(temp);

            image = new Bitmap(image, width, height);

            using (var memory = new MemoryStream())
            {
                image.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                _createTestViewModel.Image = bitmapImage;

                byte[] byteImage = memory.ToArray();
                return Convert.ToBase64String(byteImage);
            }
        }

        private string GenerateFileName()
        {
            // String formatting
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string filename = textInfo.ToLower(_createTestViewModel.TestModel.article.title);
            filename = filename.Replace(" ", "_");

            // Remove restricted chars
            filename = filename.Replace(":", string.Empty);
            filename = filename.Replace("|", string.Empty);
            filename = filename.Replace(@"\", string.Empty);
            filename = filename.Replace("/", string.Empty);
            filename = filename.Replace("?", string.Empty);
            filename = filename.Replace(">", string.Empty);
            filename = filename.Replace("<", string.Empty);
            filename = filename.Replace("*", string.Empty);
            filename = filename.Replace("\"", string.Empty);

            if (filename.Length > 15)
            {
                // truncate file name
                filename = filename.Substring(0, 15);
            }

            // Append extension
            filename += ".typr";

            // Insert path
            filename = filename.Insert(0, "tests/");

            if (!Directory.Exists("tests"))
            {
                Directory.CreateDirectory("tests");
            }

            if (File.Exists(filename))
            {
                int i = 1;
                filename = filename.Insert(filename.Length - 5, "_1");
                while (File.Exists(filename))
                {
                    i++;
                    filename = filename.Remove(filename.Length - 6, 1);
                    filename = filename.Insert(filename.Length - 5, i.ToString());
                }
            }

            return filename;
        }
    }
}
