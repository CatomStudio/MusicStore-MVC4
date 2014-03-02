using MusicStore.Domain.Abstract;
using MusicStore.Domain.Entities;
using System.Linq;

namespace MusicStore.Domain.Concrete
{
    public class EFOrderRepository : IOrderRepository
    {
        private EFDbContext context = new EFDbContext();
        public IQueryable<Order> Orders
        {
            get { return context.Orders; }
        }
        public void SaveOrder(Order order)
        {
            context.Orders.Add(order);
            context.SaveChanges();
        }
    }
}
