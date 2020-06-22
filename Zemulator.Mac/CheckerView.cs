using System;
using AppKit;
using Foundation;
using CoreGraphics;

namespace Zemulator.Mac
{
    /// <summary>
    /// Displays a simple checkerboard background.
    /// </summary>
    [Register( "CheckerView" )]
    public class CheckerView : NSView
    {
        /// <summary>
        /// Indicates we operate in an inverted y-axis.
        /// </summary>
        public override bool IsFlipped => true;

        /// <summary>
        /// Creates a new instance of the <see cref="CheckerView"/> class.
        /// </summary>
        /// <param name="handle">The native handle for this view.</param>
        public CheckerView( IntPtr handle ) : base( handle )
        {
        }

        /// <summary>
        /// Draws ourself.
        /// </summary>
        /// <param name="dirtyRect">The rectangle that must be drawn.</param>
        public override void DrawRect( CGRect dirtyRect )
        {
            const int boxSize = 16;
            var colors = new[] { NSColor.DarkGray, NSColor.LightGray };
            int colorIndex;
            int yIndex = 0;

            for ( int y = 0; y < Frame.Size.Height; y += boxSize, yIndex += 1 )
            {
                colorIndex = yIndex & 1;

                for ( int x = 0, xIndex = 0; x < Frame.Size.Width; x += boxSize, xIndex += 1 )
                {
                    var color = colors[( xIndex + colorIndex ) & 1];

                    color.Set();
                    NSBezierPath.FillRect( new CGRect( x, y, boxSize, boxSize ) );
                }
            }
        }
    }
}
