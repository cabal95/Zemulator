using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Zemulator.Common;

namespace Zebra_Emulator
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsWindow"/> class.
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent();

            cbLabelWidth.ItemStringFormat = "f2\"";
            cbLabelHeight.ItemStringFormat = "f2\"";

            var sizes = new List<Item<double>>();
            for ( double size = 2.0d; size <= 6.0d; size += 0.25d )
            {
                sizes.Add( new Item<double>( size, size.ToString( "F2" ) + "\"" ) );
            }

            var densities = new List<Item<LabelDensity>>
            {
                new Item<LabelDensity>( LabelDensity.Density_152, "152dpi" ),
                new Item<LabelDensity>( LabelDensity.Density_203, "203dpi" ),
                new Item<LabelDensity>( LabelDensity.Density_300, "300dpi" ),
                new Item<LabelDensity>( LabelDensity.Density_600, "600dpi" )
            };

            cbLabelWidth.ItemsSource = sizes;
            cbLabelHeight.ItemsSource = sizes;
            cbPrintDensity.ItemsSource = densities;

            cbLabelWidth.SelectedItem = sizes.SingleOrDefault( a => a.Value == Settings.Default.LabelWidth );
            cbLabelHeight.SelectedItem = sizes.SingleOrDefault( a => a.Value == Settings.Default.LabelHeight );
            cbPrintDensity.SelectedItem = densities.SingleOrDefault( a => a.Value == Settings.Default.PrintDensity );

            _ = DetectBluetooth();
        }

        /// <summary>
        /// Detects the bluetooth support.
        /// </summary>
        private async Task DetectBluetooth()
        {
            switch ( await WindowsBluetoothPrinter.IsSupported() )
            {
                case BluetoothSupport.Supported:
                    lbBluetooth.Text = "Bluetooth support has been detected and enabled.";
                    break;

                case BluetoothSupport.Unknown:
                    lbBluetooth.Text = "Unable to determine bluetooth support.";
                    break;

                case BluetoothSupport.WindowsTooOld:
                    lbBluetooth.Text = "Your Windows version is too old to enable bluetooth support.";
                    break;

                case BluetoothSupport.NoAdapter:
                    lbBluetooth.Text = "No bluetooth adapter detected.";
                    break;

                case BluetoothSupport.PeripheralRoleNotSupported:
                    lbBluetooth.Text = "Your bluetooth adapter does not support peripheral mode.";
                    break;
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cbLabelWidth control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void cbLabelWidth_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            Settings.Default.LabelWidth = ( ( Item<double> ) cbLabelWidth.SelectedItem ).Value;
            Settings.Default.Save();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cbLabelHeight control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void cbLabelHeight_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            Settings.Default.LabelHeight = ( ( Item<double> ) cbLabelHeight.SelectedItem ).Value;
            Settings.Default.Save();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cbPrintDensity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void cbPrintDensity_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            Settings.Default.PrintDensity = ( ( Item<LabelDensity> ) cbPrintDensity.SelectedItem ).Value;
            Settings.Default.Save();
        }

        private class Item<T>
        {
            public T Value { get; }

            public string Text { get; }

            public Item( T value, string text )
            {
                Value = value;
                Text = text;
            }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}
