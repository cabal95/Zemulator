//
//  ViewController.h
//  Zebra Emulator
//
//  Created by Daniel Hazelbaker on 9/21/18.
//  Copyright © 2018 Daniel Hazelbaker. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#import <CoreBluetooth/CoreBluetooth.h>
#import "TCPServer.h"

@interface ViewController : NSViewController <CBPeripheralManagerDelegate, NSTableViewDataSource, TCPServerDelegate>

- (void)clearLabels;
- (void)updateLabelWidth:(CGFloat)width height:(CGFloat)height;

@end

