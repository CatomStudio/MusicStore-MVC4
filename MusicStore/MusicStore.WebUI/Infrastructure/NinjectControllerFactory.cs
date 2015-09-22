using System;
using System.Web.Mvc;
using System.Web.Routing;
using Ninject;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Data;
using MusicStore.Domain.Repo;
using MusicStore.Service.Accounts;
using MusicStore.Service.Orders;
using MusicStore.Service.Products;
using System.Collections.Generic;
using System.Linq;
using Moq;
using System.Configuration;
using MusicStore.WebUI.Infrastructure.Abstract;
using MusicStore.WebUI.Infrastructure.Concrete;

namespace MusicStore.WebUI.Infrastructure
{
    public class NinjectControllerFactory : DefaultControllerFactory
    {
        private IKernel ninjectKernel;
        public NinjectControllerFactory()
        {
            ninjectKernel = new StandardKernel();
            AddBindings();
        }

        //protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        //{
        //    return controllerType == null ? null : (IController)ninjectKernel.Get(controllerType);
        //}
        
        private void AddBindings()
        {
            ninjectKernel.Bind<AccountManage>();
            ninjectKernel.Bind<ProductManage>();
            ninjectKernel.Bind<OrderManage>();
            //ninjectKernel.Bind<IAuthProvider>().To<FormsAuthProvider>();
        }

    }
}