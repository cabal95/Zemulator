using System;
using System.Threading.Tasks;

using Zemulator.Common;

namespace Zemulator.WPF
{
    public class WindowsTimer : ITimer
    {
        /// <summary>
        /// Returns a task that is completed once the specified date and time
        /// has passed.
        /// </summary>
        /// <param name="dateTime">The date and time to wait until.</param>
        /// <returns>
        /// A Task that indicates when the date and time has elapsed.
        /// </returns>
        public Task WaitUntil( DateTime dateTime )
        {
            var ms = ( int ) dateTime.Subtract( DateTime.Now ).TotalMilliseconds;

            if ( ms <= 0 )
            {
                return Task.CompletedTask;
            }

            return Task.Delay( ms );
        }
    }
}
