using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the player's fuel which decreases over time and can be increased by pickups.
/// Emits events when fuel changes and when fuel reaches zero.
/// </summary>
public class FuelSystem : MonoBehaviour
{
    [Header("Fuel Settings")]
    public float maxFuel = 100f;
    public float startFuel = 100f;
    public float fuelDrainPerSecond = 2f;

    [Header("Auto Attach")]
    public PlayerController player;

    public float CurrentFuel { get; private set; }

    public UnityEvent<float> OnFuelChanged; // passes normalized 0..1
    public UnityEvent OnFuelEmpty;

    private void Awake()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        CurrentFuel = Mathf.Clamp(startFuel, 0f, maxFuel);
    }

    private void Update()
    {
        // Drain fuel over time
        if (CurrentFuel > 0f)
        {
            CurrentFuel = Mathf.Max(0f, CurrentFuel - fuelDrainPerSecond * Time.deltaTime);
            OnFuelChanged?.Invoke(CurrentFuel / maxFuel);

            if (CurrentFuel <= 0f)
            {
                OnFuelEmpty?.Invoke();
            }
        }
    }

    /// <summary>
    /// Add fuel amount (clamped). Returns actual amount added.
    /// </summary>
    public float AddFuel(float amount)
    {
        if (amount <= 0f) return 0f;
        float before = CurrentFuel;
        CurrentFuel = Mathf.Min(maxFuel, CurrentFuel + amount);
        float added = CurrentFuel - before;
        if (added > 0f)
            OnFuelChanged?.Invoke(CurrentFuel / maxFuel);
        return added;
    }

    /// <summary>
    /// Consume fuel immediately (returns whether there was enough fuel)
    /// </summary>
    public bool ConsumeFuel(float amount)
    {
        if (CurrentFuel >= amount)
        {
            CurrentFuel -= amount;
            OnFuelChanged?.Invoke(CurrentFuel / maxFuel);
            if (CurrentFuel <= 0f) OnFuelEmpty?.Invoke();
            return true;
        }
        return false;
    }
}
