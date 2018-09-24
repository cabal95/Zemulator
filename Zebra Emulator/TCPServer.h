//
//  TCPServer.h
//  Zebra Emulator
//
//  Created by Daniel Hazelbaker on 9/21/18.
//  Copyright © 2018 Daniel Hazelbaker. All rights reserved.
//

#import <Foundation/Foundation.h>

@protocol TCPServerDelegate;

@interface TCPServer : NSObject

@property (nonatomic, retain) id<TCPServerDelegate> delegate;

- (void)listenOnPort:(NSUInteger)port;

@end

@protocol TCPServerDelegate

- (void)tcpServer:(TCPServer *)tcpServer didAcceptConnection:(NSFileHandle *)fileHandle;

@end

