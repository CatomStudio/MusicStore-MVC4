using MusicStore.Domain.Data;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Repo;
using MusicStore.Domain.Utils;
using MusicStore.Service.Products;
using MusicStore.Service.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MusicStore.WebUI.Infrastructure.Abstract;
using MusicStore.WebUI.Infrastructure.Concrete;

namespace MusicStore.WebUI.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        IAuthProvider authProvider;
        private ProductManage proRepo;
        private OrderManage ordRepo;

        public AdminController()
        {
            using (UnitOfWork)
            {
                this.proRepo = new ProductManage(UnitOfWork);
                this.ordRepo = new OrderManage(UnitOfWork);
            }
        }

        public ViewResult Index()
        {
            return View(proRepo.Products);
        }

        public ViewResult Edit(int productId)
        {
            Product product = proRepo.Products.FirstOrDefault(p => p.ProductId == productId);
            return View(product);
        }

        [HttpPost]
        public ActionResult Edit(Product product, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                if (image != null)
                {
                    product.ImageMimeType = image.ContentType;
                    product.ImageData = new byte[image.ContentLength];
                    image.InputStream.Read(product.ImageData, 0, image.ContentLength);
                }
                this.proRepo.SaveProduct(product);
                TempData["message"] = string.Format("{0} has been saved", product.Name);
                return RedirectToAction("Index");
            }
            else
            {
                // there is something wrong with the data values
                return View(product);
            }
        }

        public ViewResult Create()
        {
            return View("Edit", new Product());
        }

        [HttpPost]
        public ActionResult Delete(int productId)
        {
            Product deletedProduct = this.proRepo.DeleteProduct(productId);
            if (deletedProduct != null)
            {
                TempData["message"] = string.Format("{0} was deleted",
                deletedProduct.Name);
            }
            return RedirectToAction("Index");
        }

        public ActionResult History()
        {
            return View(this.ordRepo.Orders);
        }

        public ActionResult Logout()
        {
            authProvider.Logout();
            Session["User"] = null;
            return RedirectToAction("List", "Product");
        }
    }

}

