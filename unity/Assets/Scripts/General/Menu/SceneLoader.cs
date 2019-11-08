namespace General.Menu
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Simple class encapsulating load scene method.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>
        /// Run when loading a new scene.
        /// </summary>
        /// <param name="a_nextScene"></param>
        public void LoadScene(string a_nextScene)
        {
            SceneManager.LoadScene(a_nextScene);
        }
    }
}
