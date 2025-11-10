namespace SLK.NT.Common.Model
{
    public class B2cOutOrder
    {
        //public class B2cOutOrderReq
        //{
        //    public string docNo { get; set; }
        //    public string bCode { get; set; }
        //    public string onePieceYN { get; set; }
        //    public string commandDate { get; set; }
        //    public string orderGrpType { get; set; }
        //    public int totalCount { get; set; }
        //    public int sendCount { get; set; }
        //    public List<B2cOutOrderReqList>? data { get; set; }
        //}

        
    }

    public class B2cOutOrderReqList_NT
    {
        public string docNo { get; set; }
        public string bCode { get; set; }
        public string onePieceYN { get; set; }
        public string commandDate { get; set; }
        public string orderGrpType { get; set; }
        public int totalCount { get; set; }
        public int sendCount { get; set; }
        public string commandNo { get; set; }
        public string orderType { get; set; }
        public string orderDateTime { get; set; }
        public string? orderName { get; set; }
        public string? orderCellNo { get; set; }
        public string? orderTelNo { get; set; }
        public string receiverName { get; set; }
        public string receiverCellNo { get; set; }
        public string? receiverTelNo { get; set; }
        public string zipNo { get; set; }
        public string addr1 { get; set; }
        public string? addr2 { get; set; }
        public string? orderMessage { get; set; }
        public string? deliveryMessage { get; set; }
        public string? giveaways { get; set; }
        public string? mallCode { get; set; }
        public string mallName { get; set; }
        public string warehouseCode { get; set; }
        public string shipFromCode { get; set; }
        public string? courierCode { get; set; }
        public string? courierFeeDiv { get; set; }
        public int courierFee { get; set; }
        public string? waybillNo { get; set; }
        public string? seqNo { get; set; }
        public string? remark { get; set; }
        public int? saleAmount { get; set; }
        public int? payAmount { get; set; }
        public int outPriority { get; set; }
        public string? hostKey1 { get; set; }
        public string? hostKey2 { get; set; }
        public string? hostKey3 { get; set; }
        public string? extColumn1 { get; set; }
        public string? extColumn2 { get; set; }
        public int? extColumn3 { get; set; }
        public List<B2cOutOrderReqPiList>? productInfo { get; set; }
    }

    public class B2cOutOrderReqPiList
    {
        public string commandNo { get; set; }
        public int serialNo { get; set; }
        public string commandSerialNo { get; set; }
        public string orderType { get; set; }
        public string productCode { get; set; }
        public string mallProductName { get; set; }
        public int orderQty { get; set; }
        public int? salePrice { get; set; }
        public int? payPrice { get; set; }
        public string setDiv { get; set; }
        public string orderNo { get; set; }
        public string? orderNo1 { get; set; }
        public string? orderNo2 { get; set; }
        public string? orderNo3 { get; set; }
        public string? hostKey4 { get; set; }
        public string? hostKey5 { get; set; }
        public string? hostKey6 { get; set; }
        public string? extColumn4 { get; set; }
        public string? extColumn5 { get; set; }
        public int? extColumn6 { get; set; }
    }
}
