using UnityEngine;

public class Ending : MonoBehaviour
{
    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject instructionKey;

    private bool isNear;

    private void Update()
    {
        if (isNear)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                endScreen.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
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
