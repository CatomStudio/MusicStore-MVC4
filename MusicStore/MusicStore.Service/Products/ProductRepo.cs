using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Data;
using MusicStore.Domain.Repo;
using MusicStore.Domain.Utils;

namespace MusicStore.Service.Products
{
    public class ProductRepo : BaseRepository<Product>
    {
        public ProductRepo(IUnitOfWork uw) : base(uw) { }
    }
}
