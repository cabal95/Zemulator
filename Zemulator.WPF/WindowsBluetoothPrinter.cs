using System;
using System.Threading.Tasks;

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation.Metadata;
using Windows.Storage.Streams;

using Zemulator.Common;

namespace Zemulator.WPF
{
    public class WindowsBluetoothPrinter : IBluetoothPrinter
    {
        #region Fields

        /// <summary>
        /// The write callback for notifying the printer that we received data.
        /// </summary>
        private Action<byte[]> _writeCallback;

        /// <summary>
        /// The service provider for the GATT publishing.
        /// </summary>
        private GattServiceProvider _serviceProvider;

        /// <summary>
        /// The read characteristic.
        /// </summary>
        private GattLocalCharacteristic _readCharacteristic;

        /// <summary>
        /// The write characteristic.
        /// </summary>
        private GattLocalCharacteristic _writeCharacteristic;

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether bluetooth is supported.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if bluetooth is supported; otherwise, <c>false</c>.
        /// </returns>
        public static async Task<BluetoothSupport> IsSupported()
        {
            try
            {
                if ( !ApiInformation.IsApiContractPresent( "Windows.Foundation.UniversalApiContract", 4 ) )
                {
                    return BluetoothSupport.WindowsTooOld;
                }

                var adapter = await BluetoothAdapter.GetDefaultAsync();

                if ( adapter == null )
                {
                    return BluetoothSupport.NoAdapter;
                }

                if ( !adapter.IsPeripheralRoleSupported )
                {
                    return BluetoothSupport.PeripheralRoleNotSupported;
                }

                return BluetoothSupport.Supported;
            }
            catch
            {
                return BluetoothSupport.Unknown;
            }
        }

        /// <summary>
        /// Starts the server and uses the specified print server for callbacks.
        /// </summary>
        /// <param name="writeCallback">The function to call when data has been written.</param>
        public async Task<bool> Start( Action<byte[]> writeCallback )
        {
            if ( _serviceProvider != null )
            {
                throw new InvalidOperationException( "Service already running." );
            }

            if ( await IsSupported() != BluetoothSupport.Supported )
            {
                return false;
            }

            var result = await GattServiceProvider.CreateAsync( ZPrinterLEConstants.ServiceUuid );
            if ( result.Error != BluetoothError.Success )
            {
                return false;
            }

            _serviceProvider = result.ServiceProvider;

            //
            // Setup the parameters that will be used to create our characteristics.
            //
            var readParameters = new GattLocalCharacteristicParameters()
            {
                CharacteristicProperties = GattCharacteristicProperties.Read,
            };
            var writeParameters = new GattLocalCharacteristicParameters()
            {
                CharacteristicProperties = GattCharacteristicProperties.Write
            };

            //
            // Create the characteristic that clients will (not) use to read data
            // from us.
            //
            var readCharacteristicResult = await _serviceProvider.Service.CreateCharacteristicAsync( ZPrinterLEConstants.ReadFromPrinterCharacteristicUuid, readParameters );
            if ( readCharacteristicResult.Error != BluetoothError.Success )
            {
                return false;
            }

            //
            // Create the characteristic that clients will use to write data to us.
            //
            var writeCharacteristicResult = await _serviceProvider.Service.CreateCharacteristicAsync( ZPrinterLEConstants.WriteToPrinterCharacteristicUuid, writeParameters );
            if ( writeCharacteristicResult.Error != BluetoothError.Success )
            {
                return false;
            }

            _readCharacteristic = readCharacteristicResult.Characteristic;
            _readCharacteristic.ReadRequested += ReadCharacteristic_ReadRequested;

            _writeCharacteristic = writeCharacteristicResult.Characteristic;
            _writeCharacteristic.WriteRequested += WriteCharacteristic_WriteRequested;

            var advParameters = new GattServiceProviderAdvertisingParameters
            {
                IsDiscoverable = true,
                IsConnectable = true
            };
            _serviceProvider.StartAdvertising( advParameters );

            _writeCallback = writeCallback;

            return true;
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public Task Stop()
        {
            _writeCallback = null;
            _writeCharacteristic = null;
            _readCharacteristic = null;

            _serviceProvider.StopAdvertising();
            _serviceProvider = null;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Reads data and sends it to the client.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GattReadRequestedEventArgs"/> instance containing the event data.</param>
        private async void ReadCharacteristic_ReadRequested( GattLocalCharacteristic sender, GattReadRequestedEventArgs args )
        {
            var deferral = args.GetDeferral();

            var writer = new DataWriter();
            var request = await args.GetRequestAsync();

            // We don't actually support reading.

            request.RespondWithValue( writer.DetachBuffer() );
            deferral.Complete();
        }

        /// <summary>
        /// Processes a write request to our virtual bluetooth device.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GattWriteRequestedEventArgs"/> instance containing the event data.</param>
        private async void WriteCharacteristic_WriteRequested( GattLocalCharacteristic sender, GattWriteRequestedEventArgs args )
        {
            var deferral = args.GetDeferral();

            var request = await args.GetRequestAsync();
            var reader = DataReader.FromBuffer( request.Value );

            var buffer = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes( buffer );

            System.Diagnostics.Debug.WriteLine($"BLE Read {buffer.Length} bytes.");
            _writeCallback?.Invoke( buffer );

            request.Respond();
            deferral.Complete();
        }

        #endregion
    }
}
