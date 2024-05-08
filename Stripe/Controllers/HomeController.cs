using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stripe.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Stripe.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController()
        {
            
        }

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        //public string Token_ID { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Charge (string stripeEmail , string stripeToken)
        {

            //Token_ID = stripeToken;
            var customers = new CustomerService();
            var customer = customers.Create(new CustomerCreateOptions { 
                Email = stripeEmail,
                Source = stripeToken
            });

            var charges = new ChargeService();


            var charge = charges.Create(new ChargeCreateOptions { 
                Amount = 500,
                Description = "Sucsess!!",
                Currency = "usd",
                Customer=customer.Id
            });

            if (charge.Status == "succeeded")
            {
                string BalanceTransactionId = charge.BalanceTransactionId;
                return RedirectToAction("Index");
            }
            else
                return BadRequest();

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
