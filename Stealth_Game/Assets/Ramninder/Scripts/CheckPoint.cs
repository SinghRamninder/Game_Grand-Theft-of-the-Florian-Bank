using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private GameObject guardCaughtDisplay;
    [SerializeField] private GameObject timesUpDisplay;
    [SerializeField] private StealMoney countdownTime;
    [SerializeField] private DoorScript basement1Door;
    [SerializeField] private DoorScript basement2Door;
    [SerializeField] private DoorScript groundDoor;
    [SerializeField] private DoorScript firstDoor;

    [SerializeField] private SecurityOfficerScript firstGuard;
    [SerializeField] private SecurityOfficerScript basement1Guard;
    [SerializeField] private SecurityOfficerScript basement2Guard;

    public bool moneyStolen;

    private bool checkpoint1;
    private bool checkpoint2;

    private Transform check1Transform;
    private Transform check2Transform;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Checkpoint1"))
        {
            checkpoint1 = true;
            check1Transform = collision.transform;
        }
        if (collision.CompareTag("Checkpoint2"))
        {
            checkpoint2 = true;
            check2Transform = collision.transform;
        }
    }

    public void Restart()
    {
        if (checkpoint1 && !moneyStolen)
        {
            transform.position = check1Transform.position;
            guardCaughtDisplay.SetActive(false);
            Time.timeScale = 1f;
            firstGuard.ForceStopChaseToPatrol();
            basement1Guard.ForceStopChaseToPatrol();
            basement2Guard.ForceStopChaseToPatrol();
        }
        if (moneyStolen)
        {
            transform.position = check2Transform.position;
            timesUpDisplay.SetActive(false);
            guardCaughtDisplay.SetActive(false);
            countdownTime.countdownSeconds = 60f;
            Time.timeScale = 1f;
            firstGuard.ForceStopChaseToPatrol();
            basement1Guard.ForceStopChaseToPatrol();
            basement2Guard.ForceStopChaseToPatrol();
            basement1Door.lockAllDoors();
            basement2Door.lockAllDoors();
            groundDoor.lockAllDoors();
            firstDoor.lockAllDoors();
        }
    }
}
