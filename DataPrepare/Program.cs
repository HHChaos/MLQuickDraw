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
            IDataConverter dataConverter = null;
            var useConverterV1 = true;
            Console.WriteLine("键入使用V1还是V2数据转换：（输入1或者2，推荐使用V2，训练时间短，效果也好的多）");
            if (int.TryParse(Console.ReadLine(), out var inputChoice))
            {
                if (inputChoice != 1)
                    useConverterV1 = false;
            }
            if (useConverterV1)
            {
                var dataConverterV1 = new DataConverterV1();
                //Console.WriteLine("键入点阵图保存精度（例如28X28输入28,默认为28）：");
                //if (!int.TryParse(Console.ReadLine(), out var pixelWidth))
                //{
                //    pixelWidth = 28;
                //}
                //dataConverterV1.PixelWidth = pixelWidth;
                dataConverterV1.PixelWidth = 28;
                dataConverterV1.NeedSaveImage = false;
                dataConverterV1.ImageSaveFolder = directoryInfo.CreateSubdirectory("Images");
                dataConverter = dataConverterV1;
            }
            else
            {
                var dataConverterV2 = new DataConverterV2();
                dataConverterV2.PickPointCount = 150;
                dataConverter = dataConverterV2;
            }


            var files = directoryInfo.GetFiles("*.ndjson");
            if (files?.Length > 0)
            {
                var streamReaders = new List<StreamReader>();
                using (var dataFile = new FileStream($"{directoryInfo.FullName}\\data{numberLimit}.csv", FileMode.Create))
                {
                    var header = dataConverter.GetHeader();
                    if (!string.IsNullOrEmpty(header))
                    {
                        var buf = Encoding.UTF8.GetBytes(header);
                        dataFile.Write(buf, 0, buf.Length);
                    }
                    foreach (var ndjson in files)
                    {
                        streamReaders.Add(ndjson.OpenText());
                    }

                    var rand = new Random();

                    for (int i = 0; i < numberLimit; i++)
                    {
                        var dataList = new List<DrawingInfo>();
                        foreach (var reader in streamReaders)
                        {
                            DrawingInfo data = null;
                            do
                            {
                                if (reader.EndOfStream)
                                    break;
                                data = JsonConvert.DeserializeObject<DrawingInfo>(reader.ReadLine());
                            } while (data.Recognized == false);
                            if (data != null)
                            {
                                dataList.Add(data);
                            }
                        }

                        while (dataList.Count > 0)
                        {
                            var index = rand.Next(dataList.Count - 1);
                            var dataStr = dataConverter.CovertData(dataList[index]);
                            if (!string.IsNullOrEmpty(dataStr))
                            {
                                var buf = Encoding.UTF8.GetBytes(dataStr);
                                dataFile.Write(buf, 0, buf.Length);
                            }
                            dataList.RemoveAt(index);
                        }
                    }
                    dataFile.Flush();
                    dataFile.Close();
                }
            }
        }
    }
}
