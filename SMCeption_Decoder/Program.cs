using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.Json;
using System.Drawing.Drawing2D;
using System.Threading;

namespace SMCeption_Decoder
{
    public class Screen
    {
        public float height { get; set; }
        public float width { get; set; }
    }
    public class Frame
    {
        public Pixel[] p { get; set; }
    }
    public class Pixel
    {
        public string a { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            int width = 96;
            int height = 54;
            //Load datapath
            string dataPath = File.ReadAllText(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\dataPath.txt");

            Console.Title = "SM Screen Mod Decoder";


            if (!Directory.Exists(dataPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Couldn't find Scrap Mechanic's Data path");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Path Does not exist: " + dataPath);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Paste the correct path into 'dataPath.txt'");
                Console.ReadKey();
                throw new Exception();
            }

            Console.Write("Decoding your screen...");

            
            //test frame
            /*List<Pixel> pixels = new List<Pixel>();
            for (int x = 0; x < 96; x++)
            {
                for (int y = 0; y < 54; y++)
                {
                    pixels.Add(new Pixel() { a = "696969AA", });
                }
            }
            Frame frame = new Frame() { p = pixels.ToArray() };
            string json = JsonSerializer.Serialize(frame);
            File.WriteAllText(dataPath + "\\frame.json", json);*/
            bool error = true;

            while (true)
            {
                //Get Data form game
                if (File.Exists(dataPath + "\\done.json") || error)
                {
                    //Generate new frame
                    var image = ScreenCapture.CaptureDesktop();
                    Bitmap rawFrame = new Bitmap(image);
                    Bitmap resizedFrame = ResizeImage(rawFrame, width, height);
                    //resizedFrame.Save(@"C:\Users\claas\Desktop\snippetsource.jpg", ImageFormat.Jpeg);
                    //image.Save(@"C:\Users\claas\Desktop\snippetsource.jpg", ImageFormat.Jpeg);

                    List<Pixel> pixels = new List<Pixel>();
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var color = resizedFrame.GetPixel(x, y);
                            pixels.Add(new Pixel() { a = $"{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}" });
                        }
                    }
                    pixels.Reverse();
                    Frame frame = new Frame() { p = pixels.ToArray() };
                    string json = JsonSerializer.Serialize(frame);
                    try
                    {
                        File.WriteAllText(dataPath + "\\frame.json", json);
                        error = false;
                    }
                    catch {
                        error = true;
                        Console.WriteLine("Error handled");
                    }

                    //delete done
                    try
                    {
                        File.Delete(dataPath + "\\done.json");
                    }
                    catch
                    {
                    }
                }
                try
                {
                    if (File.Exists(dataPath + "\\screen.json")) ;
                    {
                        string json = File.ReadAllText(dataPath + "\\screen.json");
                        Screen poopScreen = JsonSerializer.Deserialize<Screen>(json);
                        width = (int)poopScreen.width;
                        height = (int)poopScreen.height;
                        File.Delete(dataPath + "\\screen.json");
                    }
                }
                catch { }
            }
        }
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        public class ScreenCapture
        {
            [DllImport("user32.dll")]
            private static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern IntPtr GetDesktopWindow();

            [StructLayout(LayoutKind.Sequential)]
            private struct Rect
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            [DllImport("user32.dll")]
            private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

            public static Image CaptureDesktop()
            {
                return CaptureWindow(GetDesktopWindow());
            }

            public static Bitmap CaptureActiveWindow()
            {
                return CaptureWindow(GetForegroundWindow());
            }

            public static Bitmap CaptureWindow(IntPtr handle)
            {
                var rect = new Rect();
                GetWindowRect(handle, ref rect);
                var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
                var result = new Bitmap(bounds.Width, bounds.Height);

                using (var graphics = Graphics.FromImage(result))
                {
                    graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                }

                return result;
            }
        }
    }
}
