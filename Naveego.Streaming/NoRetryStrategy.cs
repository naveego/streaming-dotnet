using System.Threading;
using System.Threading.Tasks;

namespace Naveego.Streaming
{
    /// <summary>
    /// The NoRetryStrategy will always return false.  This is used when the user does
    /// not indicate any retry strategy.
    /// </summary>
    /// <inheritdoc cref="IRetryStrategy" />
    public class NoRetryStrategy : IRetryStrategy
    {
        public static readonly NoRetryStrategy Instance = new NoRetryStrategy();
        
        public Task<bool> Next(CancellationToken cancellationToken)
        {
            return Task.Run(() => false, cancellationToken);
        }
    }
}