namespace General.Menu
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Container for a button that allows the button to be toggled on/of.
    /// </summary>
    public class ButtonContainer : MonoBehaviour
    {
        private GameObject m_child;
        private Text m_text;

        // Use this for initialization
        void Awake()
        {
            // find the button and text contained in the game object
            m_child = transform.Find("Button").gameObject;
            m_text = m_child.transform.Find("Text").gameObject.GetComponent<Text>();

            if (m_child == null || m_text == null)
            {
                throw new Exception("Could not find child button and corresponding text");
            }
        }

        /// <summary>
        /// Enables the contained button
        /// </summary>
        public void Enable()
        {
            m_child.SetActive(true);
        }

        /// <summary>
        /// Disables the contained button
        /// </summary>
        public void Disable()
        {
            m_child.SetActive(false);
        }

        /// <summary>
        /// Sets the text of the button
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            m_text.text = text;
        }
    }
}
