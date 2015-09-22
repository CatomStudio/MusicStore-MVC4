using System.Collections.Specialized;
using System.Configuration;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Mvc;
using System.Data;
using MusicStore.Domain.Data;

namespace MusicStore.WebUI.App_Start
{
    public sealed class Bootstrapper
    {
        #region 载体类，单例模式
        private static Bootstrapper strapper = null;

        public static Bootstrapper Instance
        {
            get
            {
                if (strapper == null)
                {
                    strapper = new Bootstrapper();
                }
                return strapper;
            }
        }
        #endregion

        #region IoC 容器实例对象
        private IUnityContainer container = null;

        // IoC 容器实例
        public IUnityContainer UnityContainer
        {
            get { return this.container; }
        }
        #endregion

        #region 创建容器，初始注入各种类
        public void Initialise()
        {
            BuildUnityContainer();
            // 设置容器
            DependencyResolver.SetResolver(new UnityDependencyResolver(this.container));
        }

        private void BuildUnityContainer()
        {
            /*
             * 
             * Unity默认情况下会自动帮我们维护好这些对象的生命周
             * 
             * TransientLifetimeManager，瞬态生命周期，默认情况下，在使用RegisterType进行对象关系注册时如果没有指定生命周期管理器则默认使用这个生命周期管理器，这个生命周期管理器就如同其名字一样，当使用这种管理器的时候，每次通过Resolve或ResolveAll调用对象的时候都会重新创建一个新的对象。
             * 
             * ContainerControlledLifetimeManager，容器控制生命周期管理，这个生命周期管理器是RegisterInstance默认使用的生命周期管理器，也就是单件实例，UnityContainer会维护一个对象实例的强引用，每次调用的时候都会返回同一对象
             * 
            */

            container = new UnityContainer();

            #region 缓存、session等的注入
            //namevaluecollection nvc = configurationmanager.appsettings;
            //string serverlist = nvc["serverlist"];
            //string cachedarea = nvc["cachedarea"];
            //string[] serverip = serverlist.split(',');
            //imysession mysession = new mysession(serverip, int.parse(nvc["sessionexpirehours"]), nvc["sessioncookiedomain"], nvc["sessionarea"]);
            //container.registerinstance(typeof(imysession), mysession);
            //icache mycache = new memcache(serverip, cachedarea);
            //icache mycache = new ocscache();
            //container.registerinstance(typeof(icache), mycache);
            #endregion

            #region DAL 映射注入

            string Conn = ConfigurationManager.ConnectionStrings["SkyConn"].ConnectionString;
            string ReadConn = ConfigurationManager.ConnectionStrings["SkyReadConn"].ConnectionString;
            string WriteConn = ConfigurationManager.ConnectionStrings["SkyWriteConn"].ConnectionString;
            container.RegisterType(typeof(IUnitOfWork), typeof(UnitOfWork), "UnitOfWork", new InjectionConstructor(Conn));
            container.RegisterType(typeof(IUnitOfWork), typeof(UnitOfWork), "ReadUnitOfWork", new InjectionConstructor(ReadConn));
            container.RegisterType(typeof(IUnitOfWork), typeof(UnitOfWork), "WriteUnitOfWork", new InjectionConstructor(WriteConn));
            #endregion

            #region 接口映射注入

            #endregion


        }

        #endregion
    }
}

