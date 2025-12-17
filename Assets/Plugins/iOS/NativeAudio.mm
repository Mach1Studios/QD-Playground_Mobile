//
// iOS Audio Session Configuration
// Ensures audio plays even when the device is in silent mode
//

#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>

extern "C" {
    
    // Initialize iOS audio session for playback
    void _InitializeAudioSession() {
        NSError *error = nil;
        AVAudioSession *session = [AVAudioSession sharedInstance];
        
        // Set category to Playback - this allows audio to play even in silent mode
        [session setCategory:AVAudioSessionCategoryPlayback
                        mode:AVAudioSessionModeDefault
                     options:AVAudioSessionCategoryOptionMixWithOthers
                       error:&error];
        
        if (error) {
            NSLog(@"[NativeAudio] Error setting audio session category: %@", error);
        } else {
            NSLog(@"[NativeAudio] Audio session category set to Playback");
        }
        
        // Activate the session
        [session setActive:YES error:&error];
        
        if (error) {
            NSLog(@"[NativeAudio] Error activating audio session: %@", error);
        } else {
            NSLog(@"[NativeAudio] Audio session activated successfully");
        }
    }
    
    // Check if audio is currently routed to speakers/headphones
    const char* _GetAudioOutputRoute() {
        AVAudioSession *session = [AVAudioSession sharedInstance];
        AVAudioSessionRouteDescription *route = session.currentRoute;
        
        NSMutableString *routes = [NSMutableString string];
        for (AVAudioSessionPortDescription *port in route.outputs) {
            if (routes.length > 0) [routes appendString:@", "];
            [routes appendString:port.portType];
        }
        
        const char *cString = [routes UTF8String];
        char *result = (char*)malloc(strlen(cString) + 1);
        strcpy(result, cString);
        return result;
    }
    
    // Get current audio session category
    const char* _GetAudioSessionInfo() {
        AVAudioSession *session = [AVAudioSession sharedInstance];
        
        NSString *info = [NSString stringWithFormat:@"Category: %@\nMode: %@\nSampleRate: %.0f\nOutputChannels: %ld",
                          session.category,
                          session.mode,
                          session.sampleRate,
                          (long)session.outputNumberOfChannels];
        
        const char *cString = [info UTF8String];
        char *result = (char*)malloc(strlen(cString) + 1);
        strcpy(result, cString);
        return result;
    }
}

