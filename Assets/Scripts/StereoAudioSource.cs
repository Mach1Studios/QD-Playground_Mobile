using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using System.IO;

public class StereoAudioSource : MonoBehaviour
{
    private AudioListener audiolistener;
    public AudioSource stereoAudioSource;
    private Vector3 heading;
    private float distance;
    public AnimationCurve curve;

    // Start is called before the first frame update
    void Start()
    {
        attachAudioListener();

        // if there is no set curve grab the one from the audiosource
        if (curve == null)
        {
            curve = stereoAudioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (audiolistener == null)
        {
            Debug.LogError("Mach1: cannot find AudioListener!");
            attachAudioListener();
            return;
        }

        heading = stereoAudioSource.transform.position - audiolistener.transform.position;
        distance = Vector3.Dot(heading, audiolistener.transform.forward);
        //Debug.Log("Mach1: ST: Distance: " + distance);

        //Debug.Log("Mach1: ST: DistanceCurveReturn: " + curve.Evaluate(distance));
        //Debug.Log("Mach1: ST: DistanceCurveReturn: " + stereoAudioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff).Evaluate(distance));
        stereoAudioSource.volume = curve.Evaluate(distance);
    }

    public void attachAudioListener() 
    {
        audiolistener = GameObject.FindObjectOfType<AudioListener>();
    }
}
