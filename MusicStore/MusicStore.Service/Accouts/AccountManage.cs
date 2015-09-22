using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicStore.Domain.Data;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Repo;
using MusicStore.Domain.Utils;

namespace MusicStore.Service.Accounts
{
    public class AccountManage
    {
        private AccountRepo repo;

        public AccountManage(IUnitOfWork uw)
        {
            this.repo = new AccountRepo(uw);
        }

        #region CRUD 方法
        public IEnumerable<Account> Accounts
        {
            get { return repo.GetAll(); }
        }

        public bool CreateAccount(Account account)
        {
            bool exist = false;

            Account acc = repo.GetAll().FirstOrDefault(x => x.Username.Equals(account.Username));
            if (acc == null)
            {
                exist = true;
                repo.Create(acc);
            }
            return exist;
        }

        public void SaveAccount(Account account)
        {
            Account dbEntry = repo.Get(account.AccountId);
            if (dbEntry != null)
            {
                dbEntry.Name = account.Name;
                dbEntry.Address = account.Address;
                if (account.ImageData != null)
                {
                    dbEntry.ImageData = account.ImageData;
                    dbEntry.ImageMimeType = account.ImageMimeType;
                }
                repo.Update(dbEntry);
            }
        }

        #endregion

    }

}
