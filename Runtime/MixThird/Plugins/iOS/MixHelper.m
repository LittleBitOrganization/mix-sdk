#import "MixHelper.h"
#import <Foundation/Foundation.h>
#import <FBSDKCoreKit/FBSDKCoreKit.h>
#import <FBSDKShareKit/FBSDKShareKit-Swift.h>
// #import "UnityInterface.h"

@import Firebase;

// 声明外部接口
extern void UnitySendMessage(const char*, const char*, const char*);

// Converts an NSString into a const char* ready to be sent to Unity
static const char* nsstringToChar(NSString* input){
    const char* string = [input UTF8String];
    return string ? strdup(string) : NULL;
}

static NSString* charToNSString(char* x){
    return ((x) != NULL ? [NSString stringWithUTF8String:x] : [NSString stringWithUTF8String:""]);
}

void bridgeUploadAddToCart(float price, char * cur, char * itemId){
    NSString * contentId = charToNSString(itemId);
    NSString * currency = charToNSString(cur);

    [[MixHelper sharedInstance] addToCart:price currency:currency contentId:contentId];
}

void bridgeInitCheckout(float price, char * cur, char * itemId){
    NSString * contentId = charToNSString(itemId);
    NSString * currency = charToNSString(cur);

    [[MixHelper sharedInstance] initCheckout:price currency:currency contentId:contentId];
}

void bridgeUploadPurchase(float price, char * cur, char * itemId){
    NSString * contentId = charToNSString(itemId);
    NSString * currency = charToNSString(cur);

    [[MixHelper sharedInstance] purchase:price currency:currency contentId:contentId];
}

void bridgeIncRevenue(float revenue){
    [[MixHelper sharedInstance] incRevenue:revenue];
}

void bridgeShareLinkByFacebook(char * url) {
    [[MixHelper sharedInstance] shareLinkByFacebook:charToNSString(url)];
}

void bridgeSharePhotoByFacebook(char * imageBytes) {
    /// NSLog(@"MIXSDK[OC]-> imageBytes:%s;", imageBytes);
    NSString * imageString = charToNSString(imageBytes);
    // NSLog(@"MIXSDK[OC]-> imageString:%@;", imageString);
    NSData * imageData = [[NSData alloc] initWithBase64EncodedString:imageString options:NSDataBase64DecodingIgnoreUnknownCharacters];
    UIImage * image = [UIImage imageWithData: imageData];
    [[MixHelper sharedInstance] sharePhotoByFacebook:image];
}

@interface MixHelper ()<FBSDKSharingDelegate>
@end
static MixHelper * _MixHelper = nil;

@implementation MixHelper

+(void)load{
    __block id observer = [[NSNotificationCenter defaultCenter]
         addObserverForName:UIApplicationDidFinishLaunchingNotification
         object:nil
         queue:nil
         usingBlock:^(NSNotification *note) {
        NSLog(@"UP facebook ready init after the sdk success ");
        [[FBSDKSettings sharedSettings] setAutoLogAppEventsEnabled:true];
        [[FBSDKSettings sharedSettings] setAdvertiserTrackingEnabled:true];
        
        [FBSDKApplicationDelegate.sharedInstance application:[UIApplication sharedApplication] didFinishLaunchingWithOptions:note.userInfo];
        NSLog(@"UP facebook setup appDelegateProtcol");
        [[NSNotificationCenter defaultCenter] removeObserver:observer];
    }];
    
    [FIRApp configure];
}

+(instancetype)sharedInstance{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        _MixHelper = [MixHelper new];
    });
    return _MixHelper;
}

- (void) addToCart:(float)price currency:(NSString*) currency contentId:(NSString*)  contentId{
    [[FBSDKAppEvents shared] logEvent: FBSDKAppEventNameAddedToCart valueToSum:price parameters:
            @{FBSDKAppEventParameterNameContentType:@"product",
              FBSDKAppEventParameterNameContentID:contentId,
              FBSDKAppEventParameterNameCurrency:currency
            }];
    
    [FIRAnalytics logEventWithName:kFIREventAddToCart
            parameters:@{
            kFIRParameterValue: [NSNumber numberWithDouble:price],
            kFIRParameterCurrency:currency,
            kFIRParameterItems : @[
                    @{kFIRParameterItemName : contentId}
                  ]
        }];
}

