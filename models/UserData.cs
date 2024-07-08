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
    }
}
