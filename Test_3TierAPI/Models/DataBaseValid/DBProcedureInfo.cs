

namespace Test_3TierAPI.Models.DataBaseValid
{
    /// <summary>
    /// 프로시저 정보 컨테이너 클래스
    /// 프로시저의 파라미터 목록과 메타데이터를 저장
    /// </summary>
    public class DBProcedureInfo
    {
        /// <summary>
        /// 프로시저 이름
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 프로시저 파라미터 정보 목록
        /// </summary>
        public List<DBProcedureParameterInfo> Parameters { get; set; } = new List<DBProcedureParameterInfo>();

        /// <summary>
        /// 프로시저의 마지막 수정 시간 (sys.objects.modify_date)
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// 캐시에 저장된 시간
        /// </summary>
        public DateTime CachedTime { get; set; }

        /// <summary>
        /// 캐시 소스 정보 (어디서 데이터를 가져왔는지)
        /// </summary>
        public string Source { get; set; } = "New";

        /// <summary>
        /// 파라미터가 있는지 여부
        /// </summary>
        public bool HasParameters => Parameters.Count > 0;
    }
}
