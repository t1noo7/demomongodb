namespace DemoMongoDB.Models;

public class PaymentResponseModel
{
    public string OrderDescription { get; set; }
    public string TransactionId { get; set; }
    public string OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string TotalAmount { get; set; }
    public string PaymentMethod { get; set; }
    public string PaymentId { get; set; }
    public bool Success { get; set; }
    public string Token { get; set; }
    public string VnPayResponseCode { get; set; }
}