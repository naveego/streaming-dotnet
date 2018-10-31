using System.Threading.Tasks;

namespace Naveego.Streaming
{
    /// <summary>
    /// Defines the API for writing a Golden record to a stream
    /// </summary>
    public interface IStreamWriter<in T>
    {
        Task WriteAsync(T record);
    }    
}
