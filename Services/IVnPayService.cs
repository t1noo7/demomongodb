using DemoMongoDB.Models;

namespace DemoMongoDB.Services;
public interface IVnPayService
{
    string CreatePaymentUrl(CheckoutViewModel model, HttpContext context);
    PaymentResponseModel PaymentExecute(IQueryCollection collections);
}