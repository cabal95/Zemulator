using System;
using System.IO;

namespace Zemulator.Common
{
    /// <summary>
    /// Provides the event information for the <see cref="PrintServer.OnLabelReceived"/> event.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class LabelEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the image stream.
        /// </summary>
        /// <value>
        /// The image stream.
        /// </value>
        public Stream ImageStream { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelEventArgs"/> class.
        /// </summary>
        /// <param name="imageStream">The image stream.</param>
        public LabelEventArgs( Stream imageStream )
        {
            ImageStream = imageStream;
        }
    }
}
