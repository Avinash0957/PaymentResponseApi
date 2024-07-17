using Newtonsoft.Json;

namespace PaymentResponseApi.models
{
    public class Receiver
    {
        public string member_id { get; set; }
        public string role { get; set; }
        public string email { get; set; }

        public string amount { get; set; }

        public string paid { get; set; }
    }
}
