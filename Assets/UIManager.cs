using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI manager that shows fuel (slider) and survival time (score).
/// Assumes a FuelSystem is present in scene and will subscribe to its events.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider fuelSlider;
    public UnityEngine.UI.Image fuelImage;
    public Text fuelText; // optional, kept for backward compatibility
    public Text survivalTimeText;

    [Header("References")]
    public FuelSystem fuelSystem;

    private float survivalTimer = 0f;
    private bool running = true;

    // Public read-only exposure of survival time for other systems
    public float SurvivalTime => survivalTimer;

    private void Start()
    {
        if (fuelSystem == null)
            fuelSystem = FindObjectOfType<FuelSystem>();

        if (fuelSystem != null)
        {
            fuelSystem.OnFuelChanged.AddListener(OnFuelChanged);
        }

        UpdateFuelUIImmediate();
    }

    private void Update()
    {
        if (!running) return;
        survivalTimer += Time.deltaTime;
        if (survivalTimeText != null)
            survivalTimeText.text = $"Time: {survivalTimer:F1}s";
    }

    private void OnDestroy()
    {
        if (fuelSystem != null)
            fuelSystem.OnFuelChanged.RemoveListener(OnFuelChanged);
    }

    private void OnFuelChanged(float normalized)
    {
        if (fuelSlider != null)
        {
            fuelSlider.value = normalized;
        }

        // Update image fill if present (preferred)
        if (fuelImage != null)
        {
            fuelImage.fillAmount = Mathf.Clamp01(normalized);
        }

        // Optional: also update text if assigned
        if (fuelText != null && fuelSystem != null)
        {
            fuelText.text = $"Fuel: {fuelSystem.CurrentFuel:F0}/{fuelSystem.maxFuel:F0}";
        }
    }

    public void UpdateFuelUIImmediate()
    {
        if (fuelSystem == null) return;
        OnFuelChanged(fuelSystem.CurrentFuel / fuelSystem.maxFuel);
    }

    public void StopTimer()
    {
        running = false;
    }
}
