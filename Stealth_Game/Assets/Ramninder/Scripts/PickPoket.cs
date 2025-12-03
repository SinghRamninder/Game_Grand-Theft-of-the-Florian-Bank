using UnityEngine;

public class PickPoket : MonoBehaviour
{
    [SerializeField] private Transform guardPosition;
    [SerializeField] private GameObject key;
    [SerializeField] private float pickPocketDistance;
    [SerializeField] private GameObject door;

    private GameObject chest;

    private bool hasKey;
    private bool canOpenChest;

    void Update()
    {
        if(Vector2.Distance(transform.position, guardPosition.position) < pickPocketDistance)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                key.transform.SetParent(gameObject.transform);
                key.transform.position = transform.position;

                hasKey = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.O) && canOpenChest)
        {
            Destroy(chest);
            Destroy(key);
            canOpenChest = false;
            door.GetComponent<DoorScript>().enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Chest") && hasKey)
        {
            chest = collision.gameObject;
            canOpenChest = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Chest"))
        {
            canOpenChest = false;
        }
    }
}
