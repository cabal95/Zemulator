using System;
using System.Threading.Tasks;

using CoreBluetooth;

using Foundation;

using Zemulator.Common;

namespace Zemulator.Mac
{
    public class MacBluetoothPrinter : NSObject, IBluetoothPrinter, ICBPeripheralManagerDelegate
    {
        private CBPeripheralManager _peripheralManager;
        private CBCharacteristic _readCharacteristic;
        private CBCharacteristic _writeCharacteristic;

        private Action<byte[]> _writeCallback;

        public Task<bool> Start( Action<byte[]> writeCallback )
        {
            if ( _peripheralManager != null )
            {
                throw new NotSupportedException( "Bluetooth has already started." );
            }

            _writeCallback = writeCallback;
            _peripheralManager = new CBPeripheralManager( this, null );

            return Task.FromResult( true );
        }

        [Export( "peripheralManagerDidUpdateState:" )]
        public void StateUpdated( CBPeripheralManager peripheral )
        {

            if ( peripheral.State == CBPeripheralManagerState.PoweredOn )
            {
                var service = new CBMutableService( ZPrinterLEConstants.ServiceUuid.ToCBUuid(), true );

                _readCharacteristic = new CBMutableCharacteristic( ZPrinterLEConstants.ReadFromPrinterCharacteristicUuid.ToCBUuid(), CBCharacteristicProperties.Read, null, CBAttributePermissions.Readable );
                _writeCharacteristic = new CBMutableCharacteristic( ZPrinterLEConstants.WriteToPrinterCharacteristicUuid.ToCBUuid(), CBCharacteristicProperties.Write, null, CBAttributePermissions.Writeable );

                service.Characteristics = new[] { _readCharacteristic, _writeCharacteristic };

                _peripheralManager.AddService( service );

                var dictionary = NSDictionary.FromObjectAndKey( NSArray.FromObjects( service.UUID ), CBAdvertisement.DataServiceUUIDsKey );
                _peripheralManager.StartAdvertising( dictionary );
            }
        }

        [Export( "peripheralManager:didReceiveWriteRequests:" )]
        public void WriteRequestReceived( CBPeripheralManager peripheral, CBATTRequest[] requests )
        {
            foreach ( var request in requests )
            {
                if ( request.Characteristic == _writeCharacteristic )
                {
                    _writeCallback( request.Value.ToArray() );

                    peripheral.RespondToRequest( request, CBATTError.Success );
                }
                else
                {
                    peripheral.RespondToRequest( request, CBATTError.WriteNotPermitted );
                }
            }
        }

        public Task Stop()
        {
            _peripheralManager?.StopAdvertising();
            _peripheralManager = null;
            _readCharacteristic = null;
            _writeCharacteristic = null;
            _writeCallback = null;

            return Task.CompletedTask;
        }
    }
}
