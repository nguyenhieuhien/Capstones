namespace LogiSimEduProject_BE_API.Controllers.Request
{
    public class CallbackData
    {
        public long orderCode { get; set; }
        public int status { get; set; } // "PAID" hoặc "CANCELLED"
    }
}
