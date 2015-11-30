//
//  FKGatewayResponseFilter.h
//
//  Copyright (c) 2015 Thong Nguyen. All rights reserved.
//

#import <Foundation/Foundation.h>


@protocol FKGatewayResponseFilter<NSObject>
-(NSObject*) gateway:(NSObject*)gateway receivedResponse:(NSObject*)response fromRequestURL:(NSString*)url withRequestObject:(NSObject*)obj;
@end