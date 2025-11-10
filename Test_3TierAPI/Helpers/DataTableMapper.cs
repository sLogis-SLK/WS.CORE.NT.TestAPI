using System.Data;

namespace Test_3TierAPI.Helpers
{
    public static class DataTableMapper
    {
        /// <summary>
        /// DataTable을 List<T>로 매핑
        /// 컬럼명과 속성명이 일치해야 동작
        /// </summary>
        public static List<T> MapToList<T>(DataTable table) where T : new()
        {
            List<T> list = new List<T>();

            foreach (DataRow row in table.Rows)
            {
                T obj = new T();

                foreach (DataColumn column in table.Columns)
                {
                    var property = typeof(T).GetProperty(column.ColumnName);

                    if (property != null && row[column] != DBNull.Value)
                    {
                        try
                        {
                            var value = Convert.ChangeType(row[column], Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
                            property.SetValue(obj, value);
                        }
                        catch
                        {
                            // 무시하거나 로깅
                        }
                    }
                }

                list.Add(obj);
            }

            return list;
        }
    }
}
