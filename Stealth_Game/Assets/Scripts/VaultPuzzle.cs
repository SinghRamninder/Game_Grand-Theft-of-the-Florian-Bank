using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class VaultPuzzle : MonoBehaviour
{
    [SerializeField] private float normalSpeed = 5f;
    [SerializeField] private float speedIncrement = 5f;

    [SerializeField] private TMP_Text numbersDisplay;

    [SerializeField] private Image indicationArrow;

    private float speed;

    private float angle;

    private List<int> randomNumbers = new List<int>();

    private int currentNumber;
    private int roundNumber;
    private bool wrongSelection;

    private void Start()
    {
        GenerateNumbers();
        speed = normalSpeed;
    }

    void Update()
    {
        angle += speed * Time.deltaTime;
        angle = angle % 360;

        int calculatedNumber = Mathf.RoundToInt(angle / 36f);
        calculatedNumber = ((calculatedNumber % 10) + 10) % 10;

        if (calculatedNumber == currentNumber && !wrongSelection)
        {
            indicationArrow.color = Color.green;
        }
        else if (!wrongSelection)
        {
            indicationArrow.color = Color.black;
        }

        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (roundNumber < 4)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (calculatedNumber == currentNumber)
                {
                    roundNumber += 1;
                    if (roundNumber == 4) return;

                    speed = -(Mathf.Abs(speed) + speedIncrement) * Mathf.Sign(speed);
                    currentNumber = randomNumbers[roundNumber - 1];

                }
                else
                {
                    wrongSelection = true;
                    numbersDisplay.text = string.Empty;
                    StartCoroutine(WrongPress());
                }
            }
        }
        else
        {
            StartCoroutine(PuzzleCleared());
        }
        
    }

    private void GenerateNumbers()
    {
        randomNumbers.Clear();
        numbersDisplay.text = string.Empty;

        for (int i = 0; i < 3; i++)
        {
            int generatedNumber = Random.Range(0, 10);
            numbersDisplay.text += "\u00A0" + generatedNumber;
            randomNumbers.Add(generatedNumber);
        }

        currentNumber = randomNumbers[0];
        roundNumber = 1;
    }

    private IEnumerator WrongPress()
    {
        speed = -(Mathf.Abs(speed) + 500f) * Mathf.Sign(speed);

        float elapsed = 0f;
        while (elapsed < 3f)
        {
            float t = Mathf.PingPong(Time.time * speed, 1f);
            indicationArrow.color = Color.Lerp(Color.red, Color.black, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        indicationArrow.color = Color.black;
        wrongSelection = false;
        speed = normalSpeed;
        GenerateNumbers();
    }

    private IEnumerator PuzzleCleared()
    {
        speed = 0f;

        yield return new WaitForSeconds(2f);

        //sound
        gameObject.SetActive(false);
    }
}
