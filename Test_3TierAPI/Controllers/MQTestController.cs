using Azure;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using MQ_Message;
using Newtonsoft.Json;
using SLK_Model;
using System.Data;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using Test_3TierAPI.CustomAttribute;
using Test_3TierAPI.Helpers;
using Test_3TierAPI.Infrastructure.DataBase;
using Test_3TierAPI.MassTransit.MQMessage;
using Test_3TierAPI.Repositories;
using Test_3TierAPI.Services;
using SLK.NT.Common.Model;

namespace Test_3TierAPI.Controllers
{
    [ApiController]
    [Route("api/mqtest")]
    [RequireData(false)]
    public class MQTestController : ControllerBase
    {
        private readonly ILogger<MQTestController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly DatabaseTransactionManager _dbManager;
        private readonly IHttpClientFactory _httpClientFactory; // HttpClient 대신 Factory 사용

        public MQTestController(
            ILogger<MQTestController> logger,
            IPublishEndpoint publishEndpoint,
            DatabaseTransactionManager databaseTransactionManager,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _dbManager = databaseTransactionManager;
            _httpClientFactory = httpClientFactory; // Factory 저장
        }

        //[HttpPost("onlineorderbulk")]
        //public async Task<IActionResult> OnlineOrderBulk([FromBody] string message)
        //{
        //    try
        //    {
        //        string query = @"
        //            SELECT TOP(10000) *
        //            FROM 온라인_주문마스터 WITH (NOLOCK)
        //            WHERE 센터코드 = 'TLK09'
        //            AND B코드 = 'HB'
        //            AND 작업일자 BETWEEN '20250101' AND '20250131'";

        //        DataTable returnTable = await _dbManager.GetDataTableAsync("02", query, CommandType.Text, null);
        //        List<Model_Online_Order_Master> orderList = DataTableMapper.MapToList<Model_Online_Order_Master>(returnTable);
        //        List<List<Model_Online_Order_Master>> splitedList = DataSplitter.SplitList(orderList, 100);

        //        int maxSequence = splitedList.Count;

        //        for (int i = 0; i < maxSequence; i++)
        //        {
        //            var container = new Message_MQ_Container
        //            {
        //                MessageAmount = maxSequence,
        //                UUID = Guid.NewGuid().ToString(),
        //                Sequence = (i + 1).ToString(),
        //                Orders = splitedList[i]
        //            };

        //            _logger.LogInformation($"Orchestration Service publish Message [ Data Sequence: {container.Sequence} ]");

        //            await _publishEndpoint.Publish(container, context =>
        //            {
        //                context.SetPriority(10);    // fanout에서도 정상 작동
        //                context.Durable = true;     // 메시지 persistent 설정 : 서버 disk에 저장
        //            });
        //        }

        //        _logger.LogInformation("All messages published successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while publishing bulk online orders");
        //        return StatusCode(500, "Internal server error");
        //    }

        //    return Ok("Bulk publish success");
        //}

        //[HttpPost("onlineordersingle")]
        //public async Task<IActionResult> OnlineOrderSingle([FromBody] string message)
        //{
        //    try
        //    {
        //        var container = new Message_MQ_Container
        //        {
        //            MessageAmount = 1,
        //            UUID = Guid.NewGuid().ToString(),
        //            Sequence = "99999"
        //        };

        //        _logger.LogInformation($"Orchestration Service publish HIGH PRIORITY Message [ Data Sequence: {container.Sequence} ]");

        //        await _publishEndpoint.Publish(container, context =>
        //        {
        //            context.SetPriority(90); // 높은 우선순위
        //            context.Durable = true;     // 메시지 persistent 설정 : 서버 disk에 저장
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while publishing single online order");
        //        return StatusCode(500, "Internal server error");
        //    }

        //    return Ok("Success to publish High priority Message");
        //}

