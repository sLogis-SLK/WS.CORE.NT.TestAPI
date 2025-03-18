using System.Data;
using System.Data.Common;

namespace Test_3TierAPI.Infrastructure.DataBase
{
    public class DatabaseQueryBuilder : IAsyncDisposable
    {
        private readonly Task<DbDataReader> _readerTask;
        private readonly Func<Task<DbConnection>> _connectionProvider;

        public DatabaseQueryBuilder(Task<DbDataReader> readerTask, Func<Task<DbConnection>> connectionProvider)
        {
            _readerTask = readerTask;
            _connectionProvider = connectionProvider;
        }

        public async Task<DataTable> ToDataTableAsync()
        {
            var dataTable = new DataTable();
            await using var reader = await _readerTask;
            dataTable.Load(reader);
            return dataTable;
        }

        public async Task<List<Dictionary<string,object>>> ToDictionaryListAsync()
        {
            var list = new List<Dictionary<string, object>>();
            await using var reader = await _readerTask;

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                list.Add(row);
            }

            return list;
        }

        public async Task<T> ToScalarAsync<T>()
        {
            await using var reader = await _readerTask;
            if (await reader.ReadAsync())
            {
                object value = reader.GetValue(0);
                return value == DBNull.Value ? default : (T)Convert.ChangeType(value, typeof(T));
            }
            return default;
        }

        public async ValueTask DisposeAsync()
        {
            await using var connection = await _connectionProvider();
            await connection.DisposeAsync();
        }
    }
}
