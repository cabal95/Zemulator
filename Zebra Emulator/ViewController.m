//
//  ViewController.m
//  Zebra Emulator
//
//  Created by Daniel Hazelbaker on 9/21/18.
//  Copyright © 2018 Daniel Hazelbaker. All rights reserved.
//

#import "ViewController.h"
#import "ZPrinterLEService.h"

@interface ViewController ()

@property (nonatomic, retain) IBOutlet NSTableView *tableView;
@property (nonatomic, retain) CBPeripheralManager *peripheralManager;
@property (nonatomic, retain) CBMutableCharacteristic *cRead, *cWrite;

@property (nonatomic, retain) NSMutableArray *labels;
@property (nonatomic, retain) NSMutableArray *pendingLabels;
@property (nonatomic, assign) CGFloat labelWidth;
@property (nonatomic, assign) CGFloat labelHeight;

@property (nonatomic, retain) NSMutableData *bluetoothData;

@property (nonatomic, retain) TCPServer *rawPrintServer;
@property (nonatomic, retain) NSMutableArray *rawPrintClients;
@property (nonatomic, retain) NSMutableData *rawPrintData;

@end


@implementation ViewController

//
// The view has been loaded, set initial values and start listening on port 9100 as
// well as via Bluetooth.
//
- (void)viewDidLoad
{
    [super viewDidLoad];
    
    self.labels = [NSMutableArray array];
    self.pendingLabels = [NSMutableArray array];
    self.bluetoothData = [NSMutableData data];

    self.rawPrintClients = [NSMutableArray array];
    self.rawPrintServer = [TCPServer new];
    self.rawPrintServer.delegate = self;
    [self.rawPrintServer listenOnPort:9100];
    self.rawPrintData = [NSMutableData data];
    
    self.peripheralManager = [[CBPeripheralManager alloc] initWithDelegate:self queue:nil options:nil];
}

//
// Add a new label to the queue of labels to be rendered.
//
- (void)addNewLabel:(NSData *)zpl
{
    @synchronized(_pendingLabels)
    {
        [_pendingLabels addObject:zpl];
        if (_pendingLabels.count == 1)
        {
            [self loadNextLabel];
        }
    }
}


//
// Begin loading the next label from Labelary and then add it to the top of the list.
//
- (void)loadNextLabel
{
    NSURL *url = [NSURL URLWithString:[NSString stringWithFormat:@"http://api.labelary.com/v1/printers/8dpmm/labels/%fx%f/0/", self.labelWidth, self.labelHeight]];
    NSData *zpl = nil;
    
    @synchronized(_pendingLabels)
    {
        zpl = _pendingLabels[0];
    }
    
    NSMutableURLRequest *request = [NSMutableURLRequest requestWithURL:url cachePolicy:NSURLRequestUseProtocolCachePolicy timeoutInterval:5];
    [request setHTTPMethod:@"POST"];
    NSLog(@"Requesting label for ZPL:\r\n%@", [[NSString alloc] initWithData:zpl encoding:NSUTF8StringEncoding]);
    [request setHTTPBody:zpl];

    NSURLSessionConfiguration *configuration = [NSURLSessionConfiguration defaultSessionConfiguration];
    NSURLSession *session = [NSURLSession sessionWithConfiguration:configuration delegate:nil delegateQueue:nil];
    
    NSURLSessionDataTask *task = [session dataTaskWithRequest:request completionHandler:^(NSData * _Nullable data, NSURLResponse * _Nullable response, NSError * _Nullable error) {
        if (data != nil )
        {
            NSImage *image = [[NSImage alloc] initWithData:data];
            NSLog(@"Got image: %@", image);
            if (image != nil)
            {
                [self->_labels insertObject:image atIndex:0];
                dispatch_async(dispatch_get_main_queue(), ^{
                    [self->_tableView reloadData];
                });
            }
            
            @synchronized(self->_pendingLabels)
            {
                [self->_pendingLabels removeObjectAtIndex:0];
                if (self->_pendingLabels.count > 0)
                {
                    [self loadNextLabel];
                }
            }
        }
        else
        {
            NSLog(@"Error getting label: %@", error);
        }
    }];
    
    [task resume];
}


- (void)clearLabels
{
    [self.labels removeAllObjects];
    [self.tableView reloadData];
}


- (void)updateLabelWidth:(CGFloat)width height:(CGFloat)height
{
    self.tableView.rowHeight = (height * 203.0f) / 2.0f;
    self.labelWidth = width;
    self.labelHeight = height;
}

#pragma mark Bluetooth delegate methods

