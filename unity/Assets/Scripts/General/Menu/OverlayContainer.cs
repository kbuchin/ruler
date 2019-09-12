namespace General.Menu
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    class OverlayContainer : MonoBehaviour
    {
        [SerializeField]
        private MenuOverlay m_overlay;

        public void Activate()
        {
            m_overlay.Activate();
        }
    }
}
