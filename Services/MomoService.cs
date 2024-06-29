using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using DemoMongoDB.Models.Momo;
using RestSharp;
using DemoMongoDB.Models;

namespace DemoMongoDB.Services;

public class MomoService : IMomoService
{
    private readonly IOptions<MomoOptionModel> _options;

    public MomoService(IOptions<MomoOptionModel> options)
    {
        _options = options;
    }

    public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(CheckoutViewModel model)
    {
        model.OrderId = DateTime.UtcNow.Ticks.ToString();
        model.OrderInfo = "Khách hàng: " + model.CustomerName + " Nội dung: " + model.OrderInfo;
        var rawData =
            $"partnerCode={_options.Value.PartnerCode}&accessKey={_options.Value.AccessKey}&requestId={model.OrderId}&amount={model.OrderTotal}&orderId={model.OrderId}&orderInfo={model.OrderInfo}&returnUrl={_options.Value.ReturnUrl}&notifyUrl={_options.Value.NotifyUrl}&extraData=customerEmail={model.CustomerEmail};customerName={model.CustomerName}";

        var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

        var client = new RestClient(_options.Value.MomoApiUrl);
        var request = new RestRequest() { Method = Method.Post };
        request.AddHeader("Content-Type", "application/json; charset=UTF-8");

        // Create an object representing the request data
        var requestData = new
        {
            accessKey = _options.Value.AccessKey,
            partnerCode = _options.Value.PartnerCode,
            requestType = _options.Value.RequestType,
            notifyUrl = _options.Value.NotifyUrl,
            returnUrl = _options.Value.ReturnUrl,
            orderId = model.OrderId,
            amount = model.OrderTotal.ToString(),
            orderInfo = model.OrderInfo,
            requestId = model.OrderId,
            extraData = $"customerEmail={model.CustomerEmail};customerName={model.CustomerName}",
            signature = signature
        };

        request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);

        var response = await client.ExecuteAsync(request);

        return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
    }

    public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
    {
        var amount = collection.First(s => s.Key == "amount").Value;
        var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
        var orderId = collection.First(s => s.Key == "orderId").Value;
        var extraData = collection.First(s => s.Key == "extraData").Value;

        // Tách chuỗi extraData để lấy giá trị customerEmail và customerName
        var extraDataDictionary = extraData.ToString()
            .Split(';')
            .Select(part => part.Split('='))
            .ToDictionary(split => split[0], split => split[1]);

        var customerEmail = extraDataDictionary.ContainsKey("customerEmail") ? extraDataDictionary["customerEmail"] : string.Empty;
        var customerName = extraDataDictionary.ContainsKey("customerName") ? extraDataDictionary["customerName"] : string.Empty;

        return new MomoExecuteResponseModel()
        {
            Amount = amount,
            OrderId = orderId,
            OrderInfo = orderInfo,
            CustomerEmail = customerEmail,
            CustomerName = customerName
        };
    }

    private string ComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        byte[] hashBytes;

        using (var hmac = new HMACSHA256(keyBytes))
        {
            hashBytes = hmac.ComputeHash(messageBytes);
        }

        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        return hashString;
    }
}