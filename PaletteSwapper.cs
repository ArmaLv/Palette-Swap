using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PaletteSwapper : MonoBehaviour
{
    public Texture2D dayPalette;
    public Texture2D nightPalette;

    private Material runtimeMaterial;

    private void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        runtimeMaterial = new Material(sr.sharedMaterial);
        sr.material = runtimeMaterial;

        runtimeMaterial.SetTexture("_DayPalette", dayPalette);
        runtimeMaterial.SetTexture("_NightPalette", nightPalette);
        runtimeMaterial.SetFloat("_PaletteSize", dayPalette.width);

        // Instant swap on full state change
        TimeManager.Instance.OnTimeSwitched += UpdatePalette;
        // Smooth blend during transition
        TimeManager.Instance.OnTransitionProgress += UpdateBlend;

        UpdatePalette(TimeManager.Instance.CurrentState);
    }

    private void UpdatePalette(TimeManager.TimeState state)
    {
        // Only called at transition end to snap to clean value
        runtimeMaterial.SetFloat("_IsNight", state == TimeManager.TimeState.Night ? 1f : 0f);
    }

    private void UpdateBlend(float t)
    {
        // t: 0 = full day, 1 = full night — fired every frame during transition
        runtimeMaterial.SetFloat("_IsNight", t);
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeSwitched -= UpdatePalette;
            TimeManager.Instance.OnTransitionProgress -= UpdateBlend;
        }
    }
}