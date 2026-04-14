namespace InvoiceAI.Models
{
    public class InvoiceDto
    {
        public string SupplierName { get; set; }
        public string InvoiceNumber { get; set; }
        public string Date { get; set; }
        public decimal TotalAmount { get; set; }
    }
    
    public class GeminiOptions
    {
        public string ApiKey { get; set; }
    }

}