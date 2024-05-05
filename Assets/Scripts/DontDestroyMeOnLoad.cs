using UnityEngine;

public class DontDestroyMeOnLoad : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
