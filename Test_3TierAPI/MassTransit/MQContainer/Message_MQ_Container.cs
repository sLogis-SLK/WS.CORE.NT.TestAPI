using Test_3TierAPI.MassTransit.MQMessage;

namespace MQ_Message
{
    public class Message_MQ_Container
    {
        public int? MessageAmount { get; set; } // 해당 메시지의 총 수량
        public string? Sequence { get; set; } // 해당 메시지의 순번
        public string? UUID { get; set; } // UUID (Guid)
        public List<OnlineOrderMaster_Model>? Orders { get; set; } // 온라인 주문 마스터 DTO 리스트
    }
}
