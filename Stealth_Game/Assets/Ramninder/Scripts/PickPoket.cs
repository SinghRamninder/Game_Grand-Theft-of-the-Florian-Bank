using UnityEngine;

public class PickPoket : MonoBehaviour
{
    [SerializeField] private float pickPocketDistance;

    private GameObject securityGuard;

    public bool basement1Key;
    public bool basement2Key;

    void Update()
    {
        if (securityGuard != null && securityGuard.GetComponent<SecurityOfficerScript>().hasKey)
        {
            //Show button icon
            if (Input.GetKeyDown(KeyCode.C))
            {
                securityGuard.GetComponent<SecurityOfficerScript>().key.transform.SetParent(gameObject.transform);
                securityGuard.GetComponent<SecurityOfficerScript>().key.transform.position = transform.position;

                if (securityGuard.GetComponent<SecurityOfficerScript>().keyName == "Basement 1")
                {
                    basement1Key = true;
                    basement2Key = false;
                }
                else if (securityGuard.GetComponent<SecurityOfficerScript>().keyName == "Basement 2")
                {
                    basement1Key = false;
                    basement2Key = true;
                }

                securityGuard.GetComponent<SecurityOfficerScript>().hasKey = false;
            }
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
            securityGuard = null;
        }
    }
}
