using System;
using System.Web.Mvc;
using System.Web.Routing;
using Ninject;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Abstract;
using System.Collections.Generic;
using System.Linq;
using Moq;
using MusicStore.Domain.Concrete;
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
        protected override IController GetControllerInstance(RequestContext
        requestContext, Type controllerType)
        {
            return controllerType == null
            ? null
            : (IController)ninjectKernel.Get(controllerType);
        }
        private void AddBindings()
        {
            ninjectKernel.Bind<IProductRepository>().To<EFProductRepository>();
            ninjectKernel.Bind<IAccountRepository>().To<EFAccountRepository>();
            ninjectKernel.Bind<IOrderRepository>().To<EFOrderRepository>();
            ninjectKernel.Bind<IAuthProvider>().To<FormsAuthProvider>();
        }
    }
}