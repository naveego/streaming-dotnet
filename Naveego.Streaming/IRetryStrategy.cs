using System;
using System.Threading;
using System.Threading.Tasks;

namespace Naveego.Streaming
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRetryStrategy
    {
        Task<bool> Next(CancellationToken cancellationToken);
    }    
}

