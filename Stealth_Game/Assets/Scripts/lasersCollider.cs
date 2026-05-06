using UnityEngine;

public class lasersCollider : MonoBehaviour
{
    [SerializeField] private GameObject gameOverDisplay;

    private void Start()
    {
        if (gameOverDisplay == null)
        {
            gameOverDisplay = LevelReferences.instance.gameOverDisplay;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (gameOverDisplay) gameOverDisplay.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}
