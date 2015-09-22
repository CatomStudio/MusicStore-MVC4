using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicStore.Domain.Data;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Repo;
using MusicStore.Domain.Utils;

namespace MusicStore.Service.Products
{
    public class ProductManage
    {
        private ProductRepo repo;

        public ProductManage(IUnitOfWork uw)
        {
            this.repo = new ProductRepo(uw);
        }

        #region CRUD 方法
        public IEnumerable<Product> Products
        {
            get { return this.repo.GetAll(); }
        }

        public void SaveProduct(Product product)
        {
            if (product.ProductId == 0)
            {
                this.repo.Create(product);
            }
            else
            {
                Product dbEntry = this.repo.Get(product.ProductId);
                if (dbEntry != null)
                {
                    dbEntry.Name = product.Name;
                    dbEntry.Description = product.Description;
                    dbEntry.Price = product.Price;
                    dbEntry.Category = product.Category;
                    if (product.ImageData != null)
                    {
                        dbEntry.ImageData = product.ImageData;
                        dbEntry.ImageMimeType = product.ImageMimeType;
                    }
                }
                this.repo.Update(dbEntry);
            }
        }

        public Product DeleteProduct(int productId)
        {
            Product dbEntry = this.repo.Get(productId);
            if (dbEntry != null)
            {
                this.repo.Delete(dbEntry);
            }
            return dbEntry;
        }
        #endregion
    }
}
