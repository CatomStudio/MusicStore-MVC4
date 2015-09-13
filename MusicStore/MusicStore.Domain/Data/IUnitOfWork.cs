using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MusicStore.Domain.Data
{
    public interface IUnitOfWork : IDisposable
    {
        IDbConnection Connection { get; }
    }
}
