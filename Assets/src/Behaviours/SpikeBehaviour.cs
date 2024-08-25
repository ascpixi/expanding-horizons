using System.Collections;
using UnityEngine;

public class SpikeBehaviour : MonoBehaviour
{
    public Vector2 HitboxSize = new(1f, 1f);
    
    IEnumerator Start()
    {
        var bounds = new Bounds(transform.position, HitboxSize);
        var player = PlayerController.Main;

        while (true) {
            if (bounds.Contains(player.transform.position)) {
                player.Damage();
                yield return new WaitForSeconds(player.TeleportDuration);
            }

            yield return null;
        }
    }
}