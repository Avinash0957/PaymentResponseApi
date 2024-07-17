using Newtonsoft.Json;
using System.Diagnostics.Metrics;

namespace PaymentResponseApi.models
{

    //public class WrapperClass
    //{
    //    public NewClass User_data { get; set; }
    //}
    public class NewClass
    {
        public string product_id { get; set; }
        public string product_name { get; set; }
        public string test_mode { get; set; }
        public string sale_id { get; set; }
        public string payment_gateway { get; set; }
        public string payment_amount { get; set; }
        public DateTime payment_time { get; set; }
        public string coupon { get; set; }
        public string payer_email { get; set; }
        public string payer_first_name { get; set; }
        public string payer_last_name { get; set; }
        public string payer_address_street { get; set; }
        public string payer_address_city { get; set; }
        public string payer_address_state { get; set; }
        public string payer_address_zip { get; set; }
        public string payer_address_country { get; set; }
        public string payer_phone { get; set; }
        public List<Receiver> receivers { get; set; }
    }
}
