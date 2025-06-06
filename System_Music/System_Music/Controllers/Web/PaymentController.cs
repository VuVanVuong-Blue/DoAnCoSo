using Microsoft.AspNetCore.Mvc;
using System_Music.Models;
using System_Music.Services.Implementations;
using System_Music.Services.Interfaces;
using System_Music.Models.ViewModels;
public class PaymentController : Controller
{
    private IMomoService _momoService;
    public PaymentController(IMomoService momoService)
    {
        _momoService = momoService;
    }

    [HttpPost]
    [Route("CreatePaymentUrl")]
    public async Task<IActionResult> CreatePaymentMomo(OrderInfoModel model)
    {
        var response = await _momoService.CreatePaymentMomo(model);
        return Redirect(response.PayUrl);
    }


    [HttpPost]
    public IActionResult ConfirmMockPayment(string planName, long amount)
    {
        TempData["PaymentResult"] = $"Thanh toán thành công cho gói {planName} với số tiền {amount} đ!";
        TempData["PlanName"] = planName;
        TempData["Amount"] = amount.ToString(); // Chuyển long thành string
        return RedirectToAction("PaymentResult");
    }

    [HttpGet]
    public IActionResult PaymentResult()
    {
        ViewBag.PaymentResult = TempData["PaymentResult"]?.ToString() ?? "Không có kết quả thanh toán.";
        ViewBag.PlanName = TempData["PlanName"]?.ToString() ?? "Không xác định";
        ViewBag.Amount = TempData["Amount"] != null ? long.Parse(TempData["Amount"].ToString()) : 0; // Chuyển string về long
        return View("~/Views/Premium/PaymentResult.cshtml");
    }


}