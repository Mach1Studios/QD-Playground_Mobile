using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class SongMaster : MonoBehaviour
{
    public PlayableDirector timeline;
    public M1SpatialDecode M1SpatialDecodeScript;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Pause()
    {
        timeline.Pause();
        PauseMusic();
    }

    public void PauseMusic()
    {
        M1SpatialDecodeScript.PauseAudio();
    }

    public void ResumeMusic()
    {
        M1SpatialDecodeScript.ResumeAudio();
    }

    public void Play()
    {
        timeline.Play();
        ResumeMusic();
    }






}
