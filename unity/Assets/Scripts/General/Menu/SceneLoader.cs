namespace General.Menu
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SceneLoader : MonoBehaviour
    { 
        public void LoadScene(string a_nextScene)
        {
            SceneManager.LoadScene(a_nextScene);
        }
    }
}
