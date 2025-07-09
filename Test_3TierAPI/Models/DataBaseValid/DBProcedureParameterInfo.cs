namespace Test_3TierAPI.Models.DataBaseValid
{
    /// <summary>
     /// 프로시저 파라미터 정보를 담는 클래스
     /// </summary>
    public class DBProcedureParameterInfo
    {
        public string Name { get; set; }             // 파라미터 이름 (@를 제외한 이름)
        public string DataType { get; set; }         // 데이터 타입 (char, varchar, int 등)
        public int? MaxLength { get; set; }          // 최대 길이 (문자열 타입)
        public int? Precision { get; set; }          // 정밀도 (numeric 타입)
        public int? Scale { get; set; }              // 소수점 자릿수 (numeric 타입)
        public bool HasDefaultValue { get; set; }    // 기본값 존재 여부
        public bool IsOutput { get; set; }           // OUTPUT 파라미터 여부
    }
}
