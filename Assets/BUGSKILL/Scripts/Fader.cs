using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Fader : MonoBehaviour
{
    public float fadeDuration = 1.0f;

    [SerializeField] private List<TMP_Text> tmpTexts = new List<TMP_Text>();
    [SerializeField] private List<Image> uiImages = new List<Image>();
    [SerializeField] private List<Material> customMaterials = new List<Material>();
    [SerializeField] private List<Material> materials = new List<Material>();

    void Start()
    {
        // Find all TMP_Text components
        foreach (TMP_Text tmp in GetComponentsInChildren<TMP_Text>(true))
        {
            tmpTexts.Add(tmp);
        }

        // Find all UI Images (for fading UI elements)
        foreach (Image image in GetComponentsInChildren<Image>(true))
        {
            uiImages.Add(image);
        }

        // Find all renderers for Quads and get their materials
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.HasProperty("_Alpha")) // Ensure material supports color changes
                {
                    customMaterials.Add(mat);
                }
                else if (mat.HasProperty("_Color"))
                {
                    materials.Add(mat);
                }
            }
        }
    }

    public void FadeIn()
    {
        Fade(0f, 1f);
    }

    public void FadeOut()
    {
        Fade(1f, 0f);
    }

    public void SetAlpha(float alpha)
    {
        foreach (TMP_Text tmp in tmpTexts)
        {
            Color color = tmp.color;
            color.a = alpha;
            tmp.color = color;
        }

        foreach (Image image in uiImages)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        foreach (Material mat in materials)
        {
            Color color = mat.color;
            color.a = alpha;
            mat.color = color;
        }

        foreach (Material mat in customMaterials)
        {
            mat.SetFloat("_Alpha", alpha);
        }
    }

    private void Fade(float startAlpha, float targetAlpha)
    {
        LeanTween.cancel(gameObject);
        // Fade TextMeshPro text
        foreach (TMP_Text tmp in tmpTexts)
        {
            LeanTween.value(gameObject, startAlpha, targetAlpha, fadeDuration)
                .setOnUpdate((float value) =>
                {
                    Color color = tmp.color;
                    color.a = value;
                    tmp.color = color;
                });
        }

        // Fade UI images
        foreach (Image image in uiImages)
        {
            LeanTween.value(gameObject, startAlpha, targetAlpha, fadeDuration)
                .setOnUpdate((float value) =>
                {
                    Color color = image.color;
                    color.a = value;
                    image.color = color;
                });
        }

        // Fade materials (for Quads with regular lit materials)
        foreach (Material mat in materials)
        {
            LeanTween.value(gameObject, startAlpha, targetAlpha, fadeDuration)
                .setOnUpdate((float value) =>
                {
                    Color color = mat.color;
                    color.a = value;
                    mat.color = color;
                });
        }

        // Fade materials (for Quads with custom materials)
        foreach (Material mat in customMaterials)
        {
            LeanTween.value(gameObject, startAlpha, targetAlpha, fadeDuration)
                .setOnUpdate((float value) =>
                {
                    mat.SetFloat("_Alpha", value);
                });
        }
    }
}