using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json;

namespace NoiseReducer
{
    public static class DataSmoothering
    {
        const int MULTIPLIER = 256;
        const int PADDING = 128;
        const float INNER_PADDING_OFFSET = 2.5f;
        const int X_DIVISION_AMOUNT = 8;
        const int Y_DIVISION_AMOUNT = 8;


        static readonly int WIDTH_MULTIPLIER = MULTIPLIER * 2;
        static readonly int offset = PADDING / 2;

        static SpeedAnalytic logs = new();
        static double distanceStartingPoint;
        static double distanceEndingPoint;
        static double speedStartingPoint;
        static double speedEndingPoint;
        static int width;
        static int height;


        public static void Init(string fipepath, string DiagramImgSavePath)
        {
            string path = fipepath;  // /*Console.ReadLine();*/ @"C:\Users\SoaPisGirseb\Desktop\teas\test.json";
            logs = JsonSerializer.Deserialize<SpeedAnalytic>(File.ReadAllText(path));
            distanceStartingPoint = logs.Distances[0].Distance;
            distanceEndingPoint = logs.Distances[logs.Distances.Count - 1].Distance;
            speedStartingPoint = logs.Distances.MinBy(x => x.Speed).Speed;
            speedEndingPoint = logs.Distances.MaxBy(x => x.Speed).Speed;



            logs.Distances = RemoveNoise(logs.Distances, 0.5, 0.4);
            width = (int)(logs.Distances[logs.Distances.Count - 1].Distance - distanceStartingPoint) * WIDTH_MULTIPLIER + PADDING;
            height = (int)((speedEndingPoint - speedStartingPoint) * MULTIPLIER) + PADDING;

            DrawImage(DiagramImgSavePath);
        }
        public static List<DistanceRange> FindMeasurementAreas(List<Interval> intervals, double minPeakDistance, double noiseThreshold)
        {
            List<DistanceRange> measurementAreas = new List<DistanceRange>();
            List<DistancePeak> peaks = FindPeaks(intervals, minPeakDistance);
            List<DistancePeak> filteredPeaks = FilterNoise(intervals, peaks, noiseThreshold);

            foreach (var peak in filteredPeaks)
            {
                Interval minInterval = intervals.OrderBy(i => i.Distance).SkipWhile(i => i.Distance < peak.StartDistance).FirstOrDefault();
                Interval maxInterval = intervals.OrderByDescending(i => i.Distance).SkipWhile(i => i.Distance > peak.EndDistance).FirstOrDefault();

                if (minInterval != null && maxInterval != null)
                {
                    measurementAreas.Add(new DistanceRange(minInterval.Distance, maxInterval.Distance));
                }
            }

            return measurementAreas;
        }

        // Helper functions for peak detection, noise filtering

        public static List<DistancePeak> FindPeaks(List<Interval> intervals, double minPeakDistance)
        {
            List<DistancePeak> peaks = new List<DistancePeak>();
            double prevDistance = double.MinValue;

            for (int i = 1; i < intervals.Count; i++)
            {
                if (intervals[i].Distance > prevDistance)
                {
                    peaks.Add(new DistancePeak(prevDistance, intervals[i].Distance));
                    prevDistance = intervals[i].Distance;
                }
                else if (intervals[i].Distance < prevDistance && intervals[i].Distance - prevDistance >= minPeakDistance)
                {
                    peaks.Add(new DistancePeak(prevDistance, intervals[i].Distance));
                }
            }

            return peaks;
        }

        public static List<DistancePeak> FilterNoise(List<Interval> intervals, List<DistancePeak> peaks, double noiseThreshold)
        {
            List<DistancePeak> filteredPeaks = new List<DistancePeak>();
            foreach (var peak in peaks)
            {
                double averageSpeed = intervals.Where(i => i.Distance >= peak.StartDistance && i.Distance <= peak.EndDistance)
                                               .Average(i => i.Speed);
                if (averageSpeed >= noiseThreshold)
                {
                    filteredPeaks.Add(peak);
                }
            }

            return filteredPeaks;
        }


