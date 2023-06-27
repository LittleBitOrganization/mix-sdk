#ifndef MixHelper_h
#define MixHelper_h
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@interface MixHelper:NSObject
+(instancetype)sharedInstance;
- (void) addToCart:(float)price currency:(NSString*) currency contentId:(NSString*)  contentId;
- (void) initCheckout:(float)price currency:(NSString*) currency contentId:(NSString*)  contentId;
- (void) purchase:(float)price currency:(NSString*) currency contentId:(NSString*)  contentId;
- (void) incRevenue:(float)revenue;
- (void) shareLinkByFacebook:(NSString * _Nullable) url;
- (void) sharePhotoByFacebook:(UIImage * _Nullable) image;
@end

#endif /* MixHelper_h */
