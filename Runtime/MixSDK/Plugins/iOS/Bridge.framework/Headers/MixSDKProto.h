//
//  MixSDKProto.h
//  Demo
//
//  Created by Bepic on 2022/10/19.
//

#import <Foundation/Foundation.h>

/**
 * 屏蔽日志
 * 1）__OPTIMIZE__ 是 release 默认会加的宏
 * 2）__VA_ARGS__ 是一个可变参数的宏(在__VA_ARGS__前加上##时，当可变参数的个数为0时,这里的##起到把前面多余的","去掉,否则会出错)
 */
#ifndef __OPTIMIZE__
    #define NSLog(...) NSLog(__VA_ARGS__)
#else
    #define NSLog(...){}
#endif

// Code
#define MixSDKProto_Code_Succ                                                   200
#define MixSDKProto_Code_Fail                                                   400
#define MixSDKProto_Code_Error_NotSupport                                       403
#define MixSDKProto_Code_Error_NotFound                                         404

// Key
#define MixSDKProto_Key_GameObject                                              @"MixManager"
#define MixSDKProto_Key_Symbol                                                  @"symbol"
#define MixSDKProto_Key_Arg                                                     @"arg"
#define MixSDKProto_Key_Code                                                    @"code"
#define MixSDKProto_Key_Msg                                                     @"msg"
#define MixSDKProto_Key_Data                                                    @"data"
#define MixSDKProto_Key_Success                                                 @"success"
#define MixSDKProto_Key_Failed                                                  @"failed"

// ApplePayment
#define MixSDKProto_Type_ApplePayment_Init                                      @"onInit"
#define MixSDKProto_Type_ApplePayment_Payment                                   @"onPayment"
