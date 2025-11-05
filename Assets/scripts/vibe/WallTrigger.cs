using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attach this to wall GameObjects. When PlayerController collides (or triggers) this object,
/// the public UnityEvent OnPlayerHit will be invoked. You can assign GameOverManager.ShowGameOver in Inspector.
/// </summary>
public class WallTrigger : MonoBehaviour
{
    [Tooltip("If specified, only objects with this tag will trigger the event. Leave empty to use PlayerController check.")]
    public string playerTag = "";

    public UnityEvent OnPlayerHit;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsPlayer(collision.gameObject))
        {
            OnPlayerHit?.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other.gameObject))
        {
            OnPlayerHit?.Invoke();
        }
    }

    private bool IsPlayer(GameObject go)
    {
        if (!string.IsNullOrEmpty(playerTag))
            return go.CompareTag(playerTag);

        return go.GetComponent<PlayerController>() != null;
    }
}
