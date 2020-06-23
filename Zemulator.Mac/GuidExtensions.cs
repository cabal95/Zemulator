using System;

using CoreBluetooth;

namespace Zemulator.Mac
{
    /// <summary>
    /// Extension methods related to Guids.
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>
        /// Convert a <see cref="Guid"/> value into a <see cref="CBUUID"/> value.
        /// </summary>
        /// <param name="guid">The value to be converted.</param>
        /// <returns>The <see cref="CBUUID"/> value.</returns>
        public static CBUUID ToCBUuid( this Guid guid )
        {
            return CBUUID.FromString( guid.ToString() );
        }
    }
}
