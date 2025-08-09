using UnityEngine;

public class EyeBlink : MonoBehaviour
{
    public Renderer eyeRenderer;
    public float blinkInterval = 2f;
    public float blinkDuration = 0.2f;
    public Color emissionColor = Color.red;

    private Material eyeMaterial;
    private float timer;
    private bool isBlinking;

    void Start()
    {
        eyeMaterial = eyeRenderer.material;
        EnableEmission(true);
        timer = blinkInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (!isBlinking && timer <= 0)
        {
            StartCoroutine(Blink());
        }
    }

    System.Collections.IEnumerator Blink()
    {
        isBlinking = true;
        EnableEmission(false);
        yield return new WaitForSeconds(blinkDuration);
        EnableEmission(true);
        timer = blinkInterval;
        isBlinking = false;
    }

    void EnableEmission(bool on)
    {
        if (on)
            eyeMaterial.SetColor("_EmissionColor", emissionColor * 2f);
        else
            eyeMaterial.SetColor("_EmissionColor", Color.black);
    }
}
