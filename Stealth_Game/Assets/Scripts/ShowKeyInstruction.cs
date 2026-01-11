using System.Collections;
using UnityEngine;

public class ShowKeyInstruction : MonoBehaviour
{
    [SerializeField] private GameObject instructionText;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(showandhidetext());
        }
    }

    IEnumerator showandhidetext()
    {
        instructionText.SetActive(true);

        yield return new WaitForSeconds(3f);

        instructionText.SetActive(false);

        Destroy(gameObject);
    }
}
