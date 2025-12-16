using UnityEngine;

public class HiddingTable : MonoBehaviour
{
    [SerializeField] private GameObject instructionKey;

    private GameObject player;
    private bool isNear = false;
    private bool isHidden = false;

    private Vector3 originalScale;
    private Quaternion originalRotation;

    private bool onceCaptured = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z) && isNear && !isHidden)
        {
            player.transform.position = transform.position;
            player.GetComponent<Rigidbody2D>().gravityScale = 0f;
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            player.GetComponent<CapsuleCollider2D>().enabled = false;
            player.GetComponent<Animator>().SetBool("Walk", false);
            player.GetComponent<PlayerMovement>().enabled = false;
            player.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
            player.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            instructionKey.SetActive(true);
            //Color c = player.GetComponent<SpriteRenderer>().color;
            //c.a = 0.45f;
            //player.GetComponent<SpriteRenderer>().color = c;

            isHidden = true;
        }

        else if (Input.GetKeyDown(KeyCode.Z) && isHidden)
        {
            player.GetComponent<CapsuleCollider2D>().enabled = true;
            player.GetComponent<Rigidbody2D>().gravityScale = 1f;
            player.GetComponent<PlayerMovement>().enabled = true;
            player.transform.localScale = originalScale;
            player.transform.localRotation = originalRotation;
            //Color c = player.GetComponent<SpriteRenderer>().color;
            //c.a = 1f;
            //player.GetComponent<SpriteRenderer>().color = c;

            isHidden = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = collision.gameObject;

            if (!onceCaptured)
            {
                originalScale = player.transform.localScale;
                originalRotation = player.transform.localRotation;

                onceCaptured = true;
            }

            instructionKey.SetActive(true);
            isNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            instructionKey.SetActive(false);
            isNear = false;
        }
    }
}