- (void) initCheckout:(float)price currency:(NSString*) currency contentId:(NSString*)  contentId{
//    [[FBSDKAppEvents shared] logEvent: FBSDKAppEventNameInitiatedCheckout valueToSum:price parameters:
//            @{FBSDKAppEventParameterNameContentType:@"product",
//              FBSDKAppEventParameterNameContentID:contentId,
//              FBSDKAppEventParameterNameCurrency:currency
//            }];
    
    [FIRAnalytics logEventWithName:kFIREventBeginCheckout
            parameters:@{
            kFIRParameterValue: [NSNumber numberWithDouble:price],
            kFIRParameterCurrency:currency,
            kFIRParameterItems : @[
                    @{kFIRParameterItemName : contentId}
                  ]
        }];
}

- (void) purchase:(float)price currency:(NSString*) currency contentId:(NSString*)  contentId{
    
    [FIRAnalytics logEventWithName:kFIREventPurchase
            parameters:@{
            kFIRParameterValue: [NSNumber numberWithDouble:price],
            kFIRParameterCurrency:currency,
            kFIRParameterItems : @[
                    @{kFIRParameterItemName : contentId}
                  ]
        }];
}

- (void) incRevenue:(float)revenue{
    NSUserDefaults *mNsUserDefaults = [NSUserDefaults standardUserDefaults];
//    NSString * date = [mNsUserDefaults stringForKey:@"mix_firebase_lastdate"];
//    float preDateTotal = [mNsUserDefaults floatForKey:@"mix_firebase_todaytotal"];
    float loopTotal = [mNsUserDefaults floatForKey:@"mix_firebase_looptotal"];

//    NSString * today = [self getToday];
//    if(!date){
//        date = today;
//    }
//    if(![today isEqual:date]){
//        preDateTotal = 0;
//    }
//
//    NSLog(@"mix inc revenue %@ , %f , %f add:%f", date, preDateTotal, loopTotal, revenue);
//    float dateTotal = preDateTotal + revenue;

//    [self sendTCPAEvent:preDateTotal withTotal:dateTotal];
    
    loopTotal += revenue;
    if(loopTotal >= 0.01){
        [self sendROASEvent:loopTotal];
        loopTotal = 0;
    }
    
//    NSLog(@"mixthird inc revenue save %@ , %f , %f", today, dateTotal, loopTotal);
    NSLog(@"mixthird inc revenue save %f", loopTotal);
    
//    [mNsUserDefaults setValue:today forKey:@"mix_firebase_lastdate"];
//    [mNsUserDefaults setValue:[NSNumber numberWithDouble:dateTotal] forKey:@"mix_firebase_todaytotal"];
    [mNsUserDefaults setValue:[NSNumber numberWithDouble:loopTotal] forKey:@"mix_firebase_looptotal"];
    [mNsUserDefaults synchronize];
}

-(void) sendROASEvent:(float) now{
    NSString * eventName = @"Total_Ads_Revenue_001";
    [FIRAnalytics logEventWithName:eventName
                        parameters: @{
                            kFIRParameterCurrency: @"USD", // All Applovin revenue is sent in USD
                            kFIRParameterValue: [NSNumber numberWithFloat:now]
                        }];
    NSLog(@"mixthird send %@, %f", eventName, now);
}

- (void) shareLinkByFacebook:(NSString * _Nullable) url {
    if (nil == url) {
        UnitySendMessage("MixShare", "shareFacebookAsyncFail", "{\"code\":-1, \"msg\":\"url must be not null\"}");
        return;
    }
    UIViewController * viewController = [self getCurrentVC];
    FBSDKShareLinkContent * content = [[FBSDKShareLinkContent alloc] init];
    content.contentURL = [NSURL URLWithString:url];
    FBSDKShareDialog * dialog = [FBSDKShareDialog dialogWithViewController:viewController withContent:content delegate:self];
    [dialog show];
}

