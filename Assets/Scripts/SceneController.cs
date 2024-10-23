// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class SceneController : MonoBehaviour
// {

//     public List<Light> SceneLights = new List<Light>();
//     public List<Light> SceneLightsToTurnOn = new List<Light>();
//     public List<SkinnedMeshRenderer> HologramMats = new List<SkinnedMeshRenderer>();

//     public float TimeToFadeOut = 3f;
//     public float TimeToFadeIn = 6f;
//     public bool FadeOut;
//     public bool FadeIn;

//     float timeElapsed;
//     float timeElapsedLihtsOn;


//     float startValue = 0;
//     float endValue = 0;
//     float valueToLerp;

//     List<float> OriginalLightIntensity = new List<float>();
//     List<float> OriginalLightIntensityPart2 = new List<float>();

//     private void Awake()
//     {

//     }


//     void Start()
//     {
//         Invoke("SetsLightON", 37.55f);
//         foreach (var light in SceneLights)
//         {
//             OriginalLightIntensity.Add(light.intensity);
//         }

//         foreach (var lightOn in SceneLightsToTurnOn)
//         {
//             OriginalLightIntensityPart2.Add(lightOn.intensity);
//             lightOn.intensity = 0;
//         }
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         if (FadeIn)
//         {
//             FadeInLights();
//         }
//         if (FadeOut)
//         {
//             FadeOutLights();
//             //FadeOutHolograms();
//         }


//     }

//     private void FadeInLights()
//     {
//         for (int i = 0; i < SceneLightsToTurnOn.Count; i++)
//         {
//             valueToLerp = Mathf.Lerp(0, OriginalLightIntensityPart2[i], timeElapsedLihtsOn / TimeToFadeIn);
//             timeElapsedLihtsOn += Time.deltaTime;
//             SceneLightsToTurnOn[i].intensity = valueToLerp;
//         }
//     }

//     public void FadeOutLights()
//     {
//         for (int i = 0; i < SceneLights.Count; i++)
//         {
//             valueToLerp = Mathf.Lerp(OriginalLightIntensity[i], endValue, timeElapsed / TimeToFadeOut);
//             timeElapsed += Time.deltaTime;
//             SceneLights[i].intensity = valueToLerp;
//         }
//     }

//     public void FadeOutHolograms()
//     {       
//         for (int i = 0; i < HologramMats.Count; i++)
//         {           

//             valueToLerp = Mathf.Lerp(1, endValue, timeElapsed / TimeToFadeIn);
//             timeElapsed += Time.deltaTime;
//             SceneLights[i].intensity = valueToLerp;
//             Material mat = HologramMats[i].material;           
//             mat.SetFloat("_HologramHeightOffset", 5);


//         }
//     }

//     public void SetsLightsOff()
//     {
//         FadeIn = false;
//         FadeOut = true;
//     }

//     public void SetsLightON()
//     {
//         FadeOut = false;
//         FadeIn = true;
//     }

//     public void FadeOutCam()
//     {
//         Animator camFader = Camera.main.GetComponent<CamFader>().FadeAnimator;
//         Debug.Log("CAM NAME: " + camFader.name);
//         camFader.Play("CamFadeOut");
//     }

//     public void FadeInCam()
//     {
//         Animator camFader = Camera.main.GetComponent<CamFader>().FadeAnimator;
//         camFader.Play("CamFadeIn");
//     }

//     public void ResetFader()
//     {
//         Camera.main.GetComponent<CamFader>().ResetFader();
       
//     }

//     public void ShowSolidBackground()
//     {
        
//     }
// }
