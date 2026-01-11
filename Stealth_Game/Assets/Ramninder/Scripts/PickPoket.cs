using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PickPoket : MonoBehaviour
{
    [SerializeField] private GameObject instructionKey;

    private GameObject key;

    private List<string> keysHave = new List<string>();

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
                //key.transform.SetParent(gameObject.transform);
                //key.transform.position = transform.position;
                keysHave.Add(key.name);
                key.SetActive(false);
                key = null;
                instructionKey.SetActive(false);
            }
        }
        else
        {
            if (instructionKey != null)
            {
                instructionKey.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Key"))
        {
            key = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Key"))
        {
            if (instructionKey != null)
            {
                instructionKey.SetActive(false);
            }

            key = null;
        }
    }

    public bool hasKey(string key)
    {
        if (keysHave.Contains(key))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
