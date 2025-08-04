namespace LogiSimEduProject_BE_API.Controllers.Request
{
    public class PaymentWebhookData
    {
        public long orderCode { get; set; }
        public int transactionStatus { get; set; }
        public string description { get; set; }
    }
}
