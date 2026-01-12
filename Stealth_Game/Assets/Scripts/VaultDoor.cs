using UnityEngine;

public class VaultDoor : MonoBehaviour
{
    [SerializeField] private GameObject vaultPuzzle;
    [SerializeField] private GameObject instructionKey;
    private bool isPlayer = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z) && isPlayer)
        {
            if (vaultPuzzle != null)
            {
                vaultPuzzle.SetActive(true);
                instructionKey.SetActive(false);
                Destroy(gameObject.GetComponent<BoxCollider2D>());
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayer = true;
            instructionKey.SetActive(true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayer = false;
            instructionKey.SetActive(false);
        }
    }
}