        [HttpPost("apigatewaytest")]
        public async Task<IActionResult> TestAPIGateway([FromBody] string message)
        {
            // HttpClient를 using으로 관리하여 적절한 리소스 해제 보장
            using var httpClient = _httpClientFactory.CreateClient("TestAPIGateway");

            try
            {
                int randomNumber = new Random().Next(1, 1000) * 10; // 1부터 9999까지의 랜덤 숫자 생성

                string query = $@"
                            SELECT TOP({randomNumber}) *
                            FROM 온라인_주문마스터 WITH (NOLOCK)
                            WHERE 센터코드 = 'TLK09'
                            AND B코드 = 'HB'
                            AND 작업일자 BETWEEN '20250101' AND '20250131'";

                DataTable returnTable = await _dbManager.GetDataTableAsync("02", query, CommandType.Text, null);
                List<OnlineOrderMaster_Model> orderList = DataTableMapper.MapToList<OnlineOrderMaster_Model>(returnTable);

                //var dto = new DTO_APIGateway<List<Model_Online_Order_Master>>
                //{
                //    RequestId = Guid.NewGuid().ToString(),
                //    TimeAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //    _requestData = orderList
                //};

                string jsonData = JsonConvert.SerializeObject(orderList);
                var jsonContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // HttpClient 호출 시 예외 처리 추가
                //HttpResponseMessage response = await httpClient.PostAsync("/api/orchestration/test/onlineorderbulk", jsonContent);
                HttpResponseMessage response = await httpClient.PostAsync("/api/orchestration/onlineorder/onlineorderNew", jsonContent);      // Hangfire에서 호출하는 API

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"API Gateway returned error status: {response.StatusCode}");
                }

