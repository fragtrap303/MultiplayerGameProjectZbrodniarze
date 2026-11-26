
using UnityEngine;
using UnityEngine.UI;

public class CustomizationWindow : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Material originalMaterial;
    [SerializeField] private Renderer[] targetRenderers;

    [Header("UI Sliders")]
    [SerializeField] private Slider hueSlider;
    [SerializeField] private Slider saturationSlider;
    [SerializeField] private Slider brightnessSlider;

    private Material runtimeMaterial;

    private float hue;
    private float saturation;
    private float brightness;

    private string colorProperty;

    private void Start()
    {
        if (originalMaterial == null)
        {
            Debug.LogError("Assign an original material!");
            return;
        }

        // Create one independent material instance.
        runtimeMaterial = new Material(originalMaterial);

        // Support Built-in and URP Lit shaders.
        colorProperty = runtimeMaterial.HasProperty("_BaseColor")
            ? "_BaseColor"
            : "_Color";

        Color.RGBToHSV(
            runtimeMaterial.GetColor(colorProperty),
            out hue, out saturation, out brightness
        );

        // Set up the sliders.
        hueSlider.minValue = 0f;
        hueSlider.maxValue = 1f;

        saturationSlider.minValue = 0f;
        saturationSlider.maxValue = 1f;

        brightnessSlider.minValue = 0f;
        brightnessSlider.maxValue = 1f;

        hueSlider.SetValueWithoutNotify(hue);
        saturationSlider.SetValueWithoutNotify(saturation);
        brightnessSlider.SetValueWithoutNotify(brightness);

        hueSlider.onValueChanged.AddListener(SetHue);
        saturationSlider.onValueChanged.AddListener(SetSaturation);
        brightnessSlider.onValueChanged.AddListener(SetBrightness);

        // Make every target use the SAME material instance.
        foreach (Renderer target in targetRenderers)
        {
            if (target != null)
                target.sharedMaterial = runtimeMaterial;
        }

        UpdateColor();
    }

    public void SetHue(float value)
    {
        hue = value;
        UpdateColor();
    }

    public void SetSaturation(float value)
    {
        saturation = value;
        UpdateColor();
    }

    public void SetBrightness(float value)
    {
        brightness = value;
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (runtimeMaterial == null)
            return;

        Color newColor = Color.HSVToRGB(
            hue, saturation, brightness
        );

        runtimeMaterial.SetColor(colorProperty, newColor);
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }
}
