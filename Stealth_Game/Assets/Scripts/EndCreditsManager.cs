using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class EndCreditsManager : MonoBehaviour
{
    public enum CreditType
    {
        Text,
        Image
    }

    [System.Serializable]
    public class CreditStep
    {
        [HideInInspector] public CreditType type = CreditType.Text;

        [Tooltip("The actual TextMeshPro component for this credit line.")]
        public TMP_Text text;

        [Tooltip("The Image component for this credit line, if Image type is selected.")]
        public Image image;

        [Tooltip("How much time to wait before fading this text/image in.")]
        public float delayBefore = 0.5f;

        [Tooltip("How long this text/image stays fully visible before fading out.")]
        public float holdTime = 2f;

        [Tooltip("Duration of the fade-in and fade-out effect for this text/image.")]
        public float fadeDuration = 0.4f;
    }

    [Header("Black Fade Effect")]
    [Tooltip("The full screen image component used for fading out the background.")]
    public Image blackFadeImage;
    [Tooltip("Amount of seconds to delay before initiating the black fade.")]
    public float startCreditsDelay = 1f;
    [Tooltip("How long the black screen fade-in takes in seconds.")]
    public float blackFadeInDuration = 0.8f;

    [Header("Credits Sequence")]
    [Tooltip("The sequential list of credits to display on the screen.")]
    public List<CreditStep> credits = new List<CreditStep>();

    public void StartCreditsSequence()
    {
        StartCoroutine(PlayCreditsSequence());
    }

    public IEnumerator PlayCreditsSequence()
    {
        if (startCreditsDelay > 0f)
        {
            yield return new WaitForSeconds(startCreditsDelay);
        }

        if (blackFadeImage != null)
        {
            yield return StartCoroutine(FadeImageAlpha(blackFadeImage, 0f, 1f, blackFadeInDuration));
        }

        if (credits != null && credits.Count > 0)
        {
            for (int i = 0; i < credits.Count; i++)
            {
                CreditStep step = credits[i];

                if (step == null) continue;

                if (step.delayBefore > 0f)
                    yield return new WaitForSeconds(step.delayBefore);

                if (step.type == CreditType.Text && step.text != null)
                {
                    step.text.gameObject.SetActive(true);
                    
                    yield return StartCoroutine(FadeTMPAlpha(step.text, 0f, 1f, step.fadeDuration));

                    if (step.holdTime > 0f)
                        yield return new WaitForSeconds(step.holdTime);

                    yield return StartCoroutine(FadeTMPAlpha(step.text, 1f, 0f, step.fadeDuration));

                    step.text.gameObject.SetActive(false);
                }
                else if (step.type == CreditType.Image && step.image != null)
                {
                    step.image.gameObject.SetActive(true);
                    
                    yield return StartCoroutine(FadeImageAlpha(step.image, 0f, 1f, step.fadeDuration));

                    if (step.holdTime > 0f)
                        yield return new WaitForSeconds(step.holdTime);

                    yield return StartCoroutine(FadeImageAlpha(step.image, 1f, 0f, step.fadeDuration));

                    step.image.gameObject.SetActive(false);
                }
            }
        }
    }

    private IEnumerator FadeImageAlpha(Image img, float from, float to, float duration)
    {
        if (img == null) yield break;

        Color c = img.color;

        if (duration <= 0f)
        {
            img.color = new Color(c.r, c.g, c.b, to);
            yield break;
        }

        float t = 0f;
        img.color = new Color(c.r, c.g, c.b, from);

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            img.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        img.color = new Color(c.r, c.g, c.b, to);
    }

    private IEnumerator FadeTMPAlpha(TMP_Text tmp, float from, float to, float duration)
    {
        if (tmp == null) yield break;

        if (duration <= 0f)
        {
            tmp.alpha = to;
            yield break;
        }

        float t = 0f;
        tmp.alpha = from;

        while (t < duration)
        {
            t += Time.deltaTime;
            tmp.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }

        tmp.alpha = to;
    }
}
