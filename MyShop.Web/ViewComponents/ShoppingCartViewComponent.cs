﻿using Microsoft.AspNetCore.Mvc;
using MyShop.Entities.Repositories;
using MyShop.Utilities;
using System.Security.Claims;

namespace MyShop.Web.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitofwork;
        public ShoppingCartViewComponent(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(SD.SessionKey) != null)
                {
                    return View(HttpContext.Session.GetInt32(SD.SessionKey));
                }
                else
                {
                    HttpContext.Session.SetInt32(SD.SessionKey, _unitofwork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value).Count());
                    return View(HttpContext.Session.GetInt32(SD.SessionKey));
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}