                string result = await response.Content.ReadAsStringAsync();
                return Ok(result);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request failed in TestAPIGateway");
                return StatusCode(502, "External API communication failed");
            }
            catch (TaskCanceledException timeoutEx)
            {
                _logger.LogError(timeoutEx, "Request timeout in TestAPIGateway");
                return StatusCode(408, "Request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in TestAPIGateway");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("apigatewayhighprioritytest")]
        public async Task<IActionResult> TestAPIGatewayHighPriority([FromBody] string message)
        {
            // HttpClient를 using으로 관리하여 적절한 리소스 해제 보장
            using var httpClient = _httpClientFactory.CreateClient("TestAPIGateway");

            try
            {
                string query = @"
                            SELECT TOP(1) *
                            FROM 온라인_주문마스터 WITH (NOLOCK)
                            WHERE 센터코드 = 'TLK09'
                            AND B코드 = 'HB'
                            AND 작업일자 BETWEEN '20250101' AND '20250131'";

                DataTable returnTable = await _dbManager.GetDataTableAsync("02", query, CommandType.Text, null);
                List<OnlineOrderMaster_Model> orderList = DataTableMapper.MapToList<OnlineOrderMaster_Model>(returnTable);

                string jsonData = JsonConvert.SerializeObject(orderList);
                var jsonContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // HttpClient 호출 시 예외 처리 추가
                HttpResponseMessage response = await httpClient.PostAsync("/api/orchestration/onlineorder/onlineorder", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"API Gateway returned error status: {response.StatusCode}");
                }

                string result = await response.Content.ReadAsStringAsync();
                return Ok(result);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request failed in TestAPIGatewayHighPriority");
                return StatusCode(502, "External API communication failed");
            }
            catch (TaskCanceledException timeoutEx)
            {
                _logger.LogError(timeoutEx, "Request timeout in TestAPIGatewayHighPriority");
                return StatusCode(408, "Request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in TestAPIGatewayHighPriority");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("receivingbulktest")]
        public async Task<IActionResult> TestReceivingBulk([FromBody] string message)
        {
            // HttpClient를 using으로 관리하여 적절한 리소스 해제 보장
            using var httpClient = _httpClientFactory.CreateClient("TestAPIGateway");

            int randomNumber = new Random().Next(1, 1000) * 10; // 1부터 9999까지의 랜덤 숫자 생성

            try
            {
                string query = $@"
                            SELECT TOP({randomNumber}) *
                            FROM 입고_메인 WITH (NOLOCK)
                            WHERE 센터코드 = 'TLK09'
                            AND B코드 = 'HB'
                            AND 수불일자 BETWEEN '20250101' AND '20250231'";

                DataTable returnTable = await _dbManager.GetDataTableAsync("02", query, CommandType.Text, null);
                List<ReceivingMaster_Model> orderList = DataTableMapper.MapToList<ReceivingMaster_Model>(returnTable);

                string jsonData = JsonConvert.SerializeObject(orderList);
                var jsonContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // HttpClient 호출 시 예외 처리 추가
                HttpResponseMessage response = await httpClient.PostAsync("/api/orchestration/receiving/receivingnew", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"API Gateway returned error status: {response.StatusCode}");
                }

                string result = await response.Content.ReadAsStringAsync();
                return Ok(result);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request failed in TestAPIGatewayHighPriority");
                return StatusCode(502, "External API communication failed");
            }
            catch (TaskCanceledException timeoutEx)
            {
                _logger.LogError(timeoutEx, "Request timeout in TestAPIGatewayHighPriority");
                return StatusCode(408, "Request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in TestAPIGatewayHighPriority");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("getjobidstatus")]
        public async Task<IActionResult> GetJobIdStatus([FromBody] string jobId)
        {
            try
            {
                var encodedJobId = Uri.EscapeDataString(jobId);
                var requestUri = $"/api/orchestration/retrievestatus/jobid/{encodedJobId}";

                using var httpClient = _httpClientFactory.CreateClient("TestAPIGateway");
                using var response = await httpClient.GetAsync(requestUri);

                var contentString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var jobModel = JsonConvert.DeserializeObject<Job_Model>(contentString);

                    if (jobModel == null)
                    {
                        _logger.LogWarning($"Job with ID {jobId} not found or deserialization failed.");
                        throw new Exception("Job not found or deserialization failed");
                    }

                    return Ok(jobModel);
                }
                else
                {
                    _logger.LogWarning($"Failed to retrieve job status for ID {jobId}. Status code: {response.StatusCode}");
                    throw new HttpRequestException($"Failed to retrieve job status. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving job status");
                throw new HttpRequestException("Failed to retrieve job status", ex);
            }
        }

        [HttpPost("getchunkidstatus")]
        public async Task<IActionResult> GetChunkIdStatus([FromBody] string chunkId)
        {
            try
            {
                var encodedChunkId = Uri.EscapeDataString(chunkId);
                var requestUri = $"/api/orchestration/retrievestatus/chunkid/{encodedChunkId}";
                using var httpClient = _httpClientFactory.CreateClient("TestAPIGateway");
                using var response = await httpClient.GetAsync(requestUri);
                var contentString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var jobChunkModel = JsonConvert.DeserializeObject<JobChunk_Model>(contentString);
                    if (jobChunkModel == null)
                    {
                        _logger.LogWarning($"Chunk with ID {chunkId} not found or deserialization failed.");
                        throw new Exception("Chunk not found or deserialization failed");
                    }
                    return Ok(jobChunkModel);
                }
                else
                {
                    _logger.LogWarning($"Failed to retrieve chunk status for ID {chunkId}. Status code: {response.StatusCode}");
                    throw new HttpRequestException($"Failed to retrieve chunk status. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving chunk status");
                throw new HttpRequestException("Failed to retrieve chunk status", ex);
            }
        }

