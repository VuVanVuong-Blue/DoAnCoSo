// File: System_Music.Services.Interfaces/IMomoService.cs
using System_Music.Models;

using Microsoft.AspNetCore.Http;
using System_Music.Services.Implementations;
using System_Music.Models.ViewModels;

namespace System_Music.Services.Interfaces
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model);
        Task CreatePaymentMomo();
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}