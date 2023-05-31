//
//  MixSDKBridge.h
//  Demo
//
//  Created by Bepic on 2022/10/19.
//

#import <Foundation/Foundation.h>
#import "MixSDKProto.h"
#import "CSBean.h"

NS_ASSUME_NONNULL_BEGIN

static BOOL UNITY_2019_2_OR_OLDER;
/// 桥接工具
@interface MixSDKBridge : NSObject

/// 获取单例对象
+ (MixSDKBridge *) getInstance;

/// OC 回调到 C#
/// - Parameters:
///   - bean: Bean
///   - params: 回调参数
+ (void) callback:(CSBean * _Nullable)bean params:(Params * _Nonnull) params;

/// OC 回调到 C#
/// - Parameters:
///   - bean: Bean
///   - code: 状态码
///   - msg: 回调信息
///   - data: 回调数据
+ (void) callback:(CSBean * _Nullable)bean code:(int)code msg:(NSString * _Nonnull)msg data:(NSString * _Nullable)data;

/// OC 回调到 C#[成功状态]
/// - Parameters:
///   - bean: Bean
///   - msg: 回调信息
+ (void) succ:(CSBean * _Nullable)bean msg:(NSString * _Nonnull)msg;

/// OC 回调到 C#[成功状态]
/// - Parameters:
///   - bean: Bean
///   - msg: 回调信息
///   - data: 回调数据
+ (void) succ:(CSBean * _Nullable)bean msg:(NSString * _Nonnull)msg data:(NSString * _Nonnull)data;

/// OC 回调到 C#[失败状态]
/// - Parameters:
///   - bean: Bean
///   - msg: 回调信息
+ (void) fail:(CSBean * _Nullable)bean msg:(NSString * _Nonnull)msg;

/// OC 调用 C#(用于Unity2019.3及以上版本)
/// - Parameters:
///   - goName: The name of the target GameObject
///   - name: The script method to call on that object(必须是挂在对应游戏物体上的脚本定义的函数)
///   - msg: The message string to pass to the called method
+ (void) sendMessageToGOWithName:(NSString * _Nonnull)goName functionName:(NSString * _Nonnull)name message:(NSString * _Nullable)msg;

/// OC 调用 C#(适配Unity2019.2及以下版本)
/// - Parameters:
///   - obj: The name of the target GameObject
///   - method: The script method to call on that object
///   - msg: The message string to pass to the called method
+ (void) unitySendMessage:(NSString * _Nonnull)obj method:(NSString * _Nonnull)method msg:(NSString * _Nullable)msg;
@end

NS_ASSUME_NONNULL_END
