using UnityEngine;
using System.Collections;

public class TimedPlatform1 : MonoBehaviour
{
    public float disappearTime = 5f;
    public float fadeDuration = 1f;

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
            Debug.Log("Player Collided");
            Invoke(nameof(StartFadeOut), disappearTime);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player leaving timed platform");
            // Optionally trigger reappearance
            Invoke(nameof(StartFadeIn), disappearTime + 2f); // Add a delay after fade out
        }
    }

    private void StartFadeOut()
    {
        StartCoroutine(FadeOutAndDisable());
    }

    private void StartFadeIn()
    {
        StartCoroutine(FadeInAndEnable());
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
        platformRenderer.enabled = false; // Hide object instead of SetActive
    }

    private IEnumerator FadeInAndEnable()
    {
        platformCollider.enabled = true;
        platformRenderer.enabled = true;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            platformMaterial.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        platformMaterial.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    }
}

