using UnityEngine;

public class CreditsBehaviour : MonoBehaviour
{
    void Update()
    {
        if (Input.anyKey) {
            Application.Quit();
        }
    }
}