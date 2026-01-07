using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AudioTester : MonoBehaviour
{
    // Diagnostic info for iOS debugging
    private string diagnosticInfo = "";
    [Header("References")]
    public AudioSource targetSource;       // The object making sound
    public Transform listenerTransform;    // Your head (Main Camera)
    public Text statusText;                // UI Text to show info

    [Header("Spatial Audio Plugin")]
    [Tooltip("The spatial audio component (SteamAudioSource, ResonanceAudioSource, MetaXRAudioSource, etc.)")]
    public MonoBehaviour spatialAudioComponent;  // Drag the plugin's audio source component here
    public Text modeButtonText;            // Text on the toggle button to show current mode
    public GameObject spatializeIndicator; // Logo/image to show when spatialize is ON

    [Header("Audio Clips")]
    public List<AudioClip> clips;          // Drag mono/stereo files here

    [Header("Settings")]
    public float distance = 2.0f;          // How far away the sound is
    public float rotationSpeed = 45.0f;    // Speed for "Rotate" mode

    private int currentClipIndex = 0;
    private bool isRotating = false;
    private bool useSpatialPlugin = false;  // Start with plugin enabled

    void Awake()
    {
        // Initialize iOS audio session FIRST - this is critical for iOS audio to work
        iOSAudioHelper.Initialize();
    }
    
    void Start()
    {
        // Collect diagnostic info for iOS debugging
        CollectDiagnostics();
        
        // Load the first clip if available
        if (clips.Count > 0 && targetSource != null)
        {
            targetSource.clip = clips[currentClipIndex];
        }
        
        // Initialize spatial audio state based on which plugin is being used
        string spatializerPlugin = AudioSettings.GetSpatializerPluginName();
        bool isResonanceAudio = spatializerPlugin == "Resonance Audio";
        
        if (spatialAudioComponent != null)
        {
            useSpatialPlugin = spatialAudioComponent.enabled;
            
            // Key difference between spatial audio plugins:
            // - Resonance Audio: Uses Unity's spatializer system, needs spatialize = TRUE
            // - SteamAudio: Bypasses Unity's spatializer, needs spatialize = FALSE
            if (isResonanceAudio)
            {
                // Resonance Audio requires spatialize = true to process audio
                targetSource.spatialize = true;
                Debug.Log("[AudioTester] Resonance Audio detected - spatialize enabled");
            }
            else
            {
                // SteamAudio and others handle spatialization independently
                targetSource.spatialize = !useSpatialPlugin;
                Debug.Log($"[AudioTester] {spatializerPlugin} detected - spatialize={targetSource.spatialize}");
            }
        }
        else
        {
            // No plugin component - use Unity's spatialize if a spatializer is set
            targetSource.spatialize = !string.IsNullOrEmpty(spatializerPlugin);
        }
        
        // Initial Setup: Move sound to front (0 degrees)
        SetAngle(0);
        
        // Sync the spatialize indicator with initial state
        if (spatializeIndicator != null)
        {
            spatializeIndicator.SetActive(targetSource.spatialize);
        }
        
        // Force play the audio
        if (targetSource != null && targetSource.clip != null)
        {
            targetSource.Play();
            Debug.Log($"[AudioTester] Playing clip: {targetSource.clip.name}, Volume: {targetSource.volume}, Spatialize: {targetSource.spatialize}, Plugin enabled: {useSpatialPlugin}");
        }
        
        // Log spatializer info on startup (visible in Xcode console)
        LogSpatializerInfo();
        
        UpdateUI();
    }
    
    void CollectDiagnostics()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        sb.AppendLine($"Platform: {Application.platform}");
        
        // Audio configuration
        var config = AudioSettings.GetConfiguration();
        sb.AppendLine($"Sample: {config.sampleRate}Hz");
        sb.AppendLine($"Speaker: {config.speakerMode}");
        
        // Check AudioListener
        var listener = FindObjectOfType<AudioListener>();
        sb.AppendLine($"Listener: {(listener != null ? (listener.enabled ? "OK" : "DISABLED") : "MISSING")}");
        sb.AppendLine($"ListenerVol: {AudioListener.volume}");
        
        // Check clip loading
        if (clips.Count > 0 && clips[0] != null)
        {
            sb.AppendLine($"Clip: {clips[0].loadState}");
        }
        else
        {
            sb.AppendLine("Clip: NONE");
        }
        
        // iOS-specific info
        sb.AppendLine($"Route: {iOSAudioHelper.GetAudioRoute()}");
        
        diagnosticInfo = sb.ToString();
        Debug.Log($"[AudioTester] Diagnostics:\n{diagnosticInfo}");
    }

    private bool lastPlayingState = false;
    
    void Update()
    {
        if (isRotating)
        {
            // Orbit around the listener
            targetSource.transform.RotateAround(listenerTransform.position, Vector3.up, rotationSpeed * Time.deltaTime);
            // Always face the listener (important for emission patterns)
            targetSource.transform.LookAt(listenerTransform);
        }
        
        // Update UI when play state changes
        if (targetSource != null && targetSource.isPlaying != lastPlayingState)
        {
            lastPlayingState = targetSource.isPlaying;
            UpdateUI();
        }
    }

    // --- BUTTON FUNCTIONS ---

    public void SetAngle(float angleDegrees)
    {
        isRotating = false; // Stop spinning if we click a specific angle

        // Math to place object on a circle at specific degree
        // 0 degrees = Forward (Z+), 90 degrees = Right (X+)
        float radians = angleDegrees * Mathf.Deg2Rad;
        float x = Mathf.Sin(radians) * distance;
        float z = Mathf.Cos(radians) * distance;

        targetSource.transform.position = listenerTransform.position + new Vector3(x, 0, z);
        targetSource.transform.LookAt(listenerTransform);
        
        // Ensure sound is playing if it was stopped
        if (!targetSource.isPlaying) targetSource.Play();
    }

    public void ToggleRotate()
    {
        isRotating = !isRotating;
    }

    public void TogglePlayStop()
    {
        if (targetSource.isPlaying)
        {
            targetSource.Stop();
            Debug.Log("[AudioTester] Stopped audio");
        }
        else
        {
            // Ensure we have a clip
            if (targetSource.clip == null && clips.Count > 0)
            {
                targetSource.clip = clips[currentClipIndex];
            }
            
            targetSource.Play();
            Debug.Log($"[AudioTester] Playing: clip={targetSource.clip?.name}, volume={targetSource.volume}, mute={targetSource.mute}, spatialize={targetSource.spatialize}");
        }
        UpdateUI();
    }
    
    /// <summary>
    /// Test function to play audio with NO spatial processing at all.
    /// Use this to verify audio output works on iOS.
    /// </summary>
    public void TestPlayDirect()
    {
        if (targetSource == null || clips.Count == 0) return;
        
        // Disable ALL spatial processing for pure 2D test
        targetSource.spatialize = false;
        targetSource.spatialBlend = 0f; // 0 = 2D audio, 1 = 3D audio
        if (spatialAudioComponent != null) spatialAudioComponent.enabled = false;
        
        // Make sure we have a clip and it's loaded
        if (targetSource.clip == null)
            targetSource.clip = clips[0];
        
        // Force volume up
        targetSource.volume = 1f;
        targetSource.mute = false;
        AudioListener.volume = 1f;
        
        // Stop then play to ensure fresh start
        targetSource.Stop();
        targetSource.Play();
        
        string clipState = targetSource.clip != null ? targetSource.clip.loadState.ToString() : "null";
        Debug.Log($"[AudioTester] 2D TEST: clip={targetSource.clip?.name}, loadState={clipState}, isPlaying={targetSource.isPlaying}");
        
        UpdateUI();
    }
    
    /// <summary>
    /// Show diagnostic info - call this to debug iOS audio issues
    /// </summary>
    public void ShowDiagnostics()
    {
        CollectDiagnostics();
        
        // Add iOS session info
        string sessionInfo = iOSAudioHelper.GetSessionInfo();
        diagnosticInfo += $"\n{sessionInfo}";
        
        if (statusText != null)
        {
            statusText.text = diagnosticInfo;
        }
        Debug.Log($"[AudioTester] Full diagnostics:\n{diagnosticInfo}");
    }
    
    /// <summary>
    /// Force reinitialize iOS audio session - useful if audio stops working
    /// </summary>
    public void ReinitAudio()
    {
        iOSAudioHelper.Initialize();
        
        // Also restart playback
        if (targetSource != null && targetSource.clip != null)
        {
            targetSource.Stop();
            targetSource.Play();
        }
        
        UpdateUI();
    }

    public void NextClip()
    {
        if (clips.Count == 0) return;

        currentClipIndex = (currentClipIndex + 1) % clips.Count;
        targetSource.clip = clips[currentClipIndex];
        targetSource.Play();
        UpdateUI();
    }

    /// <summary>
    /// Toggle between Unity's built-in AudioSource spatialization and the scene's spatial audio plugin.
    /// </summary>
    public void ToggleSpatialMode()
    {
        if (spatialAudioComponent == null)
        {
            Debug.LogWarning("No spatial audio component assigned to toggle.");
            return;
        }

        useSpatialPlugin = !useSpatialPlugin;
        spatialAudioComponent.enabled = useSpatialPlugin;
        
        // Unity's Spatialize should be ON when plugin is OFF (for basic 3D audio)
        // When plugin is ON, it handles spatialization
        targetSource.spatialize = !useSpatialPlugin;
        
        UpdateUI();
    }
    
    /// <summary>
    /// Toggle the AudioSource's spatialize property directly.
    /// Shows indicator when spatialize is ON, hides when OFF.
    /// For Resonance Audio: ON = HRTF spatialization, OFF = No spatialization (2D-like)
    /// </summary>
    public void ToggleSpatialize()
    {
        if (targetSource == null) return;
        
        // Remember playback state
        bool wasPlaying = targetSource.isPlaying;
        float playbackTime = targetSource.time;
        
        // Must stop audio before changing spatialize property
        if (wasPlaying) targetSource.Stop();
        
        targetSource.spatialize = !targetSource.spatialize;
        
        // Restart audio if it was playing
        if (wasPlaying)
        {
            targetSource.time = playbackTime;
            targetSource.Play();
        }
        
        // Update the indicator visibility
        if (spatializeIndicator != null)
        {
            spatializeIndicator.SetActive(targetSource.spatialize);
        }
        
        string pluginName = AudioSettings.GetSpatializerPluginName();
        Debug.Log($"[AudioTester] Spatialize: {targetSource.spatialize}, Plugin: {pluginName}");
        
        // Log additional info for debugging
        if (targetSource.spatialize)
        {
            Debug.Log("[AudioTester] HRTF spatialization should now be active");
        }
        else
        {
            Debug.Log("[AudioTester] Spatialization disabled - audio will be non-spatial");
        }
        
        UpdateUI();
    }

    void UpdateUI()
    {
        // Update clip info
        if (statusText && targetSource != null)
        {
            string clipName = targetSource.clip != null ? targetSource.clip.name : "NO CLIP!";
            string type = targetSource.clip != null ? (targetSource.clip.channels == 2 ? "Stereo" : "Mono") : "-";
            string mode = GetCurrentModeName();
            string playState = targetSource.isPlaying ? "PLAYING" : "STOPPED";
            string spatState = GetSpatializerState();
            statusText.text = $"{clipName} ({type})\n{mode} | {playState} | {spatState}";
        }
        
        // Update mode button text
        if (modeButtonText != null)
        {
            modeButtonText.text = useSpatialPlugin ? "Plugin" : "Unity";
        }
        
        // Sync spatialize indicator
        if (spatializeIndicator != null && targetSource != null)
        {
            spatializeIndicator.SetActive(targetSource.spatialize);
        }
    }
    
    /// <summary>
    /// Get the current spatializer state based on the actual plugin loaded
    /// </summary>
    string GetSpatializerState()
    {
        if (!targetSource.spatialize)
            return "Unity3D";
            
        // Get the actual spatializer plugin name from Audio Settings
        string pluginName = AudioSettings.GetSpatializerPluginName();
        
        if (string.IsNullOrEmpty(pluginName))
            return "Unity3D";
        
        // Shorten common plugin names
        if (pluginName.Contains("Resonance"))
            return "Resonance";
        if (pluginName.Contains("Steam"))
            return "Steam";
        if (pluginName.Contains("Meta") || pluginName.Contains("Oculus"))
            return "Meta";
        if (pluginName.Contains("Apple") || pluginName.Contains("PHASE"))
            return "Apple";
            
        return pluginName;
    }

    string GetCurrentModeName()
    {
        if (spatialAudioComponent == null)
            return "Unity Only";
            
        if (useSpatialPlugin)
        {
            // Get the component type name (e.g., "SteamAudioSource", "ResonanceAudioSource")
            string typeName = spatialAudioComponent.GetType().Name;
            // Shorten common names for display
            return typeName.Replace("AudioSource", "").Replace("Source", "");
        }
        
        return "Unity";
    }
    
    /// <summary>
    /// Log detailed spatializer info for debugging (visible in Xcode console)
    /// </summary>
    public void LogSpatializerInfo()
    {
        string pluginName = AudioSettings.GetSpatializerPluginName();
        
        Debug.Log($"[AudioTester] ========== SPATIALIZER INFO ==========");
        Debug.Log($"[AudioTester] Spatializer Plugin: '{pluginName}'");
        Debug.Log($"[AudioTester] AudioSource.spatialize: {targetSource?.spatialize}");
        Debug.Log($"[AudioTester] AudioSource.spatialBlend: {targetSource?.spatialBlend}");
        Debug.Log($"[AudioTester] Plugin Component: {spatialAudioComponent?.GetType().Name ?? "none"}");
        Debug.Log($"[AudioTester] Plugin Enabled: {spatialAudioComponent?.enabled}");
        
        // Check if Resonance Audio is properly set up
        if (pluginName == "Resonance Audio")
        {
            Debug.Log($"[AudioTester] Resonance Audio is the active spatializer");
            
            // Check for ResonanceAudioListener
            var listener = FindObjectOfType<ResonanceAudioListener>();
            Debug.Log($"[AudioTester] ResonanceAudioListener: {(listener != null ? "Found" : "MISSING!")}");
            
            // Check mixer group
            if (targetSource != null)
            {
                Debug.Log($"[AudioTester] Output Mixer Group: {targetSource.outputAudioMixerGroup?.name ?? "NONE (direct)"}");
            }
        }
        
        Debug.Log($"[AudioTester] ======================================");
    }
}