        public static List<Interval> RemoveNoise(List<Interval> intervals, double minPeakDistance, double noiseThreshold)
        {
            List<DistanceRange> measurementAreas = FindMeasurementAreas(intervals, minPeakDistance, noiseThreshold);
            List<Interval> filteredIntervals = new List<Interval>();
            List<Interval> notTobeAdded = new List<Interval>();

            for (int i = 0; i < intervals.Count; i++)
            {
                Interval currentInterval = intervals[i];

                // Check if the interval falls within any measurement area
                bool isMeasurementArea = measurementAreas.Any(area => area.StartDistance <= currentInterval.Distance && area.EndDistance >= currentInterval.Distance);

                if (i < intervals.Count - 1)
                {
                    Interval nextInterval = intervals[i + 1];
                    double speedChange = nextInterval.Speed - currentInterval.Speed;

                    // Check if the speed change is negative and significant
                    if (speedChange < 0 && Math.Abs(speedChange) > 0.5)
                    {
                        notTobeAdded.Add(nextInterval);
                    }
                }

                // If it's a measurement area interval and not marked for exclusion, add it
                if (isMeasurementArea && !notTobeAdded.Contains(currentInterval))
                {
                    filteredIntervals.Add(currentInterval);
                }
            }

            return filteredIntervals;
        }

        static void DrawImage(string DiagramImgSavePath)
        {

            var (innerWidth, innerHeight) = (width + PADDING * 4, height + PADDING * 4);
            var (innerWidthWithOffset, innerHeightWithOffset) = (width + PADDING * INNER_PADDING_OFFSET, height + PADDING * INNER_PADDING_OFFSET);

            var image = new Bitmap(innerWidth, innerHeight);
            Graphics graphics = Graphics.FromImage(image);
            GraphicsPath graphicsPath = new();
            GraphicsPath gridPath = new();

            Pen pen = new Pen(Color.White);
            Pen graphPen = new Pen(Color.Blue, 1);
            Pen gridPen = new Pen(Color.Black, 1);
            Brush brush = new SolidBrush(Color.White);

            float xProgressionValueAmount = (float)(speedEndingPoint - speedStartingPoint) / X_DIVISION_AMOUNT;
            float yProgressionValueAmount = (float)(distanceEndingPoint - distanceStartingPoint) / Y_DIVISION_AMOUNT;

            float xProgressionScreenAmount = (float)(innerWidthWithOffset) / X_DIVISION_AMOUNT;
            float yProgressionScreenAmount = (float)(innerHeightWithOffset) / Y_DIVISION_AMOUNT;


            //Draw Graph
            for (var i = 0; logs.Distances.Count() > i; i++)
            {
                Interval firstPoint = logs.Distances[i];
                if (logs.Distances.Count() > i + 1)
                {
                    Interval secondPoint = logs.Distances[i + 1];
                    graphicsPath.AddLine(GetDistance(firstPoint.Distance), GetSpeed(firstPoint.Speed), GetDistance(secondPoint.Distance), GetSpeed(secondPoint.Speed));
                }
            }

            //Draw Grid X
            for (var i = 1; X_DIVISION_AMOUNT >= i; i++)
            {
                gridPath.AddLine(xProgressionScreenAmount * i, 0, xProgressionScreenAmount * i, innerHeightWithOffset);
                gridPath.StartFigure();
            }

            //Draw Grid Y
            for (var i = 1; Y_DIVISION_AMOUNT >= i; i++)
            {
                gridPath.AddLine(0, yProgressionScreenAmount * i, innerWidthWithOffset, yProgressionScreenAmount * i);
                gridPath.StartFigure();
            }



            graphics.FillRectangle(brush, 0, 0, innerWidth, innerHeight);
            graphics.DrawPath(gridPen, gridPath);
            graphics.DrawPath(graphPen, graphicsPath);

            image.RotateFlip(RotateFlipType.Rotate180FlipX);




            image.Save(@"C:\Users\SoaPisGirseb\Desktop\teas\image.png");

            image.Dispose();
            graphics.Dispose();
            pen.Dispose();
            graphPen.Dispose();
            gridPen.Dispose();
            brush.Dispose();
        }

        static int GetSpeed(double speed)
        {
            return (int)(speed * MULTIPLIER + PADDING * INNER_PADDING_OFFSET);
        }

        static int GetDistance(double distance)
        {
            return (int)((distance - distanceStartingPoint) * WIDTH_MULTIPLIER);
        }
    }

}
