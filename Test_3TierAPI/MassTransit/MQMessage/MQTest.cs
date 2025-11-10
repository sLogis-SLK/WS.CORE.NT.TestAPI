namespace Test_3TierAPI.MassTransit.MQMessage
{
    public class MQTest
    {
        public string UUID { get; set; }
        public DateTime CreatedAt { get; set; }

        public string MaxSequence { get; set; }
        public string Sequence { get; set; } 

        public List<OnlineOrderMaster_Model> Orders { get; set; }
    }
}
