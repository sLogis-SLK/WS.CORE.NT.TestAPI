using System;
using System.Text.Json;

namespace SLK.NT.Common.Model
{
    public static class ProductsJsonConverter
    {
        /// <summary>
        /// 지정한 경로의 JSON 파일을 읽고, ProductsReqList_NT 리스트로 변환
        /// </summary>
        /// <param name="filePath">JSON 파일 경로</param>
        /// <returns>ProductsReqList_NT 리스트</returns>
        public static List<ProductsReqList_NT> LoadAndConvert(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("JSON 파일을 찾을 수 없습니다.", filePath);

            string json = File.ReadAllText(filePath);

            // JSON -> ProductsReq
            var productsReq = JsonSerializer.Deserialize<ProductsReq>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (productsReq?.data == null)
                return new List<ProductsReqList_NT>();

            var random = new Random();
            var docNo = string.Concat(Enumerable.Range(0, 20).Select(_ => random.Next(0, 10)));

            // ProductsReqList -> ProductsReqList_NT 변환
            var result = new List<ProductsReqList_NT>();
            foreach (var item in productsReq.data)
            {
                result.Add(new ProductsReqList_NT
                {
                    docNo = docNo,
                    bCode = productsReq.bCode,
                    totalCount = productsReq.totalCount,
                    sendCount = productsReq.sendCount,

                    serialNo = item.serialNo,
                    productCode = item.productCode,
                    productNumber = item.productNumber,
                    style = item.style,
                    colorOptionCode = item.colorOptionCode,
                    colorOptionName = item.colorOptionName,
                    sizeOptionCode = item.sizeOptionCode,
                    sizeOptionName = item.sizeOptionName,
                    barCode = item.barCode,
                    barCode1 = item.barCode1,
                    barCode2 = item.barCode2,
                    barCode3 = item.barCode3,
                    productName = item.productName,
                    extProductName = item.extProductName,
                    brand = item.brand,
                    saleUnitPrice = item.saleUnitPrice,
                    COGM = item.COGM,
                    COGS = item.COGS,
                    productionYear = item.productionYear,
                    gender = item.gender,
                    season = item.season,
                    itemGroup = item.itemGroup,
                    setDiv = item.setDiv,
                    distributorCode = item.distributorCode,
                    endYN = item.endYN,
                    extColumn1 = item.extColumn1,
                    extColumn2 = item.extColumn2,
                    extColumn3 = item.extColumn3
                });
            }

            return result;
        }
    }

    public class ProductsReq
    {
        public string docNo { get; set; }
        public string bCode { get; set; }
        public int totalCount { get; set; }
        public int sendCount { get; set; }
        public List<ProductsReqList>? data { get; set; }
    }

    public class ProductsReqList
    {
        public int serialNo { get; set; }
        public string productCode { get; set; }
        public string productNumber { get; set; }
        public string? style { get; set; }
        public string? colorOptionCode { get; set; }
        public string? colorOptionName { get; set; }
        public string? sizeOptionCode { get; set; }
        public string? sizeOptionName { get; set; }
        public string barCode { get; set; }
        public string? barCode1 { get; set; }
        public string? barCode2 { get; set; }
        public string? barCode3 { get; set; }
        public string productName { get; set; }
        public string? extProductName { get; set; }
        public string brand { get; set; }
        public int? saleUnitPrice { get; set; }
        public int? COGM { get; set; }
        public int? COGS { get; set; }
        public string? productionYear { get; set; }
        public string? gender { get; set; }
        public string? season { get; set; }
        public string? itemGroup { get; set; }
        public string setDiv { get; set; }
        public string? distributorCode { get; set; }
        public string endYN { get; set; }
        public string? extColumn1 { get; set; }
        public string? extColumn2 { get; set; }
        public int? extColumn3 { get; set; }
    }

    public class ProductsReqList_NT
    {
        public string docNo { get; set; }
        public string bCode { get; set; }
        public int totalCount { get; set; }
        public int sendCount { get; set; }

        public int serialNo { get; set; }
        public string productCode { get; set; }
        public string productNumber { get; set; }
        public string? style { get; set; }
        public string? colorOptionCode { get; set; }
        public string? colorOptionName { get; set; }
        public string? sizeOptionCode { get; set; }
        public string? sizeOptionName { get; set; }
        public string barCode { get; set; }
        public string? barCode1 { get; set; }
        public string? barCode2 { get; set; }
        public string? barCode3 { get; set; }
        public string productName { get; set; }
        public string? extProductName { get; set; }
        public string brand { get; set; }
        public int? saleUnitPrice { get; set; }
        public int? COGM { get; set; }
        public int? COGS { get; set; }
        public string? productionYear { get; set; }
        public string? gender { get; set; }
        public string? season { get; set; }
        public string? itemGroup { get; set; }
        public string setDiv { get; set; }
        public string? distributorCode { get; set; }
        public string endYN { get; set; }
        public string? extColumn1 { get; set; }
        public string? extColumn2 { get; set; }
        public int? extColumn3 { get; set; }
    }
}
