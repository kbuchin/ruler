using UnityEngine;
using System.Collections;
using System;

namespace Divide
{
    public class Thing : MonoBehaviour
    {
        bool m_selected;
        private GameController controller;

        void Awake()
        {
            controller = GameObject.FindGameObjectWithTag(Tags.GameController).GetComponent<GameController>();
        }

        // Use this for initialization
        void Start()
        {
            this.m_selected = false;
        }

        // Update is called once per frame
        void Update()
        {

        }


        void OnMouseUpAsButton()
        {
            //change selection
            m_selected = !m_selected;

            //set sprite accordingly (0 is the sprite, 1 is the attached selection box)
            gameObject.GetComponentsInChildren<SpriteRenderer>()[1].enabled = m_selected;

            //inform controller
            controller.thingClick(this, m_selected);
        }

        internal void deselect()
        {
            m_selected = false;

            //set sprite accordingly (0 is the sprite, 1 is the attached selction box)
            gameObject.GetComponentsInChildren<SpriteRenderer>()[1].enabled = m_selected;
        }
    }
}