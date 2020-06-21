using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Linq;

namespace ZE.Common
{
    /// <summary>
    /// Handles receiving and processing label ZPL data over a raw
    /// printer port.
    /// </summary>
    public class PrintServer
    {
        #region Events

        /// <summary>
        /// Occurs when a label has been rendered into a PNG and is
        /// ready to be displayed.
        /// </summary>
        public event EventHandler<LabelEventArgs> OnLabelReceived;

        #endregion

        #region Fields

        /// <summary>
        /// The TCP listener that acts as a raw printer port.
        /// </summary>
        private TcpListener _listener;

        /// <summary>
        /// The bluetooth printer.
        /// </summary>
        private readonly IBluetoothPrinter _bluetoothPrinter;

        /// <summary>
        /// The client that we use to connect to Labelary for rendering.
        /// </summary>
        private readonly HttpClient _labelaryClient = new HttpClient();

        /// <summary>
        /// The cancellation token source to terminate background tasks.
        /// </summary>
        private readonly CancellationTokenSource _cancelSource = new CancellationTokenSource();

        /// <summary>
        /// The ZPL label queue.
        /// </summary>
        private readonly Queue<string> _zplLabels = new Queue<string>();

        /// <summary>
        /// The ZPL semaphore that indicates when a new label is in the queue.
        /// </summary>
        private readonly SemaphoreSlim _zplSemaphore = new SemaphoreSlim( 1 );

        private string _bluetoothData = string.Empty;
        private readonly object _bluetoothDataLock = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the width of the label to use when rendering.
        /// </summary>
        /// <value>
        /// The width of the label to use when rendering.
        /// </value>
        public double LabelWidth { get; set; } = 4d;

        /// <summary>
        /// Gets or sets the height of the label to use when rendering.
        /// </summary>
        /// <value>
        /// The height of the label to use when rendering.
        /// </value>
        public double LabelHeight { get; set; } = 2d;

        /// <summary>
        /// Gets or sets the density to use when rendering the labels.
        /// </summary>
        /// <value>
        /// The density to use when rendering the labels.
        /// </value>
        public LabelDensity Density { get; set; } = LabelDensity.Density_203;

        #endregion

        #region Constructors

        public PrintServer()
        {
        }

        public PrintServer( IBluetoothPrinter bluetoothPrinter )
            : this()
        {
            _bluetoothPrinter = bluetoothPrinter;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the print server.
        /// </summary>
        public void Start()
        {
            _listener = new TcpListener( IPAddress.Any, 9100 );
            _listener.Start();

            _bluetoothData = string.Empty;
            _bluetoothPrinter?.Start( data => ReceivedBluetoothData( data ) );

            Task.Run( ConnectionTask );
            Task.Run( RenderTask );
        }

        /// <summary>
        /// Stops the print server.
        /// </summary>
        public void Stop()
        {
            _cancelSource.Cancel();
            _bluetoothPrinter?.Stop();
            _listener.Stop();
        }

        private void ReceivedBluetoothData( byte[] data )
        {
            lock ( _bluetoothDataLock )
            {
                _bluetoothData += Encoding.UTF8.GetString( data );
                System.Diagnostics.Debug.WriteLine($"Appending {data.Length} bytes to BL data.");

                while ( true )
                {
                    var endIndex = _bluetoothData.ToString().IndexOf( "^XZ" );
                    if ( endIndex < 0 )
                    {
                        break;
                    }

                    var labelData = _bluetoothData.Substring( 0, endIndex + 3 );
                    _bluetoothData = _bluetoothData.Substring( endIndex + 3 );

                    using ( var ms = new MemoryStream( Encoding.UTF8.GetBytes( labelData ) ) )
                    {
                        System.Diagnostics.Debug.WriteLine("Submitted 1 BLE label.");
                        ProcessLabelData( ms );
                    }
                }
            }
        }

        /// <summary>
        /// Processes the label data.
        /// </summary>
        /// <param name="stream">The stream.</param>
        private void ProcessLabelData( MemoryStream stream )
        {
            stream.Seek( 0, SeekOrigin.Begin );

            lock ( _zplLabels )
            {
                //
                // Convert the ZPL data to a string for parsing.
                //
                var dataString = Encoding.UTF8.GetString( stream.ToArray() );

                //
                // Deal with multiple labels sent.
                //
                var labels = dataString.Split( new string[] { "^XZ" }, StringSplitOptions.None )
                    .Where( a => a.Contains( "^XA" ) )
                    .Select( a => a + "^XZ" )
                    .ToList();

                //
                // Queue up each label found.
                //
                foreach ( var label in labels )
                {
                    _zplLabels.Enqueue( label );
                }

                _zplSemaphore.Release();
            }
        }

        /// <summary>
        /// Background task that handles incoming TCP connections.
        /// </summary>
        private async Task ConnectionTask()
        {
            while ( !_cancelSource.Token.IsCancellationRequested )
            {
                var client = await _listener.AcceptTcpClientAsync();

                var stream = client.GetStream();
                var buffer = new MemoryStream();

                //
                // Read data from the client 4K at a time.
                //
                var tempBuffer = new byte[4096];
                int readSize;
                while ( ( readSize = await stream.ReadAsync( tempBuffer, 0, tempBuffer.Length ) ) > 0 )
                {
                    _cancelSource.Token.ThrowIfCancellationRequested();
                    buffer.Write( tempBuffer, 0, readSize );
                }

                //
                // Process any labels found in the stream.
                //
                ProcessLabelData( buffer );

                client.Close();
            }
        }

        /// <summary>
        /// Background task that handles rendering the ZPL data into a PNG.
        /// </summary>
        private async Task RenderTask()
        {
            while ( !_cancelSource.Token.IsCancellationRequested )
            {
                await _zplSemaphore.WaitAsync( _cancelSource.Token );
                string zplLabel;

                while ( true )
                {
                    await Task.Delay(250);

                    //
                    // Try to get the next label.
                    //
                    lock ( _zplLabels )
                    {
                        if ( _zplLabels.Count > 0 )
                        {
                            zplLabel = _zplLabels.Dequeue();
                            System.Diagnostics.Debug.WriteLine("Dequeued 1 ZPL label: " + _zplLabels.Count.ToString());
                        }
                        else
                        {
                            break;
                        }
                    }

                    try
                    {
                        //
                        // Request Labelary to render the label for us.
                        //
                        var uri = new Uri( $"http://api.labelary.com/v1/printers/8dpmm/labels/{LabelWidth}x{LabelHeight}/0/" );
                        var content = new StreamContent( new MemoryStream( Encoding.UTF8.GetBytes( zplLabel ) ) );
                        var response = await _labelaryClient.PostAsync( uri, content, _cancelSource.Token );

                        if (!response.IsSuccessStatusCode)
                        {
                            var msg = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"Failed to retrieve ZPL label: {msg}");
                        }
                        else
                        {
                            var imageStream = await response.Content.ReadAsStreamAsync();

                            System.Diagnostics.Debug.WriteLine("Rendered 1 ZPL label.");
                            OnLabelReceived.Invoke(this, new LabelEventArgs(imageStream));
                        }
                    }
                    catch ( Exception ex )
                    {
                        System.Diagnostics.Debug.WriteLine( $"Failed to retrieve ZPL label: {ex.Message}" );
                    }
                }
            }
        }

        #endregion
    }
}
