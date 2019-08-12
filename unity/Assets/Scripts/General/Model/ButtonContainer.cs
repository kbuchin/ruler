using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace General.Model
{
    /// <summary>
    /// Container for a button that allows the button to be toglled on/of
    /// </summary>
    public class ButtonContainer : MonoBehaviour {
        private GameObject m_child;
        private Text m_text;

        private void Awake()
        {
            m_child = transform.Find("Button").gameObject; //Hackily get child
            m_text = m_child.transform.Find("Text").gameObject.GetComponent<Text>();
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

        /// <summary>
        /// Sets the text of the button
        /// </summary>
        /// <param name="text"></param>
        internal void SetText(string text)
        {
            m_text.text = text;
        }
    }
}
