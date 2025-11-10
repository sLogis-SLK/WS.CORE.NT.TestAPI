namespace Test_3TierAPI.MassTransit.MQMessage
{
    /// <summary>
    /// 온라인 주문 마스터 DTO
    /// </summary>
    public class OnlineOrderMaster_Model
    {
        public string 자체주문번호 { get; set; }                 // 자체주문번호 (char(16))
        public string 센터코드 { get; set; }                     // 센터코드 (char(5))
        public string B코드 { get; set; }                       // B코드 (char(2))
        public string 주문구분 { get; set; }                    // 주문구분 (varchar(5))
        public string 합포장기준 { get; set; }                  // 합포장기준 (char(1))
        public string 합포장구분 { get; set; }                  // 합포장구분 (char(1))
        public string 작업일자 { get; set; }                    // 작업일자 (char(8), yyyyMMdd)
        public int? 작업차수 { get; set; }                      // 작업차수
        public string 주문접수일자 { get; set; }                // 주문접수일자 (char(8))
        public string 주문접수차수 { get; set; }                // 주문접수차수 (varchar(5))
        public DateTime? 주문등록일시 { get; set; }              // 주문등록일시 (datetime)
        public string 주문자 { get; set; }                      // 주문자 (nvarchar(50))
        public string 주문자휴대폰 { get; set; }                // 주문자휴대폰 (varchar(20))
        public string 주문자연락처 { get; set; }                // 주문자연락처 (varchar(20))
        public string 수령자 { get; set; }                      // 수령자 (nvarchar(50))
        public string 수령자휴대폰 { get; set; }                // 수령자휴대폰 (varchar(20))
        public string 수령자연락처 { get; set; }                // 수령자연락처 (varchar(20))
        public string 우편번호 { get; set; }                    // 우편번호 (varchar(7))
        public string 주소1 { get; set; }                      // 주소1 (nvarchar(150))
        public string 주소2 { get; set; }                      // 주소2 (nvarchar(100))
        public string 이메일 { get; set; }                     // 이메일 (varchar(100))
        public string 배송메시지 { get; set; }                  // 배송메시지 (nvarchar(255))
        public string 주문자메시지 { get; set; }                // 주문자메시지 (nvarchar(255))
        public string 메모 { get; set; }                       // 메모 (nvarchar(255))
        public string 사은품 { get; set; }                     // 사은품 (nvarchar(255))
        public string 몰명 { get; set; }                       // 몰명 (nvarchar(50))
        public int? 판매금액 { get; set; }                      // 판매금액 (int)
        public int? 결제금액 { get; set; }                      // 결제금액 (int)
        public int? 주문수량계 { get; set; }                    // 주문수량계 (int)
        public int? 배송료 { get; set; }                        // 배송료 (int)
        public string 배송료구분 { get; set; }                  // 배송료구분 (varchar(5))
        public string 택배사코드 { get; set; }                  // 택배사코드 (varchar(5))
        public string 운송장번호 { get; set; }                  // 운송장번호 (varchar(30))
        public string 자체원주문번호 { get; set; }               // 자체원주문번호 (char(16))
        public string 작지번호 { get; set; }                    // 작지번호 (varchar(13))
        public string 몰그룹코드 { get; set; }                  // 몰그룹코드 (varchar(20))
        public string 몰코드 { get; set; }                      // 몰코드 (varchar(50))
        public string 상태구분 { get; set; }                    // 상태구분 (varchar(5))
        public bool? 부분배송여부 { get; set; }                 // 부분배송여부 (bit)
        public bool? 부분출고여부 { get; set; }                 // 부분출고여부 (bit)
        public bool? 사전결품대상 { get; set; }                 // 사전결품대상 (bit)
        public string 등록자 { get; set; }                     // 등록자 (varchar(20))
        public DateTime? 등록일시 { get; set; }                 // 등록일시 (datetime)
        public string 예비용01 { get; set; }                   // 예비용01
        public string 예비용02 { get; set; }                   // 예비용02
        public string 예비용03 { get; set; }                   // 예비용03
        public string 예비용04 { get; set; }                   // 예비용04
        public string 예비용05 { get; set; }                   // 예비용05
        public string 처리용01 { get; set; }                   // 처리용01
        public string 처리용02 { get; set; }                   // 처리용02
        public string 처리용03 { get; set; }                   // 처리용03 (varchar(500))
        public string 주문유형 { get; set; }                    // 주문유형 (varchar(5))
        public string 공동현관비번 { get; set; }                // 공동현관비번 (varchar(50))
        public int? 채번번호 { get; set; }                      // 채번번호 (int)
        public string DOT번호 { get; set; }                    // DOT번호 (char(9))
        public string HOSTKEY1 { get; set; }                   // HOSTKEY1
        public string HOSTKEY2 { get; set; }                   // HOSTKEY2
        public string HOSTKEY3 { get; set; }                   // HOSTKEY3
        public string 호스트마스터키값 { get; set; }            // 호스트마스터키값 (nvarchar(1000))
        public string 피킹모듈 { get; set; }                    // 피킹모듈
        public string 피킹유형 { get; set; }                    // 피킹유형
        public string 피킹후공정 { get; set; }                  // 피킹후공정
        public string 패킹모듈 { get; set; }                    // 패킹모듈
        public string 연결구분자 { get; set; }                  // 연결구분자
        public string 운송장유형 { get; set; }                  // 운송장유형
        public string 상품형태 { get; set; }                    // 상품형태
        public string 분류유형 { get; set; }                    // 분류유형
        public string 예약주문여부 { get; set; }                // 예약주문여부
        public string 사용자그룹핑1 { get; set; }               // 사용자그룹핑1
        public string 사용자그룹핑2 { get; set; }               // 사용자그룹핑2
        public string 사용자그룹핑3 { get; set; }               // 사용자그룹핑3
    }
}
