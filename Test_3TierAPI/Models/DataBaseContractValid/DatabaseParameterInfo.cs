using System.Data;

namespace Test_3TierAPI.Models.DataBaseContractValid
{
    /// <summary>
    /// 데이터베이스 파라미터 또는 컬럼의 메타데이터 정보를 담는 컨테이너 클래스
    /// </summary>
    public class DatabaseParameterInfo
    {
        /// <summary>
        /// 파라미터/컬럼 이름
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// SQL 서버 데이터 타입
        /// </summary>
        public SqlDbType DataType { get; set; }

        /// <summary>
        /// .NET 데이터 타입
        /// </summary>
        public Type DotNetType { get; set; }

        /// <summary>
        /// 파라미터/컬럼의 최대 길이 (문자열, 바이너리 타입에 적용)
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// 정밀도 (숫자 타입의 전체 자릿수)
        /// </summary>
        public byte Precision { get; set; }

        /// <summary>
        /// 소수점 자릿수 (숫자 타입의 소수점 이하 자릿수)
        /// </summary>
        public byte Scale { get; set; }

        /// <summary>
        /// NULL 값 허용 여부
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// 필수 여부 (프로시저 파라미터의 경우 기본값이 없고 NULL을 허용하지 않으면 필수)
        /// </summary>
        public bool IsRequired => !IsNullable && HasDefaultValue == false;

        /// <summary>
        /// 기본값 존재 여부
        /// </summary>
        public bool HasDefaultValue { get; set; }

        /// <summary>
        /// 기본값 (있는 경우)
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// 파라미터 방향 (Input, Output, InputOutput, ReturnValue)
        /// </summary>
        public ParameterDirection Direction { get; set; } = ParameterDirection.Input;

        /// <summary>
        /// 주요 키(PK) 여부 (테이블 컬럼에만 적용)
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// 자동 증가(IDENTITY) 속성 여부 (테이블 컬럼에만 적용)
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// 계산된 컬럼(COMPUTED) 여부 (테이블 컬럼에만 적용)
        /// </summary>
        public bool IsComputed { get; set; }

        /// <summary>
        /// CHECK 제약 조건 표현식 (테이블 컬럼에만 적용)
        /// </summary>
        public string CheckConstraint { get; set; }

        /// <summary>
        /// 정규식 패턴 (검증에 사용)
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// 최소값 (숫자 타입에 적용)
        /// </summary>
        public object MinValue { get; set; }

        /// <summary>
        /// 최대값 (숫자 타입에 적용)
        /// </summary>
        public object MaxValue { get; set; }

        /// <summary>
        /// 특정 타입으로 최소값을 반환
        /// </summary>
        public T GetMinValue<T>()
        {
            return MinValue != null ? (T)Convert.ChangeType(MinValue, typeof(T)) : default;
        }

        /// <summary>
        /// 특정 타입으로 최대값을 반환
        /// </summary>
        public T GetMaxValue<T>()
        {
            return MaxValue != null ? (T)Convert.ChangeType(MaxValue, typeof(T)) : default;
        }
    }
}
