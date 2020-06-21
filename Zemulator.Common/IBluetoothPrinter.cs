using System;
using System.Threading.Tasks;

namespace ZE.Common
{
    /// <summary>
    /// Defines the interface that OS implementations must follow to provide
    /// bluetooth features.
    /// </summary>
    public interface IBluetoothPrinter
    {
        /// <summary>
        /// Starts the server and uses the specified print server for callbacks.
        /// </summary>
        /// <param name="writeCallback">The function to call when data has been written.</param>
        /// <returns>
        ///   <c>true</c> if the server started successfully; otherwise <c>false</c>.
        /// </returns>
        Task<bool> Start( Action<byte[]> writeCallback );

        /// <summary>
        /// Stops this instance.
        /// </summary>
        /// <returns></returns>
        Task Stop();
    }
}
