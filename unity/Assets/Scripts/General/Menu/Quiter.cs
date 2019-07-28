using UnityEngine;

public class Quiter : MonoBehaviour
{ 
    protected virtual void Start()
    {
        #if UNITY_WEBGL
        Debug.Log("WebPlayer");
        gameObject.SetActive(false);
        #endif
    }

    public void Quit()
    {
        Application.Quit();
    }
}
