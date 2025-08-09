using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    public bool moveLight = true;
    public float flickerRate = 0.2f; // How often to pick a new target (in seconds)
    public float intensityMin = 1.5f;
    public float intensityMax = 3.0f;

    public Color colorA = new Color(1f, 0.5f, 0f); // Orange
    public Color colorB = new Color(1f, 1f, 0.3f); // Yellow

    private Light lightsource;
    private Vector3 origin;
    private Vector3 target = Vector3.zero;

    private float flickerTimer = 0f;
    private float targetIntensity;
    private float currentIntensity;

    private Color targetColor;
    private Color currentColor;

    void Start()
    {
        origin = transform.position;
        lightsource = GetComponent<Light>();

        targetIntensity = Random.Range(intensityMin, intensityMax);
        currentIntensity = targetIntensity;

        targetColor = Color.Lerp(colorA, colorB, Random.Range(0f, 1f));
        currentColor = targetColor;
    }

    void Update()
    {
        // Timer to choose new flicker targets
        flickerTimer -= Time.deltaTime;
        if (flickerTimer <= 0f)
        {
            flickerTimer = flickerRate;

            // Pick new random target values
            targetIntensity = Random.Range(intensityMin, intensityMax);
            targetColor = Color.Lerp(colorA, colorB, Random.Range(0f, 1f));
        }

        // Smoothly transition toward target values
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * 3f);
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * 3f);

        lightsource.intensity = currentIntensity;
        lightsource.color = currentColor;

        // Optional: subtle light movement
        if (moveLight)
        {
            target = origin + Random.insideUnitSphere * 0.03f;
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 5f);
        }
    }
}
