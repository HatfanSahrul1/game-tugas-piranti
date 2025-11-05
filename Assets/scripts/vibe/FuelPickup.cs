using UnityEngine;

/// <summary>
/// Simple pickup that restores fuel to the player's FuelSystem on collision.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FuelPickup : MonoBehaviour
{
    public float fuelAmount = 20f;
    public bool destroyOnPickup = true;
    // public AudioClip pickupSfx;

    private void Reset()
    {
        // Ensure collider is trigger by default for pickups
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            var fuel = player.GetComponent<FuelSystem>();
            if (fuel != null)
            {
                float added = fuel.AddFuel(fuelAmount);
                // if (added > 0f && pickupSfx != null)
                // {
                //     AudioSource.PlayClipAtPoint(pickupSfx, transform.position);
                // }
            }

            if (destroyOnPickup)
                Destroy(gameObject);
        }
    }
}
