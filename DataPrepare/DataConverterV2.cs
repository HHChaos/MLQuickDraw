using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataPrepare
{
    public struct PathInfo
    {
        public float Length { get; set; }
        public int PickPointCount { get; set; }
        public SKPath Path { get; set; }
    }
    public class DataConverterV2 : IDataConverter
    {
        public int PickPointCount { get; set; }
        public string CovertData(DrawingInfo data)
        {
            var paths = GetSKPaths(data.Data, PickPointCount);
            var points = new List<SKPoint>();
            for (int i = 0; i < paths.Length; i++)
            {
                var pathPoints = GetPathPoints(paths[i]);
                if (pathPoints?.Length > 0)
                    points.AddRange(pathPoints);
            }
            if (points?.Count > 0)
            {
                var dataStr = new StringBuilder();
                dataStr.Append($"{data.Word}");
                for (int i = 0; i < PickPointCount; i++)
                {
                    var index = i > (points.Count - 1) ? points.Count - 1 : i;
                    var point = points[index];
                    dataStr.Append($",{point.X.ToString("f1")}");
                    dataStr.Append($",{point.Y.ToString("f1")}");
                }
                dataStr.Append(Environment.NewLine);
                return dataStr.ToString();
            }
            else
            {
                return null;
            }
        }

        public string GetHeader()
        {
            var headerBuilder = new StringBuilder();
            headerBuilder.Append("Label");
            for (int i = 0; i < PickPointCount; i++)
            {
                headerBuilder.Append($",Point{i}");
                headerBuilder.Append($",Point{i}");
            }
            headerBuilder.Append(Environment.NewLine);
            return headerBuilder.ToString();
        }

        private PathInfo[] GetSKPaths(IEnumerable<int[][]> points, int totalPickPointCount)
        {
            var count = points.Count();
            var skPaths = new PathInfo[count];
            var pathMeasure = new SKPathMeasure();
            var index = 0;
            foreach (var pathData in points)
            {
                var path = new SKPath();
                if (pathData[0].Length == 1)
                {
                    path.MoveTo(new SKPoint(pathData[0][0], pathData[1][0]));
                }
                else if (pathData[0].Length > 1)
                {
                    path.MoveTo(new SKPoint(pathData[0][0], pathData[1][0]));
                    for (int i = 1; i < pathData[0].Length; i++)
                    {
                        path.LineTo(new SKPoint(pathData[0][i], pathData[1][i]));
                    }
                }
                pathMeasure.SetPath(path, false);
                skPaths[index] = new PathInfo
                {
                    Path = path,
                    Length = pathMeasure.Length
                };
                index++;
            }
            pathMeasure.Dispose();
            var totalLength = skPaths.Aggregate<PathInfo, double>(0, (current, item) => current + item.Length);
            for (int i = 0; i < skPaths.Length; i++)
            {
                if (skPaths[i].Length > 0)
                    skPaths[i].PickPointCount = (int)Math.Round(skPaths[i].Length / totalLength * totalPickPointCount);
            }
            return skPaths;
        }

        private SKPoint[] GetPathPoints(PathInfo info)
        {
            if (info.PickPointCount == 0)
                return null;
            var points = new SKPoint[info.PickPointCount];
            var pathMeasure = new SKPathMeasure();
            pathMeasure.SetPath(info.Path,false);
            var disGap = info.Length / info.PickPointCount;
            var halfGap = disGap / 2;
            for (int i = 0; i < info.PickPointCount; i++)
            {
                pathMeasure.GetPosition(disGap * i + halfGap, out var pos);
                points[i] = pos;
            }
            pathMeasure.Dispose();
            info.Path.Dispose();
            return points;
        }
    }
}