        public async Task<IActionResult> B2cOutOrderBulk([FromBody] string message)
        {
            // HttpClient를 using으로 관리하여 적절한 리소스 해제 보장
            using var httpClient = _httpClientFactory.CreateClient("TestAPIGateway");

            try
            {
                // TODO: B2cOutOrder에 맞는 프로시저 호출
                string query = "";

                DataTable returnTable = await _dbManager.GetDataTableAsync("02", query, CommandType.Text, null);
                List<B2cOutOrderReqList_NT> orderList = DataTableMapper.MapToList<B2cOutOrderReqList_NT>(returnTable);

                //var dto = new DTO_APIGateway<List<Model_Online_Order_Master>>
                //{
                //    RequestId = Guid.NewGuid().ToString(),
                //    TimeAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //    _requestData = orderList
                //};

                string jsonData = JsonConvert.SerializeObject(orderList);
                var jsonContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // HttpClient 호출 시 예외 처리 추가
                //HttpResponseMessage response = await httpClient.PostAsync("/api/orchestration/test/onlineorderbulk", jsonContent);
                HttpResponseMessage response = await httpClient.PostAsync("/api/orchestration/b2coutorder/b2coutorderreqbulk", jsonContent);      // Hangfire에서 호출하는 API

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"API Gateway returned error status: {response.StatusCode}");
                }

                string result = await response.Content.ReadAsStringAsync();
                return Ok(result);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request failed in TestAPIGateway");
                return StatusCode(502, "External API communication failed");
            }
            catch (TaskCanceledException timeoutEx)
            {
                _logger.LogError(timeoutEx, "Request timeout in TestAPIGateway");
                return StatusCode(408, "Request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in TestAPIGateway");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    //    [HttpPost("outboundtests")]
    //    public async Task<IActionResult> OutBoundTests(string message)
    //    {
    //        string query = @"
    //                        SELECT TOP(10000) 
    //                        -- 온라인_주문마스터 NOT NULL 컬럼들
    //                        마.자체주문번호,
    //                        상.순번,
    //                        마.센터코드,
    //                        마.B코드,
    //                        마.주문구분,
    //                        마.합포장기준,
    //                        마.합포장구분,
    //                        마.작업일자,
    //                        마.작업차수,
    //                        마.주문접수일자,
    //                        마.주문접수차수,
    //                        마.주문등록일시,
    //                        마.주문자,
    //                        마.수령자,
    //                        마.수령자휴대폰,
    //                        마.수령자연락처,
    //                        마.우편번호,
    //                        마.주소1,
    //                        마.배송메시지,
    //                        마.몰명,
    //                        마.판매금액,
    //                        마.결제금액,
    //                        마.주문수량계,
    //                        마.배송료,
    //                        마.배송료구분,
    //                        마.몰그룹코드,
    //                        마.몰코드,
    //                        마.상태구분 AS 마스터상태구분,
    //                        마.부분배송여부,
    //                        마.부분출고여부,
    //                        마.사전결품대상,
    //                        마.등록자,
    
    //                        -- 온라인_주문상세 NOT NULL 컬럼들 (겹치는 컬럼 제외)
    //                        상.주문번호,
    //                        상.서브키,
    //                        상.몰상품코드,
    //                        상.상품명,
    //                        상.수량,
    //                        상.작업,
    //                        상.분류,
    //                        상.소비자단가,
    //                        상.판매단가,
    //                        상.공급단가,
    //                        상.상품코드,
    //                        상.운송장발행회수,
    //                        상.B코드 AS 상세B코드,
    //                        상.몰그룹코드 AS 상세몰그룹코드,
    //                        상.몰코드 AS 상세몰코드,
    //                        상.상태구분 AS 상세상태구분,
    //                        상.택배사전송,
    //                        상.출고확정,
    //                        상.반품구분

    //                    FROM 온라인_주문마스터 마 WITH (NOLOCK)
    //                    INNER JOIN 온라인_주문상세 상 WITH (NOLOCK)
    //                    ON 마.자체주문번호 = 상.자체주문번호       
    //                    WHERE 마.센터코드 = 'TLK09'
    //                    AND 마.B코드 = 'HB'
    //                    AND 마.작업일자 BETWEEN '20250101' AND '20250131'
    //                    ORDER BY 마.자체주문번호, 상.순번;";
    //    }
    //}
}