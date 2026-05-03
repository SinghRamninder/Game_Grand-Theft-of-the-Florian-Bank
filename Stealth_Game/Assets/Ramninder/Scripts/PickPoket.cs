using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class PickPoket : MonoBehaviour
{
    [SerializeField] private GameObject instructionKey;
    private GameObject keyStolen;

    [SerializeField] private KeyInventoryUI keyUI;
    private Camera worldCamera;

    private GameObject key;
    private List<string> keysHave = new List<string>();

    void Start()
    {
        if (worldCamera == null) worldCamera = Camera.main;
    }

    void Update()
    {
        if (key != null)
        {
            if (instructionKey != null)
            {
                instructionKey.SetActive(true);
                instructionKey.transform.rotation = Quaternion.Euler(instructionKey.transform.rotation.x, 0, instructionKey.transform.rotation.z);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                keysHave.Add(key.name);

                if (keyUI != null && worldCamera != null)
                    keyUI.PlayPickupFly(key.name, key.transform.position, worldCamera);

                key.SetActive(false);
                //StartCoroutine(KeyTextHideShow());
                key = null;
                if (instructionKey != null) instructionKey.SetActive(false);
            }
        }
        else
        {
            if (instructionKey != null) instructionKey.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Key"))
            key = collision.gameObject;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Key"))
        {
            if (instructionKey != null) instructionKey.SetActive(false);
            key = null;
        }
    }

    public bool hasKey(string key)
    {
        return keysHave.Contains(key);
    }

    public void PlayUseKeyFly(string keyId, Vector3 worldTargetPosition)
    {
        if (!hasKey(keyId)) return;
        if (keyUI == null || worldCamera == null) return;

        keyUI.PlayUseFly(keyId, worldTargetPosition, worldCamera);
    }

    //private IEnumerator KeyTextHideShow()
    //{
    //    if (keyStolen != null) keyStolen.SetActive(true);
    //    yield return new WaitForSeconds(3f);
    //    if (keyStolen != null) keyStolen.SetActive(false);
    //}
}
