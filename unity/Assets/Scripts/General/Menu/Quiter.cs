namespace General.Menu
{
    using UnityEngine;

    /// <summary>
    /// Class responsible for cleanup upon quiting
    /// </summary>
    public class Quiter : MonoBehaviour
    {
        public virtual void Awake()
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
