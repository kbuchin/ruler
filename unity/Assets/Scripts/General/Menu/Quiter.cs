namespace Menu
{
    using UnityEngine;

    /// <summary>
    /// Class responsible for cleanup upon quiting
    /// </summary>
    public class Quiter : MonoBehaviour
    {
        public virtual void Start()
        {
            // run when using WEBGL
            #if UNITY_WEBGL
            Debug.Log("WebPlayer");
            gameObject.SetActive(false);
            #endif
        }

        /// <summary>
        /// Called when quiting game
        /// </summary>
        public void Quit()
        {
            Application.Quit();
        }
    }
}
