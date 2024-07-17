namespace PaymentResponseApi.models
{
    public class UserData
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public char TestMode { get; set; }
        public string SaleId { get; set; }
        public string PaymentGateway { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentTime { get; set; }
        public string PayerEmail { get; set; }
        public string PayerFirstName { get; set; }
        public string PayerLastName { get; set; }

        public string PayerAddressStreet { get; set; }
        public string PayerAddressCity { get; set; }
        public string PayerAddressState { get; set; }
        public string PayerAddressZip { get; set; }

        public string PayerAddressCountry { get; set; }

        public string payer_phone { get; set; }

        public string Receiver { get; set; }
        public string Role { get; set; }
        public string MemberId { get; set; }
        public string Paid { get; set; }
        public string TransactionId { get; set; }
        public string BrokerId { get; set; }

        public string amount { get; set; }

        public string coupon_code { get; set; } 

        public List<Receiver> Receivers { get; set; }
        public List<Receiver> Receiverss { get; set; } = new List<Receiver>();

    }
}
