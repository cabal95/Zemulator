using System;
using System.Threading.Tasks;

using CoreFoundation;

using Zemulator.Common;

namespace Zemulator.Mac
{
    public class MacTimer : ITimer
    {
        /// <summary>
        /// Returns a task that is completed once the specified date and time
        /// has passed.
        /// </summary>
        /// <param name="dateTime">The date and time to wait until.</param>
        /// <returns>A Task that indicates when the date and time has elapsed.</returns>
        public Task WaitUntil( DateTime dateTime )
        {
            var timeSpan = dateTime - DateTime.Now;

            if ( timeSpan.TotalSeconds <= 0 )
            {
                return Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource<bool>();

            DispatchQueue.GetGlobalQueue( DispatchQueuePriority.Default )
                .DispatchAfter( new DispatchTime( DispatchTime.Now, ( long ) timeSpan.TotalMilliseconds * 1000000 ), () =>
                {
                    tcs.TrySetResult( true );
                } );

            return tcs.Task;
        }
    }
}
