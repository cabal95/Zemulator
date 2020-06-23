using System;
using System.Threading.Tasks;

namespace Zemulator.Common
{
    /// <summary>
    /// An interface the describes the various methods related to timer events.
    /// </summary>
    public interface ITimer
    {
        /// <summary>
        /// Returns a task that is completed once the specified date and time
        /// has passed.
        /// </summary>
        /// <param name="dateTime">The date and time to wait until.</param>
        /// <returns>A Task that indicates when the date and time has elapsed.</returns>
        Task WaitUntil( DateTime dateTime );
    }
}
