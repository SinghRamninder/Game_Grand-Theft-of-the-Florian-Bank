using UnityEngine;

public class HiddingDesk : MonoBehaviour
{
    [SerializeField] private GameObject instructionKey;

    private GameObject player;
    private bool isNear = false;
    [HideInInspector] public bool isHidden = false;

    private Vector3 originalScale;
    private Quaternion originalRotation;

    private bool onceCaptured = false;

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (Input.GetKeyDown(KeyCode.Z) && isNear && !isHidden)
        {
            player.transform.position = transform.position;
            player.GetComponent<Rigidbody2D>().gravityScale = 0f;
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            player.GetComponent<CapsuleCollider2D>().enabled = false;
            player.GetComponent<Animator>().SetBool("Walk", false);
            player.GetComponent<PlayerMovement>().enabled = false;
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.sortingOrder = 16;
            player.transform.localScale = new Vector3(0.11f, 0.11f, 0.11f);
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
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.sortingOrder = 1;
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

    public void removeHiding()
    {
        player.GetComponent<CapsuleCollider2D>().enabled = true;
        player.GetComponent<Rigidbody2D>().gravityScale = 1f;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.transform.localScale = originalScale;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        //Color c = player.GetComponent<SpriteRenderer>().color;
        //c.a = 1f;
        //player.GetComponent<SpriteRenderer>().color = c;

        isHidden = false;
    }
}
