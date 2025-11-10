namespace Test_3TierAPI.MassTransit.MQMessage
{
    public class DTO_APIGateway <T>
    {
        public string RequestId { get; set; }          // 요청 ID (string)
        public string TimeAt { get; set; }  // 요청 시간 (DateTime)
        public T _requestData { get; set; }  // 요청 데이터 (제네릭 타입 T)
    }
}

