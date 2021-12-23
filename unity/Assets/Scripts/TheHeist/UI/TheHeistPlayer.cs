namespace TheHeist
{
    using UnityEngine;
    using Util.Geometry.Polygon;

    /// <summary>
    /// Represents the lighthouse object in the game.
    /// Holds its position as well as the corresponding visibility polygon.
    /// Handles user clicks and drags.
    /// </summary>
    public class TheHeistPlayer : MonoBehaviour
    {

        private TheHeistController m_controller;


        /// <summary>
        /// Stores player position. Updates vision after a change in position.
        /// </summary>
        public Vector3 Pos
        {
            get
            {
                return gameObject.transform.position;
            }
            set
            {
                gameObject.transform.position = value;

            }
        }

        void OnDestroy()
        {
            
        }

    }
}
