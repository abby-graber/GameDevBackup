using UnityEngine;
using System.Collections;

public class TimedPlatform : MonoBehaviour
{
    public float disappearTime = 5f; // Time before platform starts fading
    public float fadeDuration = 1f;  // How long the fade effect lasts
    private Collider platformCollider;
    private Renderer platformRenderer;
    private Material platformMaterial;
    private Color originalColor;

    private void Start()
    {
        platformCollider = GetComponent<Collider>();
        platformRenderer = GetComponent<Renderer>();
        platformMaterial = platformRenderer.material;
        originalColor = platformMaterial.color;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Invoke(nameof(StartFadeOut), disappearTime);
        }
    }

    private void StartFadeOut()
    {
        StartCoroutine(FadeOutAndDisable());
    }

    private IEnumerator FadeOutAndDisable()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            platformMaterial.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        platformMaterial.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        platformCollider.enabled = false;
        gameObject.SetActive(false);
    }
}
