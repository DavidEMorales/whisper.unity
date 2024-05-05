using UnityEngine;

public class EnableIfIsMaleEqual : MonoBehaviour
{
    public bool EnableIfEqualToThis;

    void Start()
    {
        if (EnableIfEqualToThis == GameManager.Instance.IsMale)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
