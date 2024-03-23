
namespace NoiseReducer
{
    public class SpeedAnalytic
    {
        public string Id { get; set; }
        public string IdTrace { get; set; }
        public List<Interval> Distances { get; set; }
    }
    public class DistancePeak
    {
        public double StartDistance { get; set; }
        public double EndDistance { get; set; }

        public DistancePeak(double startDistance, double endDistance)
        {
            StartDistance = startDistance;
            EndDistance = endDistance;
        }
    }

    public class DistanceRange
    {
        public double StartDistance { get; set; }
        public double EndDistance { get; set; }

        public DistanceRange(double startDistance, double endDistance)
        {
            StartDistance = startDistance;
            EndDistance = endDistance;
        }
    }
    public class Interval
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public int ElapsedTime { get; set; }
        public double Speed { get; set; }
        public double SNR1 { get; set; }
        public double Distance { get; set; }
    }
}
