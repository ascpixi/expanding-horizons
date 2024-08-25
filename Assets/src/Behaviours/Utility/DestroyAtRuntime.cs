using UnityEngine;

public class DestroyAtRuntime : MonoBehaviour
{
    void Awake() => Destroy(gameObject);
}
