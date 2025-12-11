using UnityEngine;

public class HiddingMechanics : MonoBehaviour
{
    private GameObject player;
    private bool isNear = false;
    private bool isHidden = false;

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
            isNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isNear = false;
        }
    }
}
