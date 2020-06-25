using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Extensions.DependencyInjection;

using Zemulator.Common;

namespace Zemulator.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PrintServer _printServer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Settings.Default.PropertyChanged += Settings_PropertyChanged;

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IBluetoothPrinter, WindowsBluetoothPrinter>()
                .AddSingleton<ITimer, WindowsTimer>()
                .BuildServiceProvider();
            _printServer = new PrintServer( serviceProvider )
            {
                LabelWidth = Settings.Default.LabelWidth,
                LabelHeight = Settings.Default.LabelHeight
            };
            _printServer.OnLabelReceived += PrintServer_OnLabelReceived;
            _printServer.Start();
        }

        #region Event Handlers

        /// <summary>
        /// Handles the PropertyChanged event of the Settings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void Settings_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if ( e.PropertyName == nameof( Settings.Default.LabelWidth ) )
            {
                _printServer.LabelWidth = Settings.Default.LabelWidth;
            }
            else if ( e.PropertyName == nameof( Settings.Default.LabelHeight ) )
            {
                _printServer.LabelHeight = Settings.Default.LabelHeight;
            }
            else if ( e.PropertyName == nameof( Settings.Default.PrintDensity ) )
            {
                _printServer.Density = Settings.Default.PrintDensity;
            }
        }

        /// <summary>
        /// Handles the OnLabelReceived event of the _printServer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LabelEventArgs"/> instance containing the event data.</param>
        private void PrintServer_OnLabelReceived( object sender, LabelEventArgs e )
        {
            Dispatcher.Invoke( () =>
            {
                var imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.StreamSource = e.ImageStream;
                imageSource.EndInit();

                var image = new Image()
                {
                    Source = imageSource,
                    Margin = new Thickness( 0, 0, 0, 0 ),
                    Stretch = Stretch.Uniform,
                    StretchDirection = StretchDirection.DownOnly
                };

                image.SetBinding( MaxWidthProperty, new Binding( nameof( spLabels.ActualWidth ) ) { Source = spLabels } );

                if ( spLabels.Children.Count > 0 )
                {
                    ( ( Image ) spLabels.Children[0] ).Margin = new Thickness( 0, 20, 0, 0 );
                }

                spLabels.Children.Insert( 0, image );
            } );
        }

        /// <summary>
        /// Handles the Click event of the Clear control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Clear_Click( object sender, RoutedEventArgs e )
        {
            spLabels.Children.Clear();
        }

        /// <summary>
        /// Handles the Click event of the Settings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Settings_Click( object sender, RoutedEventArgs e )
        {
            var settingsWindow = new SettingsWindow()
            {
                Owner = this
            };

            settingsWindow.ShowDialog();
        }

        #endregion
    }
}
