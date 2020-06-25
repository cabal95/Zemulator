using System;
using System.Collections.Generic;

using AppKit;

using Foundation;

using Microsoft.Extensions.DependencyInjection;

using Zemulator.Common;

namespace Zemulator.Mac
{
    /// <summary>
    /// The main content view controller.
    /// </summary>
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

        /// <summary>
        /// Any KVO observers we have that must be freed later.
        /// </summary>
        private List<IDisposable> _settingsObservers;

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
        /// The view controller is about to be disposed. Free any resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose( bool disposing )
        {
            base.Dispose( disposing );

            if ( _settingsObservers != null )
            {
                foreach ( var observer in _settingsObservers )
                {
                    observer.Dispose();
                }
                _settingsObservers = null;
            }
        }

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

            //
            // Observe any changes to the user defaults so we can update
            // the print server.
            //
            var defaults = NSUserDefaults.StandardUserDefaults;
            _settingsObservers = new List<IDisposable>
            {
                defaults.AddObserver( SettingsController.LabelWidthKey, 0, _ => SettingChanged( SettingsController.LabelWidthKey ) ),
                defaults.AddObserver( SettingsController.LabelHeightKey, 0, _ => SettingChanged( SettingsController.LabelWidthKey ) ),
                defaults.AddObserver( SettingsController.PrintDensityKey, 0, _ => SettingChanged( SettingsController.PrintDensityKey ) )
            };

            //
            // Create and start the print server.
            //
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IBluetoothPrinter, MacBluetoothPrinter>()
                .AddSingleton<ITimer, MacTimer>()
                .BuildServiceProvider();
            _printServer = new PrintServer( serviceProvider )
            {
                LabelWidth = NSUserDefaults.StandardUserDefaults.DoubleForKey( SettingsController.LabelWidthKey ),
                LabelHeight = NSUserDefaults.StandardUserDefaults.DoubleForKey( SettingsController.LabelHeightKey ),
                Density = ( LabelDensity ) ( int ) NSUserDefaults.StandardUserDefaults.IntForKey( SettingsController.PrintDensityKey )
            };
            _printServer.OnLabelReceived += PrintServer_OnLabelReceived;
            _printServer.Start();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// A user preferene setting has changed. Update any UI or print
        /// server properties.
        /// </summary>
        /// <param name="settingKey">The name of the setting.</param>
        private void SettingChanged( string settingKey )
        {
            var defaults = NSUserDefaults.StandardUserDefaults;

            if ( _printServer == null )
            {
                return;
            }

            if ( settingKey == SettingsController.LabelWidthKey )
            {
                _printServer.LabelWidth = defaults.DoubleForKey( SettingsController.LabelWidthKey );
            }
            else if ( settingKey == SettingsController.LabelHeightKey )
            {
                _printServer.LabelHeight = defaults.DoubleForKey( SettingsController.LabelHeightKey );
            }
            else if ( settingKey == SettingsController.PrintDensityKey )
            {
                _printServer.Density = ( LabelDensity ) ( int ) defaults.DoubleForKey( SettingsController.PrintDensityKey );
            }
        }

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
            var settingsController = Storyboard.InstantiateControllerWithIdentifier( "Settings" ) as NSWindowController;

            settingsController.ShowWindow( this );
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
