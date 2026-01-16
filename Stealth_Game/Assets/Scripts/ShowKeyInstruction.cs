using System.Collections;
using UnityEngine;

public class ShowKeyInstruction : MonoBehaviour
{
    [SerializeField] private GameObject instructionText;

    private Coroutine showAndHide;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (showAndHide == null)
                showAndHide = StartCoroutine(showandhidetext());
        }
    }

    IEnumerator showandhidetext()
    {
        instructionText.SetActive(true);

        yield return new WaitForSeconds(4f);

        instructionText.SetActive(false);

        Destroy(gameObject);
    }

    public void stopCouritineCall()
    {
        StopCoroutine(showAndHide);
        instructionText.SetActive(false);
        showAndHide = null;
    }
}
