using UnityEngine;
using System.Collections;

public class ShowLaserInstruction : MonoBehaviour
{
    [SerializeField] private GameObject instructionText;

    private void Start()
    {
        if (instructionText == null)
        {
            var lvlRef = GameObject.FindFirstObjectByType<LevelReferences>();

            if (lvlRef != null)
            {
                instructionText = lvlRef.laserInstructionText;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (instructionText != null)
            {
                StartCoroutine(showandhidetext());
            }
        }
    }

    IEnumerator showandhidetext()
    {
        instructionText.SetActive(true);

        yield return new WaitForSeconds(3f);

        instructionText.SetActive(false);
    }
}
