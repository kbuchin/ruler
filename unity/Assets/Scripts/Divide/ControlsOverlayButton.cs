using UnityEngine;
using System.Collections;
namespace Divide { 
    public class ControlsOverlayButton : MonoBehaviour {
        private ControlsOverlay m_controlsoverlay;

        // Use this for initialization
        void Start () {
            m_controlsoverlay = GameObject.FindObjectOfType<ControlsOverlay>();
	    }

        public void ActivateControlsOverlay()
        {
            m_controlsoverlay.Activate();
        }
    }
}