using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicStore.Domain.Data;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Repo;
using MusicStore.Domain.Utils;

namespace MusicStore.Service.Orders
{
    public class OrderRepo : BaseRepository<Order>
    {
        public OrderRepo(IUnitOfWork uw) : base(uw) { }

    }
}