- (void) sharePhotoByFacebook:(UIImage * _Nullable) image {
    if (nil == image) {
        UnitySendMessage("MixShare", "shareFacebookAsyncFail", "{\"code\":-1, \"msg\":\"image must be not null\"}");
        return;
    }
    UIViewController * viewController = [self getCurrentVC];
    FBSDKSharePhotoContent * content = [[FBSDKSharePhotoContent alloc] init];
    FBSDKSharePhoto * photo = [[FBSDKSharePhoto alloc] initWithImage:image isUserGenerated:YES];
    NSMutableArray * photos = [NSMutableArray array];
    [photos addObject:photo];
    content.photos = photos;
    FBSDKShareDialog * dialog = [FBSDKShareDialog dialogWithViewController:viewController withContent:content delegate:self];
    [dialog show];
}

//获取当前屏幕显示的viewcontroller
- (UIViewController *)getCurrentVC
{
   ///下文中有分析
    UIViewController *rootViewController = [UIApplication sharedApplication].keyWindow.rootViewController;
    UIViewController *currentVC = [self getCurrentVCFrom:rootViewController];
    return currentVC;
}

- (UIViewController *)getCurrentVCFrom:(UIViewController *)rootVC
{
    UIViewController *currentVC;
    if ([rootVC presentedViewController]) {
        // 视图是被presented出来的
        rootVC = [rootVC presentedViewController];
    }

    if ([rootVC isKindOfClass:[UITabBarController class]]) {
        // 根视图为UITabBarController
        currentVC = [self getCurrentVCFrom:[(UITabBarController *)rootVC selectedViewController]];
    } else if ([rootVC isKindOfClass:[UINavigationController class]]){
        // 根视图为UINavigationController
        currentVC = [self getCurrentVCFrom:[(UINavigationController *)rootVC visibleViewController]];
    } else {
        // 根视图为非导航类
        currentVC = rootVC;
    }
    
    return currentVC;
}

#pragma mark - FBSDKSharingDelegate
- (void)sharer:(id<FBSDKSharing> _Nonnull)sharer didCompleteWithResults:(NSDictionary<NSString *,id> * _Nonnull)results {
    // NSLog(@"MIXSDK[OC]-> didCompleteWithResults results:%@;", results);
    FBSDKShareDialog * dialog = (FBSDKShareDialog *)sharer;
    if (dialog.mode == FBSDKShareDialogModeBrowser) {
        NSString * postId = results[@"postId"];
        if (nil == postId || [postId isEqualToString:@""]) {
            // 如果使用webview分享的，但postId是空的，
            // 这种情况是用户点击了『完成』按钮，并没有真的分享
            UnitySendMessage("MixShare", "shareFacebookAsyncFail", "{\"code\":-1, \"msg\":\"cancel\"}");
            return;
        }
    }
    UnitySendMessage("MixShare", "shareFacebookAsyncSuccess", "success");
}

- (void)sharer:(id<FBSDKSharing> _Nonnull)sharer didFailWithError:(NSError * _Nonnull)error {
    // NSLog(@"MIXSDK[OC]-> didFailWithError error:%@;", error);
    NSString * jsonString = @"{\"code\":-3, \"msg\":\"unknown\"}";
    if (nil != error) {
        jsonString = [NSString stringWithFormat:@"{\"code\":-2, \"msg\":\"%@\"}", error.domain.description];
    }
    UnitySendMessage("MixShare", "shareFacebookAsyncFail", [jsonString UTF8String]);
}

- (void)sharerDidCancel:(id<FBSDKSharing> _Nonnull)sharer {
    // NSLog(@"MIXSDK[OC]-> sharerDidCancel sharer:%@;", sharer);
    UnitySendMessage("MixShare", "shareFacebookAsyncFail", "{\"code\":-1, \"msg\":\"cancel\"}");
}
@end
