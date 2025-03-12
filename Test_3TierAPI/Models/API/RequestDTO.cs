using LinqToDB.SqlQuery;
using System.Data;

namespace Test_3TierAPI.Models.API
{
    public class RequestDTO<T>
    {
        public string? Requester { get; set; }  // 요청자
        public bool bIsBypass { get; set; }     // Bypass 여부
        public DateTime RequestTimestamp { get; set; } // 요청 시각
        public string? ApiVersion { get; set; } // API 버전(필요 시 사용)
        public string? TraceId { get; set; }    // 분산 트랜잭션 추적 ID(필요 시 사용)
        public string? ClientInfo { get; set; } // 클라이언트 정보(필요 시 사용)
        public T? Data { get; set; }            // 요청 데이터
    }
}
