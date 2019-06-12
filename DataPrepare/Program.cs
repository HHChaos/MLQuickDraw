using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DataPrepare
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo directoryInfo;
            do
            {
                Console.WriteLine("键入ndjson文件夹地址：");
                var folderStr = Console.ReadLine();
                directoryInfo = new DirectoryInfo(folderStr);
                if (!directoryInfo.Exists)
                {
                    Console.WriteLine("文件夹地址非法！");
                }
                else
                {
                    break;
                }
            } while (true);
            Console.WriteLine("键入每个文件读取数据个数：");
            if (!int.TryParse(Console.ReadLine(), out var numberLimit))
            {
                return;
            }
            Console.WriteLine("键入点阵图保存精度（例如28X28输入28,默认为28）：");
            if (!int.TryParse(Console.ReadLine(), out var pixelWidth))
            {
                pixelWidth = 28;
            }
            var files = directoryInfo.GetFiles("*.ndjson");
            //var imageSaveFolder = directoryInfo.CreateSubdirectory("Images");
            if (files?.Length > 0)
            {
                using (var dataFile = new FileStream($"{directoryInfo.FullName}\\data{numberLimit}.csv", FileMode.Create))
                {
                    foreach (var ndjson in files)
                    {
                        using (var streamReader = ndjson.OpenText())
                        {
                            for (int i = 0; i < numberLimit; i++)
                            {
                                DrawingInfo data;
                                do
                                {
                                    data = JsonConvert.DeserializeObject<DrawingInfo>(streamReader.ReadLine());
                                } while (data.Recognized == false);
                                var image = DrawPath(data.Data, 255f / pixelWidth);
                                //SaveImage($"{imageSaveFolder.FullName}\\{data.KeyId}.png", image);
                                var dotArray = GetDotArray(image, pixelWidth, pixelWidth);
                                var dataStr = new StringBuilder();
                                dataStr.Append($"{data.Word}");

                                for (int j = 0; j < pixelWidth; j++)
                                {
                                    for (int k = 0; k < pixelWidth; k++)
                                    {
                                        if (dotArray[j, k])
                                        {
                                            //Console.Write("?");
                                            dataStr.Append($",1");
                                        }
                                        else
                                        {
                                            //Console.Write("|");
                                            dataStr.Append($",0");
                                        }
                                    }
                                    //Console.Write(Environment.NewLine);
                                }

                                dataStr.Append(Environment.NewLine);
                                var buf = Encoding.UTF8.GetBytes(dataStr.ToString());
                                dataFile.Write(buf, 0, buf.Length);
                            }

                        }
                    }
                    dataFile.Flush();
                    dataFile.Close();
                }
            }
        }

        private static SKImage DrawPath(IEnumerable<int[][]> points,float strokeWidth)
        {
            var info = new SKImageInfo(255, 255);
            using (var surface = SKSurface.Create(info))
            {
                SKCanvas canvas = surface.Canvas;

                canvas.Clear(SKColors.Transparent);

                var paint = new SKPaint
                {
                    StrokeWidth = strokeWidth,
                    Style = SKPaintStyle.Stroke,
                    Color = new SKColor(0x00, 0x00, 0x00),
                    StrokeCap = SKStrokeCap.Round,

                };
                foreach (var pathData in points)
                {
                    if (pathData[0].Length == 1)
                    {
                        canvas.DrawPoint(new SKPoint(pathData[0][0], pathData[1][0]), paint);
                    }
                    else if (pathData[0].Length > 1)
                    {
                        var path = new SKPath();
                        path.MoveTo(new SKPoint(pathData[0][0], pathData[1][0]));
                        for (int i = 1; i < pathData[0].Length; i++)
                        {
                            path.LineTo(new SKPoint(pathData[0][i], pathData[1][i]));
                        }
                        canvas.DrawPath(path, paint);
                    }
                }
                return surface.Snapshot();
            }
        }

        private static bool[,] GetDotArray(SKImage image, int rowCount, int colCount)
        {
            var path = new bool[rowCount, colCount];
            var startPoint = new SKPoint();

            var pixels = image.PeekPixels();
            var rectSize = new SKPoint(image.Width / (float)colCount, image.Height / (float)rowCount);
            for (var i = 0; i < rowCount; i++)
            {
                startPoint.X = 0;
                for (var j = 0; j < colCount; j++)
                {
                    var pixelColor = pixels.GetPixelColor((int)(startPoint.X + rectSize.X / 2), (int)(startPoint.Y + rectSize.Y / 2));
                    path[i, j] = pixelColor.Alpha > 0;
                    startPoint.X += rectSize.X;
                }

                startPoint.Y += rectSize.Y;
            }
            pixels.Dispose();
            return path;
        }
        private static void SaveImage(string filePath, SKImage image)
        {
            using (var imageFile = new FileStream(filePath, FileMode.Create))
            {
                var imageData = image.Encode();
                imageData.SaveTo(imageFile);
                imageData.Dispose();
            }
        }
    }
}
