using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AudioTester : MonoBehaviour
{
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
    private bool useSpatialPlugin = true;  // Start with plugin enabled

    void Start()
    {
        // Load the first clip if available
        if (clips.Count > 0 && targetSource != null)
        {
            targetSource.clip = clips[currentClipIndex];
        }
        
        // Initialize spatial audio state - avoid conflict between Unity spatialize and plugin
        if (spatialAudioComponent != null)
        {
            useSpatialPlugin = spatialAudioComponent.enabled;
            // When SteamAudio plugin is enabled, disable Unity's built-in spatialize to avoid conflict
            targetSource.spatialize = !useSpatialPlugin;
        }
        else
        {
            // No plugin - use Unity's spatialize
            targetSource.spatialize = true;
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
        
        UpdateUI();
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
    /// Use this to verify audio output works.
    /// </summary>
    public void TestPlayDirect()
    {
        if (targetSource == null || clips.Count == 0) return;
        
        // Temporarily disable all spatial processing
        bool wasSpatialize = targetSource.spatialize;
        bool wasPluginEnabled = spatialAudioComponent != null && spatialAudioComponent.enabled;
        float wasSpatialBlend = targetSource.spatialBlend;
        
        targetSource.spatialize = false;
        targetSource.spatialBlend = 0f; // 0 = 2D audio, 1 = 3D audio
        if (spatialAudioComponent != null) spatialAudioComponent.enabled = false;
        
        if (targetSource.clip == null)
            targetSource.clip = clips[0];
            
        targetSource.Play();
        Debug.Log("[AudioTester] TEST: Playing in 2D mode (no spatialization)");
        
        // Restore after a short delay would require coroutine, so just leave it for testing
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
    /// </summary>
    public void ToggleSpatialize()
    {
        if (targetSource == null) return;
        
        targetSource.spatialize = !targetSource.spatialize;
        
        // Update the indicator visibility
        if (spatializeIndicator != null)
        {
            spatializeIndicator.SetActive(targetSource.spatialize);
        }
        
        Debug.Log($"[AudioTester] Spatialize: {targetSource.spatialize}");
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
            string spatState = targetSource.spatialize ? "STEAM" : "NATIVE";
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
}