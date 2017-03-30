using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace KingsTaxes
{

    /// <summary>
    /// Container for a button that allows the button to be toglled on/of
    /// </summary>
    class DisableButtonContainer : MonoBehaviour {
        private GameObject m_child;
        private Text m_text;

        private void Awake()
        {
            m_child = transform.FindChild("Button").gameObject; //Hackily get child
            m_text = m_child.transform.FindChild("Text").gameObject.GetComponent<Text>();
        }

        /// <summary>
        /// Enables the contained button
        /// </summary>
        internal void Enable()
        {
            m_child.SetActive(true);
        }

        /// <summary>
        /// Disables the contained button
        /// </summary>
        internal void Disable()
        {
            m_child.SetActive(false);
        }

        internal void setText(string text)
        {
            m_text.text = text;
        }
    }
}
