//using MassTransit;
//using MQ_Message;


//namespace Test_3TierAPI.MassTransit.Consumer
//{
//    public class Message_MQ_ContainerConsumer : IConsumer<Message_MQ_Container>
//    {
//        private readonly ILogger<Message_MQ_ContainerConsumer> _logger;
//        public Message_MQ_ContainerConsumer(ILogger<Message_MQ_ContainerConsumer> logger)
//        {
//            _logger = logger;
//        }
//        public async Task Consume(ConsumeContext<Message_MQ_Container> context)
//        {
//            // Log the received message
//            _logger.LogInformation($"MicroService Server Consumed message [ Sequence: {context.Message.Sequence} at {DateTime.Now} ]");
//            // Simulate some processing

//            await Task.Delay(1000);
            
//            await Task.CompletedTask;
//        }
//    }
//}
