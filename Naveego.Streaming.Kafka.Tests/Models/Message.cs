using System;

namespace Naveego.Streaming.Kafka.Tests.Models
{
    public class Message
    {
        public DateTime Time { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Company { get; set; }
        public int DistanceInMiles { get; set; }
    }
}