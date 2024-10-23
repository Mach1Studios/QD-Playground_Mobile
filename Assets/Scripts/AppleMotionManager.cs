#if UNITY_IOS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HearXR
{
    public class AppleMotionManager : MonoBehaviour
    {

        #region Private Fields
        private AudioListener audiolistener;
        private bool _motionAvailable;
        private bool _tracking;
        private bool _headphonesConnected;
        private Quaternion _lastRotation = Quaternion.identity;
        private Quaternion _calibratedOffset = Quaternion.identity;
        private Vector3 _deltaAngles;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            attachAudioListener();

            // Init HeadphoneMotion. Always call this first.
            HeadphoneMotion.Init();

            // Check if headphone motion is available on this device.
            _motionAvailable = HeadphoneMotion.IsHeadphoneMotionAvailable();
            if (_motionAvailable)
            {
                Debug.Log("Mach1: Headphone motion is available");
            }
            else
            {
                Debug.Log("Mach1: Headphone motion is not available");
            }

            if (_motionAvailable)
            {              
                // Set headphones connected text to false to start with.
                HandleHeadphoneConnectionChange(false);

                // Subscribe to events before starting tracking, or will miss the initial headphones connected callback.
                // Subscribe to the headphones connected/disconnected event.
                HeadphoneMotion.OnHeadphoneConnectionChanged += HandleHeadphoneConnectionChange;
                
                // Subscribe to the rotation callback.
                HeadphoneMotion.OnHeadRotationQuaternion += HandleHeadRotationQuaternion;
                
                // Start tracking headphone motion.
                HeadphoneMotion.StartTracking();
                _tracking = true;
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
            } else {
            }
        }

        public void attachAudioListener() 
        {
            AudioListener[] myListeners = FindObjectsOfType(typeof(AudioListener)) as AudioListener[];
            int totalListeners = 0;//find out how many listeners are actually active
            foreach(AudioListener thisListener in myListeners){
                if(thisListener.enabled){totalListeners ++;}
            }
            audiolistener = GameObject.FindObjectOfType<AudioListener>();
            Debug.Log("Mach1: AudioListener Attached, number detected: " + totalListeners);
        }

        #region Event Handlers
        /// <summary>
        /// Headphone connection status was changed (callback for OnHeadphoneConnectionChanged()).
        /// </summary>
        /// <param name="connected">TRUE if connected, FALSE otherwise.</param>
        private void HandleHeadphoneConnectionChange(bool connected)
        {
            _headphonesConnected = connected;
            // Check if headphone motion is available on this device.
            _motionAvailable = HeadphoneMotion.IsHeadphoneMotionAvailable();
            if (_motionAvailable)
            {
                Debug.Log("Mach1: Headphone motion is available");
            }
            else
            {
                Debug.Log("Mach1: Headphone motion is not available");
            }
        }

        /// <summary>
        /// Receive headphone as quaternion (callback for OnHeadRotationQuaternion()).
        /// </summary>
        /// <param name="rotation">Headphone rotation</param>
        private void HandleHeadRotationQuaternion(Quaternion rotation)
        {
            if (_motionAvailable) {
                this.transform.parent.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
            } else {
                this.transform.parent.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }

            // Get the delta of each independent angle
            _deltaAngles.x = rotation.eulerAngles.x - _lastRotation.eulerAngles.x;
            _deltaAngles.y = rotation.eulerAngles.y - _lastRotation.eulerAngles.y;
            _deltaAngles.z = rotation.eulerAngles.z - _lastRotation.eulerAngles.z;
            //Debug.Log("Mach1: 0. DeltaAngles: " + _deltaAngles);

            //Debug.Log("Mach1: 1. AudioListener: " + audiolistener.transform.eulerAngles);
            //Debug.Log("Mach1: 2. MotionRotation: " + rotation.eulerAngles);

            Vector3 oldRotation;
            oldRotation.x = transform.rotation.eulerAngles.x;
            oldRotation.y = transform.rotation.eulerAngles.y;
            oldRotation.z = transform.rotation.eulerAngles.z;
            Vector3 newRotation;
            newRotation.x = oldRotation.x + -_deltaAngles.x; // requires (*-1 flip)
            newRotation.y = oldRotation.y + _deltaAngles.y;  // requires (-90.0 offset)
            newRotation.z = oldRotation.z + 0;

            Quaternion newRotQuat = Quaternion.identity;
            newRotQuat.eulerAngles = newRotation;
            //transform.rotation.SetLookRotation(newRotation, Vector3.up);
            transform.rotation = newRotQuat;
            //Debug.Log("Mach1: 3. NewRotation: " + audiolistener.transform.eulerAngles);

            // set current rotation as last rotation for next update
            _lastRotation = rotation;
        }
        #endregion

        private void CalibrateStartingRotation()
        {
            _calibratedOffset = _lastRotation;
        }

        private void ResetCalibration()
        {
            _calibratedOffset = Quaternion.identity;
        }
    }
}
#endif