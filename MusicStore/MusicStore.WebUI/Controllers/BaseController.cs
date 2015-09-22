using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using MusicStore.Domain.Data;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Repo;
using MusicStore.Domain.Utils;
using MusicStore.Service.Accounts;
using MusicStore.Service.Orders;
using MusicStore.Service.Products;

namespace MusicStore.WebUI.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
        {

        }

        [Dependency("UnitOfWork")]
        public IUnitOfWork UnitOfWork { set; get; }

        [Dependency("ReadUnitOfWork")]
        public IUnitOfWork ReadUnitOfWork { set; get; }

        [Dependency("WriteUnitOfWork")]
        public IUnitOfWork WriteUnitOfWork { set; get; }


    }
}
