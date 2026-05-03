using UnityEngine;
using UnityEngine.Rendering.Universal;

public class KeyGlow : MonoBehaviour
{
    [HideInInspector] public bool keyGlow = false;
    private Light2D keyLight;

    private void Start()
    {
        if (keyLight == null)
        {
            keyLight = GetComponentInChildren<Light2D>();
        }
    }

    private void Update()
    {
        if (keyGlow)
        {
            if (keyLight != null)
            {
                float t = Mathf.PingPong(Time.time, 1f);

                keyLight.intensity = Mathf.Lerp(0f, 15f, t);
            }
        }
    }
}
