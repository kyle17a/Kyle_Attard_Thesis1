using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public List<GameObject> Levels;
    [SerializeField] private int _level;
    [SerializeField] private GameObject SubmitButton;
    private List<List<string>> correctAnswers = new List<List<string>>();
    public TextMeshProUGUI answerOutput;
    private List<string> outputs = new List<string>();
    public TextMeshProUGUI levelIndicator;
    public Image introImage;
    public TextMeshProUGUI introText;
    public TextMeshProUGUI secondTopic;

    //second Topic Variables
    public Button choiceButton1;
    public Button choiceButton2;

    private bool[] correctButtonForLevel = { true, false };
    private int currentLevel = 0; // Track the current level



    private void Start()
    {
        StartCoroutine(StartIntroSequence());
        choiceButton1.onClick.AddListener(() => ButtonClicked(true));
        choiceButton2.onClick.AddListener(() => ButtonClicked(false));

    }

    void ButtonClicked(bool isButton1Clicked)
    {
        bool isCorrect = (isButton1Clicked == correctButtonForLevel[currentLevel]);

        if (isCorrect)
        {
            Debug.Log("Correct choice!");
            // Proceed to next level or handle victory
        }
        else
        {
            Debug.Log("Wrong choice, try again!");
            // Optionally, you can allow retrying or handle a game over scenario
        }

        StartCoroutine(DelayedLevelChange());
    }

    IEnumerator StartIntroSequence()
    {
        introImage.gameObject.SetActive(true); // Ensure the intro image is visible
        introText.gameObject.SetActive(true);  // Ensure the intro text is visible

        yield return new WaitForSeconds(3); // Wait for 3 seconds

        introImage.gameObject.SetActive(false); // Hide the intro image
        introText.gameObject.SetActive(false);  // Hide the intro text

        InitializeAnswers(); // Start initializing answers and questions after intro
        _level = 1;
        ChangeLevel(_level); // Start with the first question
        // More setup code as necessary...
    }

    void EndFirstSetOfQuestions()
    {
        introImage.gameObject.SetActive(true); // Reuse the introImage for the transition screen
        introText.gameObject.SetActive(false);  // Hide the initial introText
        secondTopic.gameObject.SetActive(true);  // Show new transition text
        secondTopic.text = "Great job! Get ready for the next Topic!";

        StartCoroutine(TransitionToSecondSet());
    }

    IEnumerator TransitionToSecondSet()
    {
        yield return new WaitForSeconds(3); // Wait for 3 seconds on the transition screen

        // Load the new scene by name or index
        SceneManager.LoadScene(2); // Replace "NameOfYourSecondScene" with the actual scene name
    }


    void ChangeLevel(int level)
    {
        foreach (var item in Levels)
        {
            item.gameObject.SetActive(false);
        }

        Levels[level - 1].SetActive(true); // Activate the current level
        UpdateLevelIndicator(level);
    }

    void UpdateLevelIndicator(int level)
    {
        levelIndicator.text = "Level " + level + " of " + Levels.Count; // Updates the text to show the current level
    }

    void InitializeAnswers()
    {
        // Example: Initialize correct answers for each level
        correctAnswers.Add(new List<string> { "int", "10" }); // Answers for level 1
        correctAnswers.Add(new List<string> { "int", "5", "int", "10", "int", "num1", "num2", }); // Answers for level 2
        correctAnswers.Add(new List<string> { "int","20","age","18" }); // Answers for level 3
        correctAnswers.Add(new List<string> { "if" }); // Answers for level 4
        correctAnswers.Add(new List<string> { "int","int","5","0","10" }); // Answers for level 5
        // Add more as needed for each level

        outputs.Add("\"Hello world!\""); // Output for level 1
        outputs.Add("\"Another output!\""); // Output for level 2
        outputs.Add("\"You are an adult\""); // Output for level 3
        outputs.Add("\"You passed the level!\""); // Output for level 4
        outputs.Add("\"true\""); // Output for level 5
    }

    public void CheckAnswer()
    {
        GameObject level = Levels[_level - 1];
        TMP_InputField[] fields = level.GetComponentsInChildren<TMP_InputField>();
        List<string> levelAnswers = correctAnswers[_level - 1];
        bool allCorrect = true;

        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i].text != levelAnswers[i])
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            answerOutput.text = outputs[_level - 1];
            if (_level == Levels.Count) // Assuming the last level of the first set is the last item in the Levels list
            {
                EndFirstSetOfQuestions(); // Call this when the last question of the first set is correctly answered
            }
            else
            {
                StartCoroutine(DelayedLevelChange()); // Proceed to the next level after a delay
            }
        }
        else
        {
            answerOutput.text = "Incorrect, try again!";
        }
    }



    IEnumerator DelayedLevelChange()
    {
        yield return new WaitForSeconds(3); // Wait for 5 seconds
        answerOutput.text = "";
        int nextLevel = _level + 1; // Calculate the next level index
        if (nextLevel <= Levels.Count) // Check if there are more levels
        {
            ChangeLevel(nextLevel); // Change to the next level
            _level = nextLevel; // Update the current level index
        }
        else
        {
            answerOutput.text = "Congratulations, all levels completed!"; // Display completion message
            Debug.Log("No More Levels");
        }
    }
}
