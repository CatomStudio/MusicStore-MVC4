using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicStore.Domain.Entities;

namespace MusicStore.Domain.Abstract
{
    public interface IAccountRepository
    {
        IQueryable<Account> Accounts { get; }
        bool CreateAccount(Account account);
        void SaveAccount(Account account);
    }
}
