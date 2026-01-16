using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyInventoryUI : MonoBehaviour
{
    [System.Serializable]
    public class KeyUIEntry
    {
        public string keyId;
        public Image icon;
        public Image checkMark;
        public Sprite sprite;
    }

    [Header("Key UI List")]
    [SerializeField] private List<KeyUIEntry> keys = new List<KeyUIEntry>();

    [Header("UI Root")]
    [SerializeField] private RectTransform canvasRoot;

    [Header("Fly Prefab")]
    [SerializeField] private Image flyImagePrefab;

    [Header("Alpha")]
    [SerializeField, Range(0f, 1f)] private float notOwnedAlpha = 0.25f;
    [SerializeField, Range(0f, 1f)] private float ownedAlpha = 1f;

    [Header("Timings")]
    [SerializeField] private float pickupFlyDuration = 0.45f;
    [SerializeField] private float useFlyDuration = 0.35f;

    [Header("Curve")]
    [SerializeField] private AnimationCurve flyEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Check Mark Blink")]
    [SerializeField] private float checkBlinkDuration = 0.55f;
    [SerializeField] private int checkBlinkCount = 2;

    private readonly Dictionary<string, KeyUIEntry> map = new Dictionary<string, KeyUIEntry>();
    private readonly Dictionary<string, Coroutine> checkRoutines = new Dictionary<string, Coroutine>();

    private void Awake()
    {
        map.Clear();
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i] == null || string.IsNullOrEmpty(keys[i].keyId) || keys[i].icon == null) continue;
            map[keys[i].keyId] = keys[i];
        }

        foreach (var kv in map)
        {
            SetOwned(kv.Key, false, true);
            HideCheck(kv.Key);
        }
    }

    public void SetOwned(string keyId, bool owned, bool instant = false)
    {
        if (!map.TryGetValue(keyId, out var entry)) return;
        if (entry.icon == null) return;

        float a = owned ? ownedAlpha : notOwnedAlpha;
        Color c = entry.icon.color;
        c.a = a;
        entry.icon.color = c;
    }

    public void PlayPickupFly(string keyId, Vector3 worldFrom, Camera worldCam)
    {
        if (!map.TryGetValue(keyId, out var entry)) return;
        if (entry.icon == null || flyImagePrefab == null || canvasRoot == null || worldCam == null) return;

        StartCoroutine(PickupFlyRoutine(entry, keyId, worldFrom, worldCam));
    }

    public void PlayUseFly(string keyId, Vector3 worldTo, Camera worldCam)
    {
        if (!map.TryGetValue(keyId, out var entry)) return;
        if (entry.icon == null || flyImagePrefab == null || canvasRoot == null || worldCam == null) return;

        StartCoroutine(UseFlyRoutine(entry, keyId, worldTo, worldCam));
    }

    private IEnumerator PickupFlyRoutine(KeyUIEntry entry, string keyId, Vector3 worldFrom, Camera worldCam)
    {
        Vector2 startLocal = WorldToCanvasPosition(worldFrom, worldCam, canvasRoot);

        Vector2 endScreen = RectTransformUtility.WorldToScreenPoint(null, entry.icon.rectTransform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRoot, endScreen, null, out var endLocal);

        Image fly = Instantiate(flyImagePrefab, canvasRoot);
        fly.sprite = entry.sprite != null ? entry.sprite : entry.icon.sprite;
        fly.raycastTarget = false;

        Color flyC = fly.color;
        flyC.a = notOwnedAlpha;
        fly.color = flyC;

        RectTransform flyRt = fly.rectTransform;
        flyRt.anchoredPosition = startLocal;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, pickupFlyDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float e = flyEase.Evaluate(Mathf.Clamp01(t));

            flyRt.anchoredPosition = Vector2.LerpUnclamped(startLocal, endLocal, e);

            Color c = fly.color;
            c.a = Mathf.Lerp(notOwnedAlpha, ownedAlpha, e);
            fly.color = c;

            yield return null;
        }

        Destroy(fly.gameObject);
        SetOwned(keyId, true, true);
        BlinkCheck(keyId);
    }

    private IEnumerator UseFlyRoutine(KeyUIEntry entry, string keyId, Vector3 worldTo, Camera worldCam)
    {
        Vector2 startScreenPos = RectTransformUtility.WorldToScreenPoint(null, entry.icon.rectTransform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRoot, startScreenPos, null, out var startLocal);

        Image fly = Instantiate(flyImagePrefab, canvasRoot);
        fly.sprite = entry.sprite != null ? entry.sprite : entry.icon.sprite;
        fly.raycastTarget = false;

        Color flyC = fly.color;
        flyC.a = ownedAlpha;
        fly.color = flyC;

        RectTransform flyRt = fly.rectTransform;
        flyRt.anchoredPosition = startLocal;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, useFlyDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float e = flyEase.Evaluate(Mathf.Clamp01(t));

            Vector2 endLocal = WorldToCanvasPosition(worldTo, worldCam, canvasRoot);

            flyRt.anchoredPosition = Vector2.LerpUnclamped(startLocal, endLocal, e);
            yield return null;
        }

        Destroy(fly.gameObject);
        SetOwned(keyId, true, true);
    }


    private void BlinkCheck(string keyId)
    {
        if (!map.TryGetValue(keyId, out var entry)) return;
        if (entry.checkMark == null) return;

        if (checkRoutines.TryGetValue(keyId, out var running) && running != null)
            StopCoroutine(running);

        checkRoutines[keyId] = StartCoroutine(CheckBlinkRoutine(keyId, entry.checkMark));
    }

    private IEnumerator CheckBlinkRoutine(string keyId, Image check)
    {
        check.gameObject.SetActive(true);

        Color c = check.color;
        c.a = 0f;
        check.color = c;

        float dur = Mathf.Max(0.0001f, checkBlinkDuration);
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / dur);
            float a = Mathf.Sin(p * Mathf.PI * checkBlinkCount);
            a = Mathf.Clamp01(a);

            Color cc = check.color;
            cc.a = a;
            check.color = cc;

            yield return null;
        }

        Color end = check.color;
        end.a = 0f;
        check.color = end;
        check.gameObject.SetActive(false);

        checkRoutines[keyId] = null;
    }

    private void HideCheck(string keyId)
    {
        if (!map.TryGetValue(keyId, out var entry)) return;
        if (entry.checkMark == null) return;

        Color c = entry.checkMark.color;
        c.a = 0f;
        entry.checkMark.color = c;
        entry.checkMark.gameObject.SetActive(false);
    }

    private static Vector2 WorldToCanvasPosition(Vector3 worldPos, Camera cam, RectTransform canvasRoot)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRoot, screen, null, out var local);
        return local;
    }
}
