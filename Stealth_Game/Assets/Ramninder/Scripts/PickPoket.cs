using UnityEngine;

public class PickPoket : MonoBehaviour
{
    //[SerializeField] private GameObject instructionKey;
    [SerializeField] private float pickPocketDistance;

    private GameObject securityGuard;

    public bool basement1Key;
    public bool basement2Key;

    void Update()
    {
        if (securityGuard != null && securityGuard.GetComponent<SecurityOfficerScript>().hasKey)
        {
            //instructionKey.SetActive(true);
            //instructionKey.transform.rotation = Quaternion.Euler(instructionKey.transform.rotation.x, 0, instructionKey.transform.rotation.z);
            if (Input.GetKeyDown(KeyCode.C))
            {
                securityGuard.GetComponent<SecurityOfficerScript>().key.transform.SetParent(gameObject.transform);
                securityGuard.GetComponent<SecurityOfficerScript>().key.transform.position = transform.position;

                if (securityGuard.GetComponent<SecurityOfficerScript>().keyName == "Basement 1")
                {
                    basement1Key = true;
                }
                else if (securityGuard.GetComponent<SecurityOfficerScript>().keyName == "Basement 2")
                {
                    basement2Key = true;
                }

                securityGuard.GetComponent<SecurityOfficerScript>().hasKey = false;
            }
        }
        else
        {
            //instructionKey.SetActive(false);
            //instructionKey.transform.rotation = Quaternion.Euler(instructionKey.transform.rotation.x, 0, instructionKey.transform.rotation.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Guard"))
        {
            securityGuard = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Guard"))
        {
            //instructionKey.SetActive(false);
            //instructionKey.transform.rotation = Quaternion.Euler(instructionKey.transform.rotation.x, 0, instructionKey.transform.rotation.z);
            securityGuard = null;
        }
    }
}
