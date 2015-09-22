using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MusicStore.Domain.Data;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Repo;
using MusicStore.Service.Products;

namespace MusicStore.WebUI.Controllers
{
    public class NavController : BaseController
    {
        private ProductManage proRepo;

        public NavController()
        {
            using (UnitOfWork)
            {
                this.proRepo = new ProductManage(UnitOfWork);
            }
        }

        public PartialViewResult Menu(string category = null)
        {
            ViewBag.SelectedCategory = category;

            IEnumerable<string> categories = this.proRepo.Products.Select(x => x.Category).Distinct().OrderBy(x => x);
            return PartialView(categories);
        }

    }
}
