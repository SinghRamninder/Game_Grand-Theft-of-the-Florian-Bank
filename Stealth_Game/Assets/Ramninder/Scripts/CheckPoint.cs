using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public static CheckPoint instance;

    [SerializeField] private GameObject guardCaughtDisplay;
    [SerializeField] private GameObject timesUpDisplay;
    [SerializeField] private GameObject timerDisplay;
    [SerializeField] private AfterStealMoney stealMoney;
    //[SerializeField] private DoorScript basement1Door;
    //[SerializeField] private DoorScript basement2Door;
    //[SerializeField] private DoorScript groundDoor;
    //[SerializeField] private DoorScript firstDoor;

    private List<DoorScript> allElevators = new List<DoorScript>();

    //[SerializeField] private SecurityOfficerScript firstGuard;
    //[SerializeField] private SecurityOfficerScript basement1Guard;
    //[SerializeField] private SecurityOfficerScript basement2Guard;

    private List<SecurityOfficerScript> allGuards = new List<SecurityOfficerScript>();
    private GameObject player;

    [HideInInspector] public bool moneyStolen;

    private bool checkpoint1;
    private bool checkpoint2;

    private Transform check1Transform;
    private Transform check2Transform;

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

    private void Start()
    {
        if (LevelReferences.instance != null)
        {
            if (guardCaughtDisplay == null)
                guardCaughtDisplay = LevelReferences.instance.gameOverDisplay;
            if (timesUpDisplay == null)
                timesUpDisplay = LevelReferences.instance.timesUpDisplay;
            if (timerDisplay == null)
                timerDisplay = LevelReferences.instance.timerDisplay;
        }

        if (stealMoney == null)
        {
            stealMoney = GameObject.FindFirstObjectByType<AfterStealMoney>();
        }

        if (player == null)
        {
            player = GameObject.FindFirstObjectByType<PlayerMovement>().gameObject;
        }

        allElevators.Clear();
        allGuards.Clear();

        allElevators.AddRange(FindObjectsByType<DoorScript>(FindObjectsSortMode.None));
        allGuards.AddRange(FindObjectsByType<SecurityOfficerScript>(FindObjectsSortMode.None));
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Checkpoint1"))
    //    {
    //        checkpoint1 = true;
    //        check1Transform = collision.transform;
    //    }
    //    if (collision.CompareTag("Checkpoint2"))
    //    {
    //        checkpoint2 = true;
    //        check2Transform = collision.transform;
    //    }
    //}

    public void Restart()
    {
        if (checkpoint1 && !moneyStolen)
        {
            player.transform.position = check1Transform.position;
            guardCaughtDisplay.SetActive(false);
            Time.timeScale = 1f;

            foreach (SecurityOfficerScript guard in allGuards)
            {
                guard.TeleportToStart();
            }
        }
        if (moneyStolen)
        {
            player.transform.position = check2Transform.position;
            timesUpDisplay.SetActive(false);
            guardCaughtDisplay.SetActive(false);
            stealMoney.ResetTimeUpUI();
            stealMoney.playerMovement.enabled = true;
            stealMoney.StartCountdown(60f);
            Time.timeScale = 1f;
            timerDisplay.SetActive(true);
            
            foreach (SecurityOfficerScript guard in allGuards)
            {
                guard.TeleportToStart();
            }

            foreach (DoorScript elevator in allElevators)
            {
                elevator.lockAllDoors();
                elevator.ChangeIsCalled();
            }
        }
    }

    public void checkPoint1Exists(Transform checkpointPos)
    {
        checkpoint1 = true;
        check1Transform = checkpointPos;
    }

    public void checkPoint2Exists(Transform checkpointPos)
    {
        checkpoint2 = true;
        check2Transform = checkpointPos;
    }
}
