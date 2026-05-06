using UnityEngine;

public class LevelReferences : MonoBehaviour
{
    public static LevelReferences instance;

    public GameObject gameOverDisplay;
    public GameObject laserInstructionText;
    public GameObject laserDeactivatedText;
    public GameObject timesUpDisplay;
    public GameObject timerDisplay;

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
