﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Data;
using MusicStore.Domain.Repo;
using MusicStore.Service.Accounts;
using MusicStore.Service.Orders;
using MusicStore.WebUI.Infrastructure.Abstract;
using MusicStore.WebUI.Models;

namespace MusicStore.WebUI.Controllers
{
    public class AccountController : BaseController
    {
        IAuthProvider authProvider;
        private AccountManage accRepo;
        private OrderManage ordRepo; 

        public AccountController()
        {
            using (UnitOfWork)
            {
                this.accRepo = new AccountManage(UnitOfWork);
                this.ordRepo = new OrderManage(UnitOfWork);
            }
        }

        public ViewResult Login()
        {
            return View();
        }

        public ActionResult Logout()
        {
            authProvider.Logout();
            Session["User"] = null;
            return RedirectToAction("List","Product");
        }
        
        public PartialViewResult User()
        {
            var user = Session["User"];
            if (user != null)
            {
                Account acc = this.accRepo.Accounts.FirstOrDefault(x => x.Username == user);
                if (acc!=null)
                {
                    return PartialView(acc);
                }
            }
            return PartialView(new Account());
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (authProvider.Authenticate(model.UserName, model.Password))
                {
                    Session["User"] = null;
                    return Redirect(returnUrl ?? Url.Action("Index", "Admin"));
                }
                else
                {
                    Account acc = this.accRepo.Accounts.FirstOrDefault(x => x.Username == model.UserName && x.Password == model.Password);
                    if (acc!=null)
                    {
                        authProvider.Logout();
                        Session["User"] = model.UserName;
                        return RedirectToAction("List", "Product");
                    }else{
                        Session["User"] = null;
                        ModelState.AddModelError("", "Incorrect username or password");
                        return View();
                    }
                }
            }
            else
            {
                return View();
            }
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Account account,HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                if (account.Username.ToLower() != "admin")
                {
                    CheckUploadImage(account, image);
                    bool succeed = this.accRepo.CreateAccount(account);
                    if (succeed)
                    {
                        TempData["message"] = string.Format("{0} has been saved", account.Username);
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        ModelState.AddModelError("", "The username already exists!");
                        return View();
                    }
                }else{
                    ModelState.AddModelError("", "The username already exists!");
                    return View();
                }
            }
            else
            {
                return View();
            }
        }

        public ActionResult Profile()
        {
            var user = Session["User"];
            Account account = this.accRepo.Accounts.FirstOrDefault(x => x.Username == user);
            return View(account);
        }
        [HttpPost]
        public ActionResult Update(Account account,HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                CheckUploadImage(account, image);
                this.accRepo.SaveAccount(account);
                TempData["message"] = string.Format("{0} has been updated", account.Username);
                return RedirectToAction("List", "Product");
            }
            else
            {
                return View("Profile",account);
            }
        }

        public ActionResult History()
        {
            var user = Session["User"];
            IEnumerable<Order> entry = this.ordRepo.Orders.Where(x => x.Username == user);
            return View(entry);
        }

        private void CheckUploadImage(Account account,HttpPostedFileBase image)
        {
            if (image != null)
            {
                account.ImageMimeType = image.ContentType;
                account.ImageData = new byte[image.ContentLength];
                image.InputStream.Read(account.ImageData, 0, image.ContentLength);
            }
        }
        public FileContentResult GetImage(int accountId)
        {
            Account acc = this.accRepo.Accounts.FirstOrDefault(p => p.AccountId == accountId);
            if (acc != null)
            {
                return File(acc.ImageData, acc.ImageMimeType);
            }
            else
            {
                return null;
            }
        }
    }
}