//using MassTransit;
//using Test_3TierAPI.MassTransit.MQMessage;

//namespace Test_3TierAPI.MassTransit.Consumer
//{
//    public class MQTestConsumer : IConsumer<MQTest>
//    {
//        private readonly ILogger<MQTestConsumer> _logger;

//        public MQTestConsumer(ILogger<MQTestConsumer> logger)
//        {
//            _logger = logger;
            
//        }

//        public async Task Consume(ConsumeContext<MQTest> context)
//        {
//            // Log the received message
//            //Console.WriteLine($"Received MQTest message with UUID: {context.Message.UUID} at {context.Message.CreatedAt}");
//            _logger.LogWarning($"!!!!!!!!!!Received MQTest message with UUID: {context.Message.UUID} at {context.Message.CreatedAt}");
//            // Simulate some processing
//            await Task.CompletedTask;
//        }
//    }
//}
