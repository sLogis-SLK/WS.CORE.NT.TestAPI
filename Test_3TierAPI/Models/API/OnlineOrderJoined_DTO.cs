namespace SLK_Model
{
    /// <summary>
    /// 온라인 주문 마스터와 상세 조인 DTO (NOT NULL 컬럼만)
    /// </summary>
    public class OnlineOrderJoined_DTO
    {
        // === 온라인_주문마스터 NOT NULL 컬럼들 ===

        /// <summary>
        /// 자체주문번호 - char(16) NOT NULL
        /// </summary>
        public string 자체주문번호 { get; set; }

        /// <summary>
        /// 순번 - int NOT NULL (상세에서 가져옴)
        /// </summary>
        public int 순번 { get; set; }

        /// <summary>
        /// 센터코드 - char(5) NOT NULL
        /// </summary>
        public string 센터코드 { get; set; }

        /// <summary>
        /// B코드 - char(2) NOT NULL
        /// </summary>
        public string B코드 { get; set; }

        /// <summary>
        /// 주문구분 - varchar(5) NOT NULL
        /// </summary>
        public string 주문구분 { get; set; }

        /// <summary>
        /// 합포장기준 - char(1) NOT NULL
        /// </summary>
        public string 합포장기준 { get; set; }

        /// <summary>
        /// 합포장구분 - char(1) NOT NULL
        /// </summary>
        public string 합포장구분 { get; set; }

        /// <summary>
        /// 작업일자 - char(8) NOT NULL
        /// </summary>
        public string 작업일자 { get; set; }

        /// <summary>
        /// 작업차수 - int NOT NULL
        /// </summary>
        public int 작업차수 { get; set; }

        /// <summary>
        /// 주문접수일자 - char(8) NOT NULL
        /// </summary>
        public string 주문접수일자 { get; set; }

        /// <summary>
        /// 주문접수차수 - varchar(5) NOT NULL
        /// </summary>
        public string 주문접수차수 { get; set; }

        /// <summary>
        /// 주문등록일시 - datetime NOT NULL
        /// </summary>
        public DateTime 주문등록일시 { get; set; }

        /// <summary>
        /// 주문자 - nvarchar(50) NOT NULL
        /// </summary>
        public string 주문자 { get; set; }

        /// <summary>
        /// 수령자 - nvarchar(50) NOT NULL
        /// </summary>
        public string 수령자 { get; set; }

        /// <summary>
        /// 수령자휴대폰 - varchar(20) NOT NULL
        /// </summary>
        public string 수령자휴대폰 { get; set; }

        /// <summary>
        /// 수령자연락처 - varchar(20) NOT NULL
        /// </summary>
        public string 수령자연락처 { get; set; }

        /// <summary>
        /// 우편번호 - varchar(7) NOT NULL
        /// </summary>
        public string 우편번호 { get; set; }

        /// <summary>
        /// 주소1 - nvarchar(150) NOT NULL
        /// </summary>
        public string 주소1 { get; set; }

        /// <summary>
        /// 배송메시지 - nvarchar(255) NOT NULL
        /// </summary>
        public string 배송메시지 { get; set; }

        /// <summary>
        /// 몰명 - nvarchar(50) NOT NULL
        /// </summary>
        public string 몰명 { get; set; }

        /// <summary>
        /// 판매금액 - int NOT NULL
        /// </summary>
        public int 판매금액 { get; set; }

        /// <summary>
        /// 결제금액 - int NOT NULL
        /// </summary>
        public int 결제금액 { get; set; }

        /// <summary>
        /// 주문수량계 - int NOT NULL
        /// </summary>
        public int 주문수량계 { get; set; }

        /// <summary>
        /// 배송료 - int NOT NULL
        /// </summary>
        public int 배송료 { get; set; }

        /// <summary>
        /// 배송료구분 - varchar(5) NOT NULL
        /// </summary>
        public string 배송료구분 { get; set; }

        /// <summary>
        /// 몰그룹코드 - varchar(20) NOT NULL
        /// </summary>
        public string 몰그룹코드 { get; set; }

        /// <summary>
        /// 몰코드 - varchar(50) NOT NULL
        /// </summary>
        public string 몰코드 { get; set; }

        /// <summary>
        /// 마스터 상태구분 - varchar(5) NOT NULL
        /// </summary>
        public string 마스터상태구분 { get; set; }

        /// <summary>
        /// 부분배송여부 - bit NOT NULL
        /// </summary>
        public bool 부분배송여부 { get; set; }

        /// <summary>
        /// 부분출고여부 - bit NOT NULL
        /// </summary>
        public bool 부분출고여부 { get; set; }

        /// <summary>
        /// 사전결품대상 - bit NOT NULL
        /// </summary>
        public bool 사전결품대상 { get; set; }

        /// <summary>
        /// 등록자 - varchar(20) NOT NULL
        /// </summary>
        public string 등록자 { get; set; }

        // === 온라인_주문상세 NOT NULL 컬럼들 (겹치는 것 제외) ===

        /// <summary>
        /// 주문번호 - varchar(50) NOT NULL
        /// </summary>
        public string 주문번호 { get; set; }

        /// <summary>
        /// 서브키 - varchar(50) NOT NULL
        /// </summary>
        public string 서브키 { get; set; }

        /// <summary>
        /// 몰상품코드 - nvarchar(60) NOT NULL
        /// </summary>
        public string 몰상품코드 { get; set; }

        /// <summary>
        /// 상품명 - nvarchar(255) NOT NULL
        /// </summary>
        public string 상품명 { get; set; }

        /// <summary>
        /// 수량 - int NOT NULL
        /// </summary>
        public int 수량 { get; set; }

        /// <summary>
        /// 작업 - int NOT NULL
        /// </summary>
        public int 작업 { get; set; }

        /// <summary>
        /// 분류 - int NOT NULL
        /// </summary>
        public int 분류 { get; set; }

        /// <summary>
        /// 소비자단가 - int NOT NULL
        /// </summary>
        public int 소비자단가 { get; set; }

        /// <summary>
        /// 판매단가 - int NOT NULL
        /// </summary>
        public int 판매단가 { get; set; }

        /// <summary>
        /// 공급단가 - int NOT NULL
        /// </summary>
        public int 공급단가 { get; set; }

        /// <summary>
        /// 상품코드 - varchar(60) NOT NULL
        /// </summary>
        public string 상품코드 { get; set; }

        /// <summary>
        /// 운송장발행회수 - int NOT NULL
        /// </summary>
        public int 운송장발행회수 { get; set; }

        /// <summary>
        /// 상세 B코드 - char(2) NOT NULL
        /// </summary>
        public string 상세B코드 { get; set; }

        /// <summary>
        /// 상세 몰그룹코드 - varchar(20) NOT NULL
        /// </summary>
        public string 상세몰그룹코드 { get; set; }

        /// <summary>
        /// 상세 몰코드 - varchar(50) NOT NULL
        /// </summary>
        public string 상세몰코드 { get; set; }

        /// <summary>
        /// 상세 상태구분 - varchar(5) NOT NULL
        /// </summary>
        public string 상세상태구분 { get; set; }

        /// <summary>
        /// 택배사전송 - bit NOT NULL
        /// </summary>
        public bool 택배사전송 { get; set; }

        /// <summary>
        /// 출고확정 - bit NOT NULL
        /// </summary>
        public bool 출고확정 { get; set; }

        /// <summary>
        /// 반품구분 - bit NOT NULL
        /// </summary>
        public bool 반품구분 { get; set; }
    }
}
