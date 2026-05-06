using System.Collections;
using UnityEngine;

public class HiddingTable : MonoBehaviour
{
    [SerializeField] private GameObject instructionKey;
    [SerializeField] private Transform hidePoint;
    [HideInInspector] public float alignDuration = 0.18f;

    private GameObject player;
    private bool isNear = false;
    [HideInInspector] public bool isHidden = false;
    private bool isAligning = false;

    private Vector3 originalScale;
    private Quaternion originalRotation;

    private bool onceCaptured = false;

    private Coroutine alignRoutine;

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (Input.GetKeyDown(KeyCode.Z) && isNear && !isHidden && !isAligning)
        {
            if (alignRoutine != null) StopCoroutine(alignRoutine);
            alignRoutine = StartCoroutine(EnterHideRoutine());
        }
        else if (Input.GetKeyDown(KeyCode.Z) && isHidden && !isAligning)
        {
            ExitHide();
        }
    }

    private IEnumerator EnterHideRoutine()
    {
        if (player == null) yield break;

        isAligning = true;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        CapsuleCollider2D col = player.GetComponent<CapsuleCollider2D>();
        Animator anim = player.GetComponent<Animator>();
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        PickPoket pp = player.GetComponent<PickPoket>();

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        col.enabled = false;

        anim.SetBool("Walk", false);

        if (pm != null) pm.enabled = false;
        if (pp != null) pp.enabled = false;

        Vector3 startPos = player.transform.position;

        Vector3 endPos;
        if (hidePoint != null)
            endPos = new Vector3(hidePoint.position.x, hidePoint.position.y, startPos.z);
        else
            endPos = new Vector3(transform.position.x, transform.position.y, startPos.z);

        float t = 0f;
        float dur = Mathf.Max(0.0001f, alignDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float e = Smooth01(t);
            player.transform.position = Vector3.Lerp(startPos, endPos, e);
            yield return null;
        }

        player.transform.position = endPos;

        player.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        anim.SetBool("HideTable", true);

        if (instructionKey != null) instructionKey.SetActive(true);

        isHidden = true;
        isAligning = false;
        alignRoutine = null;
    }

    private void ExitHide()
    {
        if (player == null) return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        CapsuleCollider2D col = player.GetComponent<CapsuleCollider2D>();
        Animator anim = player.GetComponent<Animator>();
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        PickPoket pp = player.GetComponent<PickPoket>();

        col.enabled = true;
        rb.gravityScale = 1f;

        if (pm != null) pm.enabled = true;
        if (pp != null) pp.enabled = true;

        player.transform.localScale = originalScale;

        anim.SetBool("HideTable", false);

        isHidden = false;
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

            if (instructionKey != null) instructionKey.SetActive(true);
            isNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (instructionKey != null) instructionKey.SetActive(false);
            isNear = false;
        }
    }

    private float Smooth01(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    public void removeHiding()
    {
        ExitHide();
    }
}
