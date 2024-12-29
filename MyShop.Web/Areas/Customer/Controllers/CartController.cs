using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.DataAccess.Implementation;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;
using MyShop.Utilities;
using Stripe.Checkout;
using System.Security.Claims;

namespace MyShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public decimal totalCarts { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                cartsList = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value, IncludeWord: "Product"),
                OrderHeader = new()
            };

            foreach(var item in ShoppingCartVM.cartsList)
            {
                ShoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartid)
        {
            var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c=>c.Id == cartid);
            _unitOfWork.ShoppingCart.IncreaseCount(shoppingCart, 1);
            _unitOfWork.Complete();

            return RedirectToAction("Index");
        }

        public IActionResult Minus(int cartid)
        {
            var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartid);
           
            if (shoppingCart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(shoppingCart);
                var count = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingCart.ApplicationUserId).Count() - 1;
                HttpContext.Session.SetInt32(SD.SessionKey, count);

            }
            else
            {
                _unitOfWork.ShoppingCart.DecreaseCount(shoppingCart, 1);
            }

            _unitOfWork.Complete();

            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartid)
        {
            var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartid);
            _unitOfWork.ShoppingCart.Remove(shoppingCart);
            _unitOfWork.Complete();

            var count = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingCart.ApplicationUserId).Count();
            HttpContext.Session.SetInt32(SD.SessionKey, count);

            return RedirectToAction("Index");
        }

        // order 
        [HttpGet]
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM
            {
                cartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, IncludeWord: "Product"),
                OrderHeader = new()
            };
            // bad design 
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;

            foreach (var item in ShoppingCartVM.cartsList)
            {
                ShoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult POSTSummary(ShoppingCartVM ShoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.cartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, IncludeWord: "Product");


            ShoppingCartVM.OrderHeader.OrderStatus = SD.Pending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.Pending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;


            foreach (var item in ShoppingCartVM.cartsList)
            {
                ShoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Complete();

            foreach (var item in ShoppingCartVM.cartsList)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = item.Product.Price,
                    Count = item.Count
                };

                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Complete();
            }

            var domain = "https://localhost:7224/"; // you need to change to deploy
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + $"Customer/Cart/Index",
            };

            foreach (var item in ShoppingCartVM.cartsList)
            {
                var sessionlineoption = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionlineoption);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            ShoppingCartVM.OrderHeader.SessionId = session.Id;

            _unitOfWork.Complete();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

            //_unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.CartsList);
            //         _unitOfWork.Complete();
            //         return RedirectToAction("Index","Home");

        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(id, SD.Approve, SD.Approve);
                orderHeader.PaymentIntentId = session.PaymentIntentId;
                _unitOfWork.Complete();
            }
            List<ShoppingCart> shoppingcarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            HttpContext.Session.Clear();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingcarts);
            _unitOfWork.Complete();
            return View(id);
        }

    }
}
