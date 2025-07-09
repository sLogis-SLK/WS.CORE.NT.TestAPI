using LinqToDB.SchemaProvider;

namespace Test_3TierAPI.Models.DataBaseContractValid
{
    public class DatabaseMetadata
    {
        /// <summary>
        /// 데이터베이스 객체(저장 프로시저 또는 테이블)의 메타데이터 정보를 담는 컨테이너 클래스
        /// </summary>
        public class DatabaseMetadata
        {
            /// <summary>
            /// 데이터베이스 객체 이름
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 소유자(스키마) 이름 (dbo, sys 등)
            /// </summary>
            public string SchemaName { get; set; }

            /// <summary>
            /// 객체 유형 (프로시저 또는 테이블)
            /// </summary>
            public DatabaseObjectType ObjectType { get; set; }

            /// <summary>
            /// 객체 생성 일자
            /// </summary>
            public DateTime CreateDate { get; set; }

            /// <summary>
            /// 객체 수정 일자
            /// </summary>
            public DateTime ModifyDate { get; set; }

            /// <summary>
            /// 파라미터 또는 컬럼 정보 목록
            /// </summary>
            public List<DatabaseParameterInfo> Parameters { get; set; } = new List<DatabaseParameterInfo>();

            /// <summary>
            /// 테이블인 경우, 주요 키(PK) 컬럼 이름 목록
            /// </summary>
            public List<string> PrimaryKeyColumns { get; set; } = new List<string>();

            /// <summary>
            /// 테이블인 경우, 외래 키(FK) 제약 조건 정보
            /// </summary>
            public List<ForeignKeyInfo> ForeignKeys { get; set; } = new List<ForeignKeyInfo>();

            /// <summary>
            /// 추가 메타데이터 정보 (확장성을 위한 딕셔너리)
            /// </summary>
            public Dictionary<string, object> AdditionalMetadata { get; set; } = new Dictionary<string, object>();

            /// <summary>
            /// 특정 이름의 파라미터/컬럼을 찾아 반환
            /// </summary>
            public DatabaseParameterInfo GetParameter(string name)
            {
                return Parameters.Find(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// 데이터베이스 객체 유형
        /// </summary>
        public enum DatabaseObjectType
        {
            StoredProcedure,
            Table,
            View,
            Function
        }

        /// <summary>
        /// 외래 키 제약 조건 정보
        /// </summary>
        public class ForeignKeyInfo
        {
            /// <summary>
            /// 제약 조건 이름
            /// </summary>
            public string ConstraintName { get; set; }

            /// <summary>
            /// 참조하는 컬럼 이름
            /// </summary>
            public string ColumnName { get; set; }

            /// <summary>
            /// 참조되는 테이블 이름
            /// </summary>
            public string ReferencedTable { get; set; }

            /// <summary>
            /// 참조되는 컬럼 이름
            /// </summary>
            public string ReferencedColumn { get; set; }
        }
    }
}