//
// The peripheral manager's state changed, if this is a powered on event then advertise our ZPL service.
//
- (void)peripheralManagerDidUpdateState:(nonnull CBPeripheralManager *)peripheral
{
    if (peripheral.state == CBManagerStatePoweredOn) {
        CBMutableService *sZebra = [[CBMutableService alloc] initWithType:[CBUUID UUIDWithString:ZPRINTER_SERVICE_UUID] primary:YES];
        
        _cWrite = [[CBMutableCharacteristic alloc] initWithType:[CBUUID UUIDWithString:WRITE_TO_ZPRINTER_CHARACTERISTIC_UUID] properties:CBCharacteristicPropertyWrite value:nil permissions:CBAttributePermissionsWriteable];
        _cRead = [[CBMutableCharacteristic alloc] initWithType:[CBUUID UUIDWithString:READ_FROM_ZPRINTER_CHARACTERISTIC_UUID] properties:CBCharacteristicPropertyIndicate value:nil permissions:CBAttributePermissionsReadable];
        
        sZebra.characteristics = @[_cRead, _cWrite];
        
        [_peripheralManager addService:sZebra];
        
        [_peripheralManager startAdvertising:@{ CBAdvertisementDataServiceUUIDsKey: @[sZebra.UUID] }];
    }
}

//
// Peripheral manager started advertising, or had an error.
//
- (void)peripheralManagerDidStartAdvertising:(CBPeripheralManager *)peripheral error:(NSError *)error
{
}

//
// Peripheral manager received a read request. We don't really offer 2-way communication.
//
- (void)peripheralManager:(CBPeripheralManager *)peripheral didReceiveReadRequest:(CBATTRequest *)request
{
}

//
// Peripheral manager got a write request. Store the data and if we got a complete ZPL label then
// add the label to the queue.
//
- (void)peripheralManager:(CBPeripheralManager *)peripheral didReceiveWriteRequests:(NSArray<CBATTRequest *> *)requests
{
    for (CBATTRequest *request in requests) {
        if (request.characteristic == _cWrite)
        {
            [self.bluetoothData appendData:request.value];
            
            NSString *zpl = [[NSString alloc] initWithData:self.bluetoothData encoding:NSUTF8StringEncoding];
            
            if ([zpl containsString:@"^XZ"])
            {
                //
                // We got an end without a start, something bad happened so just clear all the
                // data.
                //
                if ([zpl containsString:@"^XA"] == NO)
                {
                    [self.bluetoothData setLength:0];
                    return;
                }

                //
                // Find an print all labels.
                //
                while (true)
                {
                    NSRange start = [zpl rangeOfString:@"^XA"];
                    NSRange end = [zpl rangeOfString:@"^XZ"];
                    
                    if (start.location == NSNotFound || end.location == NSNotFound)
                    {
                        break;
                    }
                    
                    NSString *label = [zpl substringWithRange:NSMakeRange(start.location, end.location - start.location + end.length)];
                    
                    [self addNewLabel:[label dataUsingEncoding:NSUTF8StringEncoding]];
                    
                    zpl = [zpl substringFromIndex:end.location + end.length];
                }
                
                [self.bluetoothData setData:[zpl dataUsingEncoding:NSUTF8StringEncoding]];
            }

            [peripheral respondToRequest:request withResult:CBATTErrorSuccess];
        }
        else
        {
            [peripheral respondToRequest:request withResult:CBATTErrorWriteNotPermitted];
        }
    }
}


#pragma mark NSTableViewDataSource

//
// Get the number of labels we have to be displayed.
//
- (NSInteger)numberOfRowsInTableView:(NSTableView *)tableView
{
    return self.labels.count;
}

//
// Get the image to display in a specific row.
//
- (id)tableView:(NSTableView *)tableView objectValueForTableColumn:(NSTableColumn *)tableColumn row:(NSInteger)row
{
    id image = self.labels[row];
    
    if ([image isKindOfClass:[NSNull class]])
    {
        return nil;
    }
    
    return image;
}


#pragma mark TCPServerDelegate

//
// The Raw print server has received a new connection.
//
- (void)tcpServer:(TCPServer *)tcpServer didAcceptConnection:(NSFileHandle *)fileHandle
{
    //
    // Clear any existing print data.
    //
    [self.rawPrintData setLength:0];
    
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(clientConnectionDidReadNotification:)
                                                 name:NSFileHandleReadCompletionNotification
                                               object:fileHandle];
    [fileHandle readInBackgroundAndNotify];
    [self.rawPrintClients addObject:fileHandle];
}

//
// The raw print server received data from a client.
//
- (void)clientConnectionDidReadNotification:(NSNotification *)notification
{
    NSDictionary *userInfo = notification.userInfo;
    NSFileHandle *fileHandle = notification.object;
    NSData *data = [userInfo objectForKey:NSFileHandleNotificationDataItem];
    
    if (data.length == 0)
    {
        [[NSNotificationCenter defaultCenter] removeObserver:self
                                                        name:NSFileHandleReadCompletionNotification
                                                      object:fileHandle];
        [self.rawPrintClients removeObject:fileHandle];
        [fileHandle closeFile];
        
        if (self.rawPrintData.length > 0)
        {
            NSLog(@"Printing...");
            [self addNewLabel:[self.rawPrintData copy]];
            [self.rawPrintData setLength:0];
        }
    }
    else
    {
        [self.rawPrintData appendData:data];

        [fileHandle readInBackgroundAndNotify];
    }
}

@end
