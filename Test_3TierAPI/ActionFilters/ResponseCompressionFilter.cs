using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Data;
using System.IO.Compression;
using System.Text;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.ActionFilters
{
    public class ResponseCompressionFilter : IActionFilter
    {
        private readonly ILogger<ResponseCompressionFilter> _logger;

        public ResponseCompressionFilter(ILogger<ResponseCompressionFilter> logger)
        {
            _logger = logger;
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // 컨트롤러 실행 전 수행되어야 할 로직 (현재 필요 없음)
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // 이미 오류가 발생한 경우 압축을 시도하지 않음
            if (context.Exception != null) return;

            // ApiResponseFilter가 생성한 ResponseDTO 가져오기
            if (context.Result is ObjectResult result &&
                result.Value is ResponseDTO<object> responseDto &&
                responseDto.Data is DataTable dataTable)
            {
                CompressDataTable(context, responseDto, dataTable);
            }
        }

        private void CompressDataTable(ActionExecutedContext context, ResponseDTO<object> responseDto, DataTable dataTable)
        {
            try
            {
                string dataJson = JsonConvert.SerializeObject(dataTable);
                
                // json 데이터 압축
                byte[] compressedData = CompressString(dataJson);

                // 압축된 데이터를 Base64로 인코딩
                string compressedBase64 = Convert.ToBase64String(compressedData);

                // 원본 데이터를 압축 데이터로 교체
                responseDto.Data = compressedBase64;

                // 압축 여부 표시 헤더 추가
                context.HttpContext.Response.Headers.Add("X-Content-Compressed", "true");

                // 테이블 행 수와 압축률 로깅
                int rowCount = dataTable.Rows.Count;

                double compressionRatio = ((double)compressedData.Length / dataJson.Length * 100.0);

                _logger.LogInformation(
                    "Data compression applied: {PN} {RowCount} rows, Compression ratio: {CompressionRatio:F2}%, " +
                    "Original size: {OriginalSize} bytes, Compressed size: {CompressedSize} bytes",
                    responseDto.ProcedureName, rowCount, compressionRatio, dataJson.Length, compressedData.Length);
            }
            catch (Exception ex)
            {
                // 압축 중 발생한 예외는 로그만 남기고 원본 데이터를 유지
                // 주요 기능에 영향을 주지 않도록 예외를 삼킴
                _logger.LogError(ex, "Error during response compression - keeping original data");
            }
        }

        // 문자열을 GZip으로 압축
        private byte[] CompressString(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(outputStream, CompressionLevel.Fastest))
                {
                    gzipStream.Write(bytes, 0, bytes.Length);
                }
                return outputStream.ToArray();
            }
        }
    }
}
