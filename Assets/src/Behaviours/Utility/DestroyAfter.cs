using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    public float Seconds;
    
    IEnumerator Start()
    {
        yield return new WaitForSeconds(Seconds);
        Destroy(gameObject);
    }
}
