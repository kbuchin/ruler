namespace Divide.UI
{
    using UnityEngine;
    using System.Collections;
    using System;
    using Divide.Controller;

    public class DivideSoldier : MonoBehaviour
    {
        bool m_selected;
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
            controller.SoldierClick(this);
        }

        internal void Deselect()
        {
            m_selected = false;

            //set sprite accordingly (0 is the sprite, 1 is the attached selction box)
            gameObject.GetComponentsInChildren<SpriteRenderer>()[1].enabled = m_selected;
        }
    }
}