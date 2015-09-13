using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicStore.Domain.Data;

namespace MusicStore.Service.Account
{

    public class AccountRepo : BaseRepository<Account>
    {
        public IQueryable<Account> Accounts
        {
            get { return }
        }
        public bool CreateAccount(Account account)
        {
            bool exist = false;

            Account acc = context.Accounts.FirstOrDefault(x => x.Username.Equals(account.Username));
            if (acc == null)
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
