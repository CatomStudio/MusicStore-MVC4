using MusicStore.Domain.Data;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Repo;
using MusicStore.Service.Products;
using MusicStore.Service.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MusicStore.WebUI.Models;

namespace MusicStore.WebUI.Controllers
{

    public class CartController : BaseController
    {
        private ProductManage proRepo;
        private OrderManage ordRepo;

        public CartController()
        {
            using (UnitOfWork)
            {
                this.proRepo = new ProductManage(UnitOfWork);
                this.ordRepo = new OrderManage(UnitOfWork);
            }
        }

        //Add new method for summary
        public PartialViewResult Summary(Cart cart)
        {
            return PartialView(cart);
        }

        [HttpPost]
        public ViewResult Checkout(Cart cart, Order order)
        {
            if (cart.Lines.Count() == 0)
            {
                ModelState.AddModelError("", "Sorry, your cart is empty!");
            }
            order.Datetime = DateTime.Now;
            if (ModelState.IsValid)
            {
                this.ordRepo.SaveOrder(order);
                cart.Clear();
                return View("Completed");
            }
            else
            {
                return View(new OrderViewModel
                {
                    Cart = cart,
                    Order = new Order()
                });
            }
        }

        //Add check out
        public ViewResult Checkout(Cart cart)
        {
            return View(new OrderViewModel
            {
                Cart = cart,
                Order = new Order()
            });
        }



        public ActionResult Index(Cart cart, string returnUrl)
        {
            if (Session["User"] != null)
            {
                return View(new CartIndexViewModel
                {
                    Cart = cart,
                    ReturnUrl = returnUrl
                });
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }


        public RedirectToRouteResult AddToCart(Cart cart, int productId, string returnUrl)
        {
            if (Session["User"] != null)
            {
                Product product = this.proRepo.Products.FirstOrDefault(p => p.ProductId == productId);
                if (product != null)
                {
                    cart.AddItem(product, 1);
                }
                return RedirectToAction("Index", new { returnUrl });
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public RedirectToRouteResult RemoveFromCart(Cart cart, int productId, string returnUrl)
        {
            Product product = this.proRepo.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                cart.RemoveLine(product);
            }
            return RedirectToAction("Index", new { returnUrl });
        }

        private Cart GetCart()
        {
            Cart cart = (Cart)Session["Cart"];
            if (cart == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }

    }
}