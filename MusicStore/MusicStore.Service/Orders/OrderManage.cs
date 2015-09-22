using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicStore.Domain.Utils;
using MusicStore.Domain.Repo;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Data;

namespace MusicStore.Service.Orders
{
    public class OrderManage
    {
        private OrderRepo repo;

        public OrderManage(IUnitOfWork uw)
        {
            this.repo = new OrderRepo(uw);
        }

        #region CURD 方法
        public IEnumerable<Order> Orders
        {
            get { return this.repo.GetAll(); }
        }

        public void SaveOrder(Order order)
        {
            this.repo.Create(order);
        }
        #endregion
    }
}
