using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Naveego.Streaming
{
    /// <summary>
    /// Defines the MatchGroupStreamReader API.
    /// </summary>
    public interface IStreamReader<out T>
    {
        /// <summary>
        /// Starts the processing of the match group stream asynchronously.
        /// </summary>
        /// <returns></returns>
        Task ReadAsync(Func<T, Task<HandleResult>> onMessage, CancellationToken cancellationToken);
    }
    
    public static class StreamReaderExt
    {
        /// <summary>
        /// Reads the message from the underlying stream synchronously.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="onMessage"></param>
        /// <typeparam name="T"></typeparam>
        public static void Read<T>(this IStreamReader<T> @this, Func<T, Task<HandleResult>> onMessage)
        {
            var cancelSource = new CancellationTokenSource();
            @this.ReadAsync(onMessage, cancelSource.Token).Wait(cancelSource.Token);
        }

        /// <summary>
        /// Reads the message from the underlying stream asynchronously.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="onMessage"></param>
        /// <returns></returns>
        public static Task ReadAsync<T>(this IStreamReader<T> @this, Func<T, Task<HandleResult>> onMessage)
        {
            var cancelSource = new CancellationTokenSource();
            return @this.ReadAsync(onMessage, cancelSource.Token);
        }
    }
}