using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicStore.Domain.Data;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Repo;
using MusicStore.Domain.Utils;

namespace MusicStore.Service.Accounts
{

    class AccountRepo : BaseRepository<Account>
    {
        public AccountRepo(IUnitOfWork uw) : base(uw) { }

    }
}
