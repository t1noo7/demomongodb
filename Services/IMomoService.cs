using DemoMongoDB.Models;
using DemoMongoDB.Models.Momo;

namespace DemoMongoDB.Services;

public interface IMomoService
{
    Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(CheckoutViewModel model);
    MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
}