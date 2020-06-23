using System;

using AppKit;

using Foundation;

using Microsoft.Extensions.DependencyInjection;

using Zemulator.Common;

namespace Zemulator.Mac
{
    public partial class ViewController : NSViewController
    {
        #region Fields

        /// <summary>
        /// The label stack view that we will add all our labels to.
        /// </summary>
        private LabelStackView _labelStackView;

        /// <summary>
        /// The print server that is handling label parsing and rendering.
        /// </summary>
        private PrintServer _printServer;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ViewController"/> class.
        /// </summary>
        /// <param name="handle">The native handle to initialize with.</param>
        public ViewController( IntPtr handle ) : base( handle )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when the <see cref="NSViewController.View"/> property has been set.
        /// </summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //
            // Configure the scrollview that will contain all the content.
            //
            var scrollView = new NSScrollView( View.Bounds )
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                DrawsBackground = false,
                HasVerticalScroller = true
            };
            View.AddSubview( scrollView );
            View.AddConstraint( scrollView.WidthAnchor.ConstraintEqualToAnchor( View.WidthAnchor ) );
            View.AddConstraint( scrollView.HeightAnchor.ConstraintEqualToAnchor( View.HeightAnchor ) );

            //
            // Configure the clip view manually so we have a bit more control.
            //
            var clipView = new NSClipView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                DrawsBackground = false
            };
            scrollView.ContentView = clipView;
            scrollView.AddConstraint( clipView.LeftAnchor.ConstraintEqualToAnchor( scrollView.LeftAnchor ) );
            scrollView.AddConstraint( clipView.TopAnchor.ConstraintEqualToAnchor( scrollView.TopAnchor ) );
            scrollView.AddConstraint( clipView.RightAnchor.ConstraintEqualToAnchor( scrollView.RightAnchor ) );
            scrollView.AddConstraint( clipView.BottomAnchor.ConstraintEqualToAnchor( scrollView.BottomAnchor ) );

            //
            // Setup our label stack to the the document view of the scroller.
            //
            _labelStackView = new LabelStackView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            scrollView.DocumentView = _labelStackView;
            clipView.AddConstraint( clipView.LeftAnchor.ConstraintEqualToAnchor( _labelStackView.LeftAnchor ) );
            clipView.AddConstraint( clipView.TopAnchor.ConstraintEqualToAnchor( _labelStackView.TopAnchor ) );
            clipView.AddConstraint( clipView.RightAnchor.ConstraintEqualToAnchor( _labelStackView.RightAnchor ) );

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IBluetoothPrinter, MacBluetoothPrinter>()
                .AddSingleton<ITimer, MacTimer>()
                .BuildServiceProvider();
            _printServer = new PrintServer( serviceProvider )
            {
                LabelHeight = 2,
                LabelWidth = 4,
                Density = LabelDensity.Density_203
            };
            _printServer.OnLabelReceived += PrintServer_OnLabelReceived;
            _printServer.Start();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the OnLabelReceived event of the PrintServer object.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments of the event.</param>
        private void PrintServer_OnLabelReceived( object sender, LabelEventArgs e )
        {
            NSApplication.SharedApplication.InvokeOnMainThread( () =>
            {
                try
                {
                    var image = NSImage.FromStream( e.ImageStream );

                    _labelStackView.AddLabel( image );

                    //
                    // Force the label stack to resize it's height.
                    //
                    _labelStackView.Frame = _labelStackView.Frame;
                }
                catch ( Exception ex )
                {
                    System.Diagnostics.Debug.WriteLine( $"Failed to parse label image: {ex.Message}" );
                }
            } );
        }

        /// <summary>
        /// Handles the Settings toolbar button action.
        /// </summary>
        /// <param name="sender">The object that sent the action.</param>
        [Action( "openSettings:" )]
        public void OpenSettings( NSObject sender )
        {
        }

        /// <summary>
        /// Handles the Clear toolbar button action.
        /// </summary>
        /// <param name="sender">The object that sent the action.</param>
        [Action( "clearLabels:" )]
        public void ClearLabels( NSObject sender )
        {
            _labelStackView.ClearLabels();

            //
            // Force the label stack to resize it's height.
            //
            _labelStackView.Frame = _labelStackView.Frame;
        }

        #endregion
    }
}
