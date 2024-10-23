using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

public class M1MonoHRTF : MonoBehaviour
{
    public GameObject monoAudioSource;
    public bool applyDistanceAttenuation;
    public bool debug;

    // [RangeAttribute(0,22000)]
	// public float lowFrequencyCutoff = 22000.0f;
	
	[RangeAttribute(10.0f,22000.0f)]
	public float highFrequencyCutoff = 0.0f;
    
    [RangeAttribute(10.0f,22000.0f)]
	public float highFrequencyCutoffMax = 1000.0f;

    [RangeAttribute(1,10)]
    public float highFrequencyQ = 1.0f;

    private AudioListener audiolistener;
    private AudioSource audiosource;
    private Vector3 heading;
    private float distance;
    private AudioHighPassFilter highPassFilter;
	private AudioLowPassFilter lowPassFilter;    

    // Start is called before the first frame update
    void Start()
    {
        attachAudioListener();
        audiosource = monoAudioSource.GetComponent<AudioSource>();
        highPassFilter = monoAudioSource.AddComponent<AudioHighPassFilter>();
		//lowPassFilter = monoAudioSource.AddComponent<AudioLowPassFilter>();
        highPassFilter.cutoffFrequency = highFrequencyCutoff;
		// lowPassFilter.cutoffFrequency = lowFrequencyCutoff;
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

        // results in difference of angle looking away from target object (0 -> 180 degrees)
        float angle = Vector3.Angle(audiolistener.transform.forward, audiosource.transform.position - audiolistener.transform.position);

        float coeff = Mathf.InverseLerp(0.0f, 180.0f, angle);
        
        highFrequencyCutoff = (int)Mathf.Abs(highFrequencyCutoffMax * coeff);
        highPassFilter.cutoffFrequency = highFrequencyCutoff;

        
        float[] samples = new float [audiosource.clip.samples * audiosource.clip.channels];
        audiosource.clip.GetData(samples, 0);

        if (debug) {
            Debug.Log("Mach1: HRTF: Angle   : " + angle);
            Debug.Log("Mach1: HTRF: Coeff   : " + coeff);
            Debug.Log("Mach1: HRTF: HPF     : " + highFrequencyCutoff);
        }
    }

    public void attachAudioListener() 
    {
        audiolistener = GameObject.FindObjectOfType<AudioListener>();
    }

    public static float InverseLerp(Vector3 start, Vector3 end, Vector3 currentValue)
    {
        Vector3 AB = end - start;
        Vector3 AV = currentValue - start;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }
}
