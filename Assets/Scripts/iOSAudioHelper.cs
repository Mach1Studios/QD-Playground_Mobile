using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// Helper class to configure iOS audio session for proper playback.
/// On iOS, audio won't play when the device is in silent mode unless
/// the audio session is configured for Playback category.
/// </summary>
public static class iOSAudioHelper
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _InitializeAudioSession();
    
    [DllImport("__Internal")]
    private static extern string _GetAudioOutputRoute();
    
    [DllImport("__Internal")]
    private static extern string _GetAudioSessionInfo();
#endif
    
    /// <summary>
    /// Initialize iOS audio session for playback.
    /// Call this early (e.g., in Awake) to ensure audio works.
    /// </summary>
    public static void Initialize()
    {
#if UNITY_IOS && !UNITY_EDITOR
        Debug.Log("[iOSAudioHelper] Initializing iOS audio session...");
        _InitializeAudioSession();
#else
        Debug.Log("[iOSAudioHelper] Not on iOS, skipping audio session init");
#endif
    }
    
    /// <summary>
    /// Get info about where audio is being routed (speakers, headphones, etc.)
    /// </summary>
    public static string GetAudioRoute()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return _GetAudioOutputRoute();
#else
        return "Editor/Non-iOS";
#endif
    }
    
    /// <summary>
    /// Get iOS audio session configuration info for debugging.
    /// </summary>
    public static string GetSessionInfo()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return _GetAudioSessionInfo();
#else
        return "Editor/Non-iOS - N/A";
#endif
    }
}

