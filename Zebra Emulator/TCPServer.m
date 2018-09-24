//
//  TCPServer.m
//  Zebra Emulator
//
//  Created by Daniel Hazelbaker on 9/21/18.
//  Copyright © 2018 Daniel Hazelbaker. All rights reserved.
//

#import "TCPServer.h"
#import <sys/socket.h>
#import <netinet/in.h>

@interface TCPServer ()

@property (nonatomic, assign) CFSocketRef socket;
@property (nonatomic, retain) NSFileHandle *listeningHandle;

@end

@implementation TCPServer

- (void)listenOnPort:(NSUInteger)port
{
    self.socket = CFSocketCreate(kCFAllocatorDefault, PF_INET, SOCK_STREAM, IPPROTO_TCP, 0, NULL, NULL);
    if (!self.socket)
    {
        [NSException raise:NSGenericException format:@"Could not create socket."];
    }
    
    int reuse = true;
    int fd = CFSocketGetNative(self.socket);
    if (setsockopt(fd, SOL_SOCKET, SO_REUSEADDR, (void *)&reuse, sizeof(int)) != 0)
    {
        [NSException raise:NSGenericException format:@"Unable to set socket options."];
    }
    
    struct sockaddr_in address;
    memset(&address, 0, sizeof(address));
    address.sin_len = sizeof(address);
    address.sin_family = AF_INET;
    address.sin_addr.s_addr = htonl(INADDR_ANY);
    address.sin_port = htons(port);
    CFDataRef addressData = CFDataCreate(NULL, (const UInt8 *)&address, sizeof(address));
    if (CFSocketSetAddress(self.socket, addressData) != kCFSocketSuccess)
    {
        [NSException raise:NSGenericException format:@"Unable to bind socket address."];
    }
    
    self.listeningHandle = [[NSFileHandle alloc] initWithFileDescriptor:fd closeOnDealloc:YES];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(receiveIncomingConnectionNotification:) name:NSFileHandleConnectionAcceptedNotification object:self.listeningHandle];
    [self.listeningHandle acceptConnectionInBackgroundAndNotify];
}


- (void)receiveIncomingConnectionNotification:(NSNotification *)notification
{
    NSDictionary *userInfo = notification.userInfo;
    NSFileHandle *newConnection = [userInfo objectForKey:NSFileHandleNotificationFileHandleItem];
    
    if (newConnection != nil)
    {
        [self.delegate tcpServer:self didAcceptConnection:newConnection];
    }
    
    [self.listeningHandle acceptConnectionInBackgroundAndNotify];
}

@end
