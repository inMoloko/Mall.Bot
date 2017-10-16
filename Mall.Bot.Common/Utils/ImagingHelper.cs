using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Moloko.Utils
{
    public class GetKegelResult
    {
        /// <summary>
        /// Размер шрифта
        /// </summary>
        public int Kegel { get; set; }
        /// <summary>
        /// Смещение по оси х 
        /// </summary>
        public float LongitudeDist { get; set; }
        /// <summary>
        /// Смещение по оси y 
        /// </summary>
        public float LatitudeDist { get; set; }
    }
    public class ImagingHelper
    {
        public static Color GetBitmapImageBaseColor(BitmapImage image)
        {
            if (image == null)
            {
                return Colors.White;
            }
            else
            {
                try
                {

                    var img = BitmapImage2Bitmap(image);
                    var pixel = img.GetPixel(0, 0);
                    return pixel.A == 0 ? Colors.White : Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);
                }
                catch
                {
                    return Colors.White;
                }
            }
        }

        /// <summary>
        /// Bitmap из byte[]
        /// </summary>        
        public static System.Drawing.Bitmap BytesToImages(byte[] bytes)
        {
            System.Drawing.Bitmap image;
            using (var ms = new MemoryStream(bytes))
            {
                image = new System.Drawing.Bitmap(ms);
            }
            return image;
        }

        /// <summary>
        /// Method to rotate an Image object. The result can be one of three cases:
        /// - upsizeOk = true: output image will be larger than the input, and no clipping occurs 
        /// - upsizeOk = false & clipOk = true: output same size as input, clipping occurs
        /// - upsizeOk = false & clipOk = false: output same size as input, image reduced, no clipping
        /// 
        /// A background color must be specified, and this color will fill the edges that are not 
        /// occupied by the rotated image. If color = transparent the output image will be 32-bit, 
        /// otherwise the output image will be 24-bit.
        /// 
        /// Note that this method always returns a new Bitmap object, even if rotation is zero - in 
        /// which case the returned object is a clone of the input object. 
        /// </summary>
        /// <param name="inputImage">input Image object, is not modified</param>
        /// <param name="angleDegrees">angle of rotation, in degrees</param>
        /// <param name="upsizeOk">see comments above</param>
        /// <param name="clipOk">see comments above, not used if upsizeOk = true</param>
        /// <param name="backgroundColor">color to fill exposed parts of the background</param>
        /// <returns>new Bitmap object, may be larger than input image</returns>
        public static System.Drawing.Bitmap RotateImage(System.Drawing.Image inputImage, float angleDegrees, bool upsizeOk,
                                         bool clipOk, System.Drawing.Color backgroundColor)
        {
            // Test for zero rotation and return a clone of the input image
            if (angleDegrees == 0f)
                return (System.Drawing.Bitmap)inputImage.Clone();

            // Set up old and new image dimensions, assuming upsizing not wanted and clipping OK
            int oldWidth = inputImage.Width;
            int oldHeight = inputImage.Height;
            int newWidth = oldWidth;
            int newHeight = oldHeight;
            float scaleFactor = 1f;

            // If upsizing wanted or clipping not OK calculate the size of the resulting bitmap
            if (upsizeOk || !clipOk)
            {
                double angleRadians = angleDegrees * Math.PI / 180d;

                double cos = Math.Abs(Math.Cos(angleRadians));
                double sin = Math.Abs(Math.Sin(angleRadians));
                newWidth = (int)Math.Round(oldWidth * cos + oldHeight * sin);
                newHeight = (int)Math.Round(oldWidth * sin + oldHeight * cos);
            }

            // If upsizing not wanted and clipping not OK need a scaling factor
            if (!upsizeOk && !clipOk)
            {
                scaleFactor = Math.Min((float)oldWidth / newWidth, (float)oldHeight / newHeight);
                newWidth = oldWidth;
                newHeight = oldHeight;
            }

            // Create the new bitmap object. If background color is transparent it must be 32-bit, 
            //  otherwise 24-bit is good enough.
            var newBitmap = new System.Drawing.Bitmap(newWidth, newHeight, backgroundColor == System.Drawing.Color.Transparent ?
                                             System.Drawing.Imaging.PixelFormat.Format32bppArgb : System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            newBitmap.SetResolution(inputImage.HorizontalResolution, inputImage.VerticalResolution);

            // Create the Graphics object that does the work
            using (System.Drawing.Graphics graphicsObject = System.Drawing.Graphics.FromImage(newBitmap))
            {
                graphicsObject.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsObject.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphicsObject.SmoothingMode = SmoothingMode.HighQuality;

                // Fill in the specified background color if necessary
                if (backgroundColor != System.Drawing.Color.Transparent)
                    graphicsObject.Clear(backgroundColor);

                // Set up the built-in transformation matrix to do the rotation and maybe scaling
                graphicsObject.TranslateTransform(newWidth / 2f, newHeight / 2f);

                if (scaleFactor != 1f)
                    graphicsObject.ScaleTransform(scaleFactor, scaleFactor);

                graphicsObject.RotateTransform(angleDegrees);
                graphicsObject.TranslateTransform(-oldWidth / 2f, -oldHeight / 2f);

                // Draw the result 
                graphicsObject.DrawImage(inputImage, 0, 0);
            }

            return newBitmap;
        }

        public static BitmapImage BitmapSource2BitmapImage(BitmapSource source)
        {

            if (source == null)
                return null;
            try
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new PngBitmapEncoder();
                    BitmapImage bitmapImage = new BitmapImage();
                    enc.Frames.Add(BitmapFrame.Create(source));
                    enc.Save(outStream);
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = outStream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    return bitmapImage;
                }
            }
            catch (Exception exc)
            {
                Logging.Logger.Error(exc.ToString());
            }
            return null;
        }

        public static System.Drawing.Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);
                outStream.Close();
                // return bitmap; <-- leads to problems, stream is closed/closing ...
                return new System.Drawing.Bitmap(bitmap);
            }
        }

        public static BitmapImage Bitmap2BitmapImage(System.Drawing.Bitmap bitmap)
        {

            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public static BitmapImage GetBitMapImage(byte[] data, int maxHeight = -1, int maxWidth = -1)
        {
            int w = 0;
            int h = 0;

            BitmapImage bi = null;
            if (data != null && data.Count() > 0)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        bi = new BitmapImage();
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.StreamSource = ms;

                        if (maxHeight != -1 || maxWidth != -1)
                            if (h > maxHeight || w > maxWidth)
                            {
                                double deltaW;
                                double deltaH;

                                deltaH = (double)maxHeight / h;
                                deltaW = (double)maxWidth / w;

                                if (deltaH <= deltaW)
                                {
                                    bi.DecodePixelHeight = Convert.ToInt32(h * deltaH);
                                    bi.DecodePixelWidth = Convert.ToInt32(w * deltaH);
                                }
                                else
                                {
                                    bi.DecodePixelHeight = Convert.ToInt32(h * deltaW);
                                    bi.DecodePixelWidth = Convert.ToInt32(w * deltaW);
                                }
                            }
                        bi.EndInit();
                        bi.Freeze();
                    }
                }
                catch (Exception exc)
                {
                    Logging.Logger.Error(exc);
                    bi = null;
                }
            }
            return bi;
        }

        public static byte[] GetBitMapImageBytes(BitmapImage image)
        {
            if (image == null)
                return null;

            MemoryStream stream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(stream);
            return stream.GetBuffer();
        }


        public static byte[] ImageToByteArray(System.Drawing.Image image)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                image.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
            catch (Exception exc)
            {
                Logging.Logger.Error(exc);
            }
            return null;
        }


        /// <summary>
        /// Преобразование GUI элемента в картинку
        /// </summary>
        public static ImageSource ToImageSource(FrameworkElement obj)
        {
            Transform transform = obj.LayoutTransform;
            obj.LayoutTransform = null;

            // fix margin offset as well
            Thickness margin = obj.Margin;
            obj.Margin = new Thickness(0, 0,
                 margin.Right - margin.Left, margin.Bottom - margin.Top);

            // Get the size of canvas
            Size size = new Size(obj.Width, obj.Height);

            // force control to Update
            obj.Measure(size);
            obj.Arrange(new Rect(size));

            RenderTargetBitmap bmp = new RenderTargetBitmap(
                (int)obj.Width, (int)obj.Height, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(obj);

            // return values as they were before
            obj.LayoutTransform = transform;
            obj.Margin = margin;
            return bmp;
        }


        public static byte[] GetScreenshot()
        {
            try
            {
                System.Drawing.Bitmap bmpScreenCapture = null;
                using (bmpScreenCapture = new System.Drawing.Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                                 Screen.PrimaryScreen.Bounds.Height))
                {
                    var destRect = new System.Drawing.Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width/10, Screen.PrimaryScreen.Bounds.Height/10);
                    var destImage = new System.Drawing.Bitmap(Screen.PrimaryScreen.Bounds.Width / 10, Screen.PrimaryScreen.Bounds.Height / 10);

                    using (var g = System.Drawing.Graphics.FromImage(bmpScreenCapture))
                    {
                        g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                         Screen.PrimaryScreen.Bounds.Y,
                                         0, 0,
                                         bmpScreenCapture.Size,
                                         System.Drawing.CopyPixelOperation.SourceCopy);                      
                    }
                    //bmpScreenCapture.Save(@"c:\temp\temp.png");
                    return ImageToByteArray(ResizeImage(bmpScreenCapture, bmpScreenCapture.Width/10, bmpScreenCapture.Height/10));
                }
            }
            catch (Exception exc)
            {
                Logging.Logger.Error(exc);
            }
            return null;
        }

        public static System.Drawing.Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new System.Drawing.Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = System.Drawing.Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        public static byte[] StreamToByteArray(Stream stream)
        {
            using (stream)
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);
                    return memStream.ToArray();
                }
            }
        }
        /// <summary>
        /// Возвращает размер шрифта которым нужно написать текст, чтобы вместить его в окружность заданного диаметра
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="text"></param>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <param name="diametr"></param>
        /// <returns></returns>
        public static GetKegelResult GetKegel(System.Drawing.Bitmap bitmap, string text, double longitude, double latitude, double diametr)
        {
            var result = new GetKegelResult();
            int i = 1;
            using (var gr = System.Drawing.Graphics.FromImage(bitmap))
            {
                bool flag = true;
                while (flag)
                {
                    System.Drawing.Font arialBold = new System.Drawing.Font("Segoe UI", i, System.Drawing.FontStyle.Bold);

                    System.Drawing.SizeF textSize = gr.MeasureString(text, arialBold);
                    if (diametr < Math.Sqrt(textSize.Height * textSize.Height + textSize.Width * textSize.Width))
                    {
                        flag = false;
                    }
                    else
                    {
                        result.Kegel = i;
                        result.LongitudeDist = textSize.Width / 2;
                        result.LatitudeDist = textSize.Height / 2;
                        i += 3;
                    }
                }
            }
            return result;
        }
    }
}
