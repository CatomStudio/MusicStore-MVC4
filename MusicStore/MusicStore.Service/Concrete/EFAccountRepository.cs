using System;
using MusicStore.Domain.Repo;
using MusicStore.Domain.Data;
using MusicStore.Domain.Entities;
using System.Linq;

namespace MusicStore.Domain.Concrete
{
    public class EFAccountRepository : BaseRepository<Account>
    {
        public IQueryable<Account> Accounts
        {
            get { return }
        }
        public bool CreateAccount(Account account)
        {
            bool exist = false;
            
            Account acc = context.Accounts.FirstOrDefault(x => x.Username.Equals(account.Username));
            if (acc==null)
            {
                exist = true;
                context.Accounts.Add(account);
            }
            context.SaveChanges();
            return exist;
        }
        public void SaveAccount(Account account)
        {
            Account dbEntry = context.Accounts.Find(account.AccountId);
            if (dbEntry != null)
            {
                dbEntry.Name = account.Name;
                dbEntry.Address = account.Address;
                if (account.ImageData != null)
                {
                    dbEntry.ImageData = account.ImageData;
                    dbEntry.ImageMimeType = account.ImageMimeType;
                }
            }
            context.SaveChanges();
        }
    }
}
