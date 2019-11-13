namespace Divide
{
    using UnityEngine;

    /// <summary>
    /// Class for the game objects (archer, spearmen, mage) that need to be divided.
    /// Can be selected in order to swap.
    /// </summary>
    public class DivideSoldier : MonoBehaviour
    {
        // whether the soldier is currently selected
        private bool m_selected;

        private DivideController controller;

        // Use this for initialization
        void Start()
        {
            this.m_selected = false;
            controller = FindObjectOfType<DivideController>();
        }

        void OnMouseUpAsButton()
        {
            //change selection
            m_selected = !m_selected;

            //set sprite accordingly (0 is the sprite, 1 is the attached selection box)
            gameObject.GetComponentsInChildren<SpriteRenderer>()[1].enabled = m_selected;

            //inform controller
            controller.HandleSoldierClick(this);
        }

        /// <summary>
        /// Soldier is deselected, either due to double selection or a swap 
        /// </summary>
        public void Deselect()
        {
            m_selected = false;

            //set sprite accordingly (0 is the sprite, 1 is the attached selction box)
            gameObject.GetComponentsInChildren<SpriteRenderer>()[1].enabled = m_selected;
        }
    }
}