using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using StripeAPITest.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

[Route("api/payment")]
[ApiController]

public class PaymentController : ControllerBase
{
    [HttpPost("Charge")]
    public ActionResult Charge(string stripeEmail)
    {
        try
        {
            var CustomerService = new CustomerService();
            var customer = CustomerService.Create(new CustomerCreateOptions
            {
                Name = "Ahmed",
                Email = stripeEmail,
                Source = "tok_visa"
            });

            var ChargeService = new ChargeService();
            var charge = ChargeService.Create(new ChargeCreateOptions
            {
                Amount = 150000,
                Description = "Sucsess!!",
                Currency = "usd",
                Customer = customer.Id,
            });
            return Ok(charge);
        }
        catch (StripeException e)
        {
            return BadRequest(e.StripeError.Message);
        }

    }



    [HttpPost("PaymentIntent")]
    public ActionResult PaymentIntent(string stripeEmail, string name)
    {
        try
        {
            var CustomerService = new CustomerService();
            var customer = CustomerService.Create(new CustomerCreateOptions
            {
                Name = name,
                Email = stripeEmail,
                Source = "tok_visa"
            });

            var options = new PaymentIntentCreateOptions
            {
                
                Amount = 2000,
                Currency = "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Customer = customer.Id
            };
            var service = new PaymentIntentService();
            PaymentIntent payment = service.Create(options);

            Console.WriteLine("No error.");
            return Ok(payment);
        }
        catch (StripeException e)
        {
            switch (e.StripeError.Type)
            {
                case "card_error":
                    Console.WriteLine($"A payment error occurred: {e.StripeError.Message}");
                    break;
                case "invalid_request_error":
                    Console.WriteLine($"An invalid request occurred : {e.StripeError.Message}");
                    break;
                default:
                    Console.WriteLine("Another problem occurred, maybe unrelated to Stripe.");
                    break;
            }
            return BadRequest(e.StripeError.Type);
        }
    }

    [HttpPost("ConfirmPayment")]
    public ActionResult ConfirmPayment(string payment_ID)
    {
        try
        {
            var options = new PaymentIntentConfirmOptions
            {
                PaymentMethod = "pm_card_visa",
                ReturnUrl = "https://www.example.com",
            };
            var service = new PaymentIntentService();
            var confirmed = service.Confirm(payment_ID, options);
            return Ok(confirmed);
        }
        catch (StripeException e)
        {
            return BadRequest(e.StripeError.Message);
        }
    }

    [HttpPost("Customer")]
    public ActionResult CustomerCreation(string stripeEmail, string name)
    {
        try
        {
            var CustomerService = new CustomerService();
            Customer customer = CustomerService.Create(new CustomerCreateOptions
            {
                Name = name,
                Email = stripeEmail,
                Source = "tok_visa",
                
            });

            //CardService cardService = new CardService();
            //var card = cardService.Create(new Stripe.Issuing.CardCreateOptions
            //{
            //    Cardholder="
            //});

            //var options = new Stripe.Issuing.CardCreateOptions
            //{
            //    Cardholder = "ich_1MsKAB2eZvKYlo2C3eZ2BdvK",
            //    Currency = "usd",
            //    Type = "virtual",
            //};

            return Ok(customer);
        }
        catch (StripeException e)
        {
            return BadRequest(e.StripeError.Message);
        }
    }

    
    [HttpPost("SessionCreation")]
    public ActionResult SessionCreation(string stripeEmail, string name)
    {

        var CustomerService = new CustomerService();
        var customer = CustomerService.Create(new CustomerCreateOptions
        {
            Name = name,
            Email = stripeEmail,
            Source = "tok_visa"
        });

        var invoiceService = new InvoiceService();
        Invoice invoice = invoiceService.Create(new InvoiceCreateOptions
        {
            Customer=customer.Id,
            Currency="usd",
            
        });

        try
        {

            var options = new SessionCreateOptions
            {
                Customer=customer.Id,
                SuccessUrl = "https://example.com/success",
                LineItems = new List<SessionLineItemOptions>
                {
                new Stripe.Checkout.SessionLineItemOptions
                {
                    Price = "price_1PDQoEHnYYD6YDOYiKnesBPn",
                    Quantity = 1,
                },
                },
                Mode = "subscription",
            };

            var service = new SessionService();



            Session session = service.Create(options);
            return Ok(session);
        }
        catch (StripeException e)
        {
            return BadRequest(e.StripeError.Message);
        }
    }

    
    [HttpPost("Customer Portal")]
    public ActionResult Customer_Portal(string session_id,string customer_id)
    {
        //"cs_test_a1Rw7b9sd4OTJ58Ya2jenLUuQKWJiru3FsKb5941vhHkoyXDiRqxT2nfz3"
        try
        {
            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customer_id,
                //ReturnUrl = "",
            };
            var service = new Stripe.BillingPortal.SessionService();
            var session =  service.Create(options);

            return Ok(session.Url);
        }
        catch (StripeException e)
        {
            return BadRequest(e.StripeError.Message);
        }
    }

    //https://localhost:44326/api/payment/WebHook

    [HttpPost("WebHook")]
    public async Task<ActionResult> WebHookAsync()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        const string endpointSecret = "whsec_6d92f9a6c26c8ab6905c9747a4d2484e3ee8df762e3381c1c5ff1aaba66d5c2c";
        try
        {
            var stripeEvent = EventUtility.ParseEvent(json);
            var signatureHeader = Request.Headers["Stripe-Signature"];

            stripeEvent = EventUtility.ConstructEvent(
                json,
                signatureHeader, 
                endpointSecret);

            if (stripeEvent.Type == Events.CustomerSubscriptionCreated)
            {
                //var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                //Console.WriteLine("A successful payment for {0} was made.", paymentIntent.Amount);
                var subscription = stripeEvent.Data.Object as Subscription;
                addSubscription(subscription);
                
            }
            else if (stripeEvent.Type == Events.CustomerSubscriptionUpdated)
            {
                //var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
                var session = stripeEvent.Data.Object as Subscription;
                updateSubscription(session);
            }
            else
            {
                Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
            }
            return Ok();
        }
        catch (StripeException e)
        {
            Console.WriteLine("Error: {0}", e.Message);
            return BadRequest();
        }
        catch (Exception e)
        {
            return StatusCode(500);
        }
    }

    private void addSubscription(Subscription subscription)
    {
        Console.WriteLine($"Added Subscription {subscription}");
    }
    private void updateSubscription(Subscription subscription)
    {
        Console.WriteLine($"updated Subscription {subscription}");
    }




}



