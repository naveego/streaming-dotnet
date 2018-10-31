using System;

namespace Naveego.Streaming
{
    public class HandleResult
    {
        public readonly IRetryStrategy RetryStrategy;
        public readonly bool Success;
        public static HandleResult Ok = new HandleResult(true);
        public static HandleResult Bad = new HandleResult(false);
        public static HandleResult Retry(IRetryStrategy retryStrategy) => new HandleResult(retryStrategy);

        private HandleResult(IRetryStrategy retryStrategy)
        {
            this.RetryStrategy = retryStrategy;
            this.Success = false;
        }
        private HandleResult(bool bad)
        {
            this.RetryStrategy = NoRetryStrategy.Instance;
            this.Success = bad;
        }
    }
}
