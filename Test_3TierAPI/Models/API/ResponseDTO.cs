namespace Test_3TierAPI.Models.API
{
    public class ResponseDTO<T>
    {
        public string? JobUUID { get; set; } // 작업 UUID
        public bool Success { get; set; } // 성공 여부
        public int StatusCode { get; set; } // HTTP 상태 코드
        public string? Message { get; set; } // 응답 메시지
        public T? Data { get; set; } // 응답 데이터
        public MetaDTO? Meta { get; set; } // 응답 메타 정보
    }
}
