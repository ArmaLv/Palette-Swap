using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PaletteSwapper : MonoBehaviour
{
    public Texture2D dayPalette;
    public Texture2D nightPalette;

    [Space]
    public bool useSpriteSwap = false;
    public Texture2D nightSprite;

    [Space]
    [Header("Transition Settings")]
    public TransitionMode transitionMode = TransitionMode.Blend;
    [Range(0f, 0.5f)] public float edgeSoftness = 0.05f;
    [Range(1, 64)]    public int rowSteps = 8;

    public enum TransitionMode
    {
        Blend            = 0,
        LeftToRight      = 1,
        RightToLeft      = 2,
        TopToBottom      = 3,
        BottomToTop      = 4,
        RowByRow         = 5,
        CenterHorizontal = 6,
        CenterVertical   = 7,
        CenterOutward    = 8,
    }

    private Material runtimeMaterial;

    private void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        runtimeMaterial = new Material(sr.sharedMaterial);
        sr.material = runtimeMaterial;

        runtimeMaterial.SetTexture("_DayPalette", dayPalette);
        runtimeMaterial.SetTexture("_NightPalette", nightPalette);
        runtimeMaterial.SetFloat("_PaletteSize", dayPalette.width);

        if (useSpriteSwap && nightSprite != null)
        {
            runtimeMaterial.SetTexture("_NightTex", nightSprite);
            runtimeMaterial.EnableKeyword("_SPRITE_SWAP");
        }
        else
        {
            runtimeMaterial.DisableKeyword("_SPRITE_SWAP");
        }

        runtimeMaterial.SetInt("_TransitionMode", (int)transitionMode);
        runtimeMaterial.SetFloat("_EdgeSoftness", edgeSoftness);
        runtimeMaterial.SetFloat("_RowSteps", rowSteps);

        TimeManager.Instance.OnTimeSwitched += UpdatePalette;
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