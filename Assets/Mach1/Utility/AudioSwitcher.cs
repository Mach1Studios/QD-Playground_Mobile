#define APPLE_PHASE_AVAILABLE
// edit out the above line if you don't have the Apple PHASE plugin installed

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

#if APPLE_PHASE_AVAILABLE
using Apple.PHASE;
using PolySpatial.Samples;
#endif

using TMPro;

public class AudioSwitcher : MonoBehaviour
{
    // Assign your GameObjects in the Unity Inspector
    public GameObject[] switchSources;

    // Assign your keys in the Unity Inspector
    public KeyCode[] keys;

    // Sequence mode variables
    public bool useSequenceMode = false;
    public KeyCode nextKey = KeyCode.Space; // Key to cycle through audio sources
    private int currentIndex = 0;

    // For displaying text on the screen
    private string displayText = "";

    [SerializeField] private GameObject positionReference;
    [SerializeField] private SpatialUISlider spatialBlendSlider;
    [SerializeField] private SpatialUIButton nextButton;
    [SerializeField] private SpatialUIButton playButton;
    private bool isPlaying = false;
    private bool paused = false;
    [SerializeField] private TextMeshPro label;

    void Start()
    {
        // Initialize volumes based on the mode
        InitializeVolumes(currentIndex);
        OutputCurrentAudioSourceInfo();

        // Assign the slider event handler
        if (spatialBlendSlider != null)
        {
            spatialBlendSlider.OnSliderUpdated += OnSliderValueChanged;
        }

        // Assign the button event handler
        if (nextButton != null)
        {
            nextButton.WasPressed += OnNextButtonPressed;
        }

        // Assign the button event handler
        if (playButton != null)
        {
            playButton.WasPressed += OnPlayButtonPressed;
        }
    }

    void OnDestroy()
    {
        // Clean up event handlers to prevent memory leaks
        if (spatialBlendSlider != null)
        {
            spatialBlendSlider.OnSliderUpdated -= OnSliderValueChanged;
        }

        if (nextButton != null)
        {
            nextButton.WasPressed -= OnNextButtonPressed;
        }
    }

    void Update()
    {
        if (positionReference != null)
        {
            gameObject.transform.position = positionReference.transform.position;
        }

        if (useSequenceMode)
        {
            if (Input.GetKeyDown(nextKey))
            {
                NextAudioSource();
            }
        }
        else
        {
            // Ensure the arrays are of the same length
            if (switchSources.Length != keys.Length)
            {
                Debug.LogError("AudioSources and keys arrays must be of the same length.");
                return;
            }

            // Check for key presses
            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.GetKeyDown(keys[i]))
                {
                    SetActiveAudioSource(i);
                }
            }
        }
    }

    // Public method to switch to the next audio source
    public void NextAudioSource()
    {
        currentIndex = (currentIndex + 1) % switchSources.Length;
        InitializeVolumes(currentIndex);
        OutputCurrentAudioSourceInfo();
        if (switchSources[currentIndex] != null)
        {
            GameObject obj = switchSources[currentIndex];
            Debug.Log("Component: " + obj.GetComponents<Component>()[0].GetComponents<Component>()[0].ToString());
            AudioSource audioSource = obj.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                spatialBlendSlider.SetPercentage(audioSource.spatialBlend);
                Debug.Log("Spatial blend set as: " + audioSource.spatialBlend);
            }
        }
    }

    // Public method to set a specific audio source active by index
    public void SetActiveAudioSource(int index)
    {
        if (index >= 0 && index < switchSources.Length)
        {
            currentIndex = index;
            InitializeVolumes(currentIndex);
            OutputCurrentAudioSourceInfo();
        }
        else
        {
            Debug.LogError("Index out of range when setting active audio source.");
        }
    }

    void InitializeVolumes(int activeIndex)
    {
        for (int i = 0; i < switchSources.Length; i++)
        {
            if (switchSources[i] != null)
            {
                GameObject obj = switchSources[i];

                // Try to get M1SpatialDecode component
                M1SpatialDecode m1SpatialDecode = obj.GetComponent<M1SpatialDecode>();
                if (m1SpatialDecode != null)
                {
                    float volume = (i == activeIndex) ? 1.0f : 0.0f;
                    m1SpatialDecode.setoutputGainMultiplier(volume);
                }

                // Try to get StSP_Player component
                StSP_Player stSP_Player = obj.GetComponent<StSP_Player>();
                if (stSP_Player != null)
                {
                    float volume = (i == activeIndex) ? 1.0f : 0.0f;
                    stSP_Player.outputGain = volume;
                }

#if APPLE_PHASE_AVAILABLE
                // Try to get PHASESource component
                PHASESource pHASESource = obj.GetComponent<PHASESource>();
                if (pHASESource != null)
                {
                    float volume = (i == activeIndex) ? 1.0f : 0.0f;
                    pHASESource.SetGain(volume);
                }
#endif

                // Try to get AudioSource component
                AudioSource audioSource = obj.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.volume = (i == activeIndex) ? 1.0f : 0.0f;
                }
                // Add additional component checks as needed
            }
        }
    }

    void OutputCurrentAudioSourceInfo()
    {
        if (switchSources[currentIndex] != null)
        {
            GameObject obj = switchSources[currentIndex];
            string componentName = "Unknown Component";
            string audioClipName = "No AudioClip assigned";

            // Check for M1SpatialDecode
            if (obj.GetComponent<M1SpatialDecode>() != null)
            {
                componentName = "M1SpatialDecode";
                // Retrieve audio clip name if possible
            }
            // Check for StSP_Player
            else if (obj.GetComponent<StSP_Player>() != null)
            {
                componentName = "StSP_Player";
                audioClipName = obj.GetComponent<StSP_Player>().stereoStaticClip.name;
            }
#if APPLE_PHASE_AVAILABLE
            // Check for PHASESource
            else if (obj.GetComponent<PHASESource>() != null)
            {
                componentName = "PHASESource";
                // PHASESource pHASESource = obj.GetComponent<PHASESource>();
                // if (pHASESource.clip != null)
                //     audioClipName = pHASESource.clip.name;
                audioClipName = "PHASESoundEvent";
            }
#endif
            else if (obj.GetComponent<AudioSource>() != null)
            {
                componentName = "AudioSource";
                // Check for AudioSource
                AudioSource audioSource = obj.GetComponent<AudioSource>();
                if (audioSource.clip != null)
                    audioClipName = audioSource.clip.name;
            }
            displayText = $"Active Index: {currentIndex}\nComponent: {componentName}\nAudioClip: {audioClipName}";
            Debug.Log(displayText);
        }
        else
        {
            displayText = $"Active Index: {currentIndex}\nGameObject is null.";
            Debug.LogWarning(displayText);
        }
        if (label != null)
        {
            label.text = displayText;
        }
    }

    void OnGUI()
    {
        // Display the text on the screen
        GUI.Label(new Rect(10, 10, Screen.width - 20, 50), displayText);
    }

    // Event handler for the button press
    private void OnNextButtonPressed(string buttonText, MeshRenderer meshRenderer)
    {
        NextAudioSource();
    }

    public void togglePlayPause()
    {
        isPlaying = !isPlaying;
        if (isPlaying)
        {
            playButton.GetComponentInChildren<TextMeshPro>().text = "Pause";
            //playButton.SetText("Stop");
        }
        else
        {
            playButton.GetComponentInChildren<TextMeshPro>().text = "Play";
            //playButton.SetText("Play");
        }

        if (isPlaying)
        {
            for (int i = 0; i < switchSources.Length; i++)
            {
                if (switchSources[i] != null)
                {
                    GameObject obj = switchSources[i];

                    // Try to get relevant component
                    M1SpatialDecode m1SpatialDecode = obj.GetComponent<M1SpatialDecode>();
                    StSP_Player stSP_Player = obj.GetComponent<StSP_Player>();
                    PHASESource pHASESource = obj.GetComponent<PHASESource>();
                    if (m1SpatialDecode != null)
                    {
                        if (paused) 
                        {
                            m1SpatialDecode.ResumeAudio();
                        } else 
                        {
                            m1SpatialDecode.PlayAudio();
                        }
                    }
                    else if (stSP_Player != null)
                    {
                        if (paused)
                        {
                            stSP_Player.UnPause();
                        }
                        else
                        {
                            stSP_Player.Play();
                        }
                    }
#if APPLE_PHASE_AVAILABLE
                    else if (pHASESource != null)
                    {
                        pHASESource.Play();
                    }
#endif
                    else
                    {
                        AudioSource audioSource = obj.GetComponent<AudioSource>();
                        if (audioSource != null)
                        {
                            if (paused)
                            {
                                audioSource.UnPause();
                            }
                            else
                            {
                                audioSource.Play();
                            }
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < switchSources.Length; i++)
            {
                if (switchSources[i] != null)
                {
                    GameObject obj = switchSources[i];

                    // Try to get relevant component
                    M1SpatialDecode m1SpatialDecode = obj.GetComponent<M1SpatialDecode>();
                    StSP_Player stSP_Player = obj.GetComponent<StSP_Player>();
                    PHASESource pHASESource = obj.GetComponent<PHASESource>();

                    if (m1SpatialDecode != null)
                    {
                        m1SpatialDecode.PauseAudio();
                    }
                    else if (stSP_Player != null)
                    {
                        stSP_Player.Pause();
                    }
#if APPLE_PHASE_AVAILABLE
                    else if (pHASESource != null)
                    {
                        pHASESource.Stop();
                    }
#endif
                    else
                    {
                        AudioSource audioSource = obj.GetComponent<AudioSource>();
                        if (audioSource != null)
                        {
                            audioSource.Pause();
                        }
                    }
                }
            }
            paused = true; // we dont flip back because thats handled by isPlaying
        }
    }

    private void OnPlayButtonPressed(string buttonText, MeshRenderer meshRenderer)
    {
        togglePlayPause();
    }

    public void OnSliderValueChanged(float newValue)
    {
        if (switchSources[currentIndex] != null)
        {
            GameObject obj = switchSources[currentIndex];
            AudioSource audioSource = obj.GetComponent<AudioSource>();
            //StSP_Player stSP_Player = obj.GetComponent<StSP_Player>();
            if (audioSource != null)
            {
                audioSource.spatialBlend = newValue;
            } 
            // else if (StSP_Player != null) 
            // {
            //     stSP_Player.stere_spatialBlend = newValue;
            // }
        }
    }
}
