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

        // Both palettes live on the material permanently
        runtimeMaterial.SetTexture("_DayPalette", dayPalette);
        runtimeMaterial.SetTexture("_NightPalette", nightPalette);
        runtimeMaterial.SetFloat("_PaletteSize", dayPalette.width);

        TimeManager.Instance.OnTimeSwitched += UpdatePalette;
        UpdatePalette(TimeManager.Instance.CurrentState);
    }

    private void UpdatePalette(TimeManager.TimeState state)
    {
        // Swap which palette is "active" by swapping NightPalette to itself or day
        // Instead we drive a blend weight: 0 = day, 1 = night
        runtimeMaterial.SetFloat("_IsNight", state == TimeManager.TimeState.Night ? 1f : 0f);
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnTimeSwitched -= UpdatePalette;
    }
}