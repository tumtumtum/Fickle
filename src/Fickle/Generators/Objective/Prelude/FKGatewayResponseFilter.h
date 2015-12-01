//
//  FKGatewayResponseFilter.h
//
//  Copyright (c) 2015 Thong Nguyen. All rights reserved.
//

#import <Foundation/Foundation.h>


@protocol FKGatewayResponseFilter<NSObject>
-(id) gateway:(NSObject*)gateway receivedResponse:(id)response fromRequestURL:(NSString*)url withRequestObject:(id)obj;
@end