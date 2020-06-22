using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;

namespace Zemulator.Mac
{
    /// <summary>
    /// Provides a vertical stack display of label images.
    /// </summary>
    public class LabelStackView : NSView
    {
        /// <summary>
        /// Indicate that we work in a flipped grid system where y=0 is the
        /// top of the view.
        /// </summary>
        public override bool IsFlipped => true;

        /// <summary>
        /// The amount of padding, in pixels, applied around labels.
        /// </summary>
        protected int LabelPadding => 12;

        /// <summary>
        /// The list of label views we track.
        /// </summary>
        private readonly List<NSImageView> _labelViews = new List<NSImageView>();

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="LabelStackView"/> class.
        /// </summary>
        public LabelStackView() : base()
        {
        }

        #endregion

        #region Methods

        public void AddLabel( NSImage labelImage )
        {
            var imageView = new NSImageView
            {
                Image = labelImage,
                ImageAlignment = NSImageAlignment.Center,
                ImageScaling = NSImageScale.ProportionallyUpOrDown,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _labelViews.Insert( 0, imageView );

            base.AddSubview( imageView );
        }

        public void ClearLabels()
        {
            while ( Subviews.Length > 0 )
            {
                Subviews[0].RemoveFromSuperview();
            }

            _labelViews.Clear();
        }

        public override void AddSubview( NSView aView )
        {
            throw new NotSupportedException( "Please use AddLabel method instead." );
        }

        public override void AddSubview( NSView aView, NSWindowOrderingMode place, NSView otherView )
        {
            throw new NotSupportedException( "Please use AddLabel method instead." );
        }

        /// <summary>
        /// Sets the size of the frame.
        /// </summary>
        /// <param name="newSize">The new size requested.</param>
        public override void SetFrameSize( CGSize newSize )
        {
            //
            // Not sure this is safe, overriding the height we are given,
            // but it seems to work.
            //
            base.SetFrameSize( new CGSize( newSize.Width, GetHeightForWidth( newSize.Width ) ) );
        }

        /// <summary>
        /// Gets the height required for the given width. This calculates the
        /// height needed by each label and all padding.
        /// </summary>
        /// <param name="targetWidth">The target width, in pixels, of the view.</param>
        /// <returns>The height required for that width.</returns>
        private nfloat GetHeightForWidth( nfloat targetWidth )
        {
            nfloat y = LabelPadding;
            var scaleFactor = Window.BackingScaleFactor;

            foreach ( var imageView in _labelViews )
            {
                var width = Math.Min( targetWidth - ( LabelPadding * 2 ), imageView.Image.Size.Width / scaleFactor );
                var ratio = imageView.Image.Size.Height / imageView.Image.Size.Width;

                y += ( nfloat ) ( width * ratio ) + LabelPadding;
            }

            return y;
        }

        /// <summary>
        /// Lays out any subviews.
        /// </summary>
        public override void Layout()
        {
            nfloat y = LabelPadding;
            var scaleFactor = Window.BackingScaleFactor;
            var targetWidth = Frame.Size.Width;

            foreach ( var imageView in _labelViews )
            {
                var width = Math.Min( targetWidth - ( LabelPadding * 2 ), imageView.Image.Size.Width / scaleFactor );
                var ratio = imageView.Image.Size.Height / imageView.Image.Size.Width;

                imageView.Frame = new CGRect( ( targetWidth - width ) / 2.0, y, width, width * ratio );

                y += imageView.Frame.Size.Height + LabelPadding;
            }
        }

        #endregion
    }
}
