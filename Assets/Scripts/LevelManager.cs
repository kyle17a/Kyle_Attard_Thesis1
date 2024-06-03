using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class LevelManager : MonoBehaviour
{
    public List<GameObject> Levels;
    [SerializeField] private int _level;
    [SerializeField] private GameObject SubmitButton;
    [SerializeField] private Button skipButton;
    private List<List<string>> correctAnswers = new List<List<string>>();
    public TextMeshProUGUI answerOutput;
    public Image backgroundImage;
    private List<string> outputs = new List<string>();
    public TextMeshProUGUI levelIndicator;
    public Image introImage;
    public TextMeshProUGUI introText;
    public TextMeshProUGUI secondTopic;

    // Second Topic Variables
    public Button choiceButton1;
    public Button choiceButton2;

    private bool[] correctButtonForLevel = { true, false, true, false, true };
    private int currentLevel = 0; // Track the current level

    public int points = 0;
    public TextMeshProUGUI pointsDisplay;

    public ParticleSystem correctAnswerEffect;
    public ParticleSystem wrongAnswerEffect;

    private List<string> hints = new List<string>();
    public Button hintButton;

    // Skip tracking
    private int skipsRemaining = 2; // Track number of skips remaining

    // Time and attempts tracking
    private List<float> levelTimes = new List<float>();
    private List<int> wrongAttempts = new List<int>();
    private List<string> skippedLevels = new List<string>(); // Track skipped levels
    private float levelStartTime;
    private int currentWrongAttempts = 0;

    private void Start()
    {
        StartCoroutine(StartIntroSequence());
        choiceButton1.onClick.AddListener(() => ButtonClicked(true));
        choiceButton2.onClick.AddListener(() => ButtonClicked(false));
        pointsDisplay.text = "Points: " + points;
        hintButton.onClick.AddListener(ShowHint);
        skipButton.onClick.AddListener(SkipLevel);
        InitializeHints();
        UpdateSkipButton();
    }

    void InitializeHints()
    {
        hints.Add("Use int to declare a variable");
        hints.Add("Use int to declare a variable to add num1 and num2");
        hints.Add("Check if age is greater than 18");
        hints.Add("Initialize an array with new int[size]");
        hints.Add("Initialize the array with values directly."); // New hint for the array question
    }

    void ShowHint()
    {
        answerOutput.text = hints[_level - 1];
    }

    void ButtonClicked(bool isButton1Clicked)
    {
        bool isCorrect = (isButton1Clicked == correctButtonForLevel[currentLevel]);

        if (isCorrect)
        {
            Debug.Log("Correct choice!");
            correctAnswerEffect.Play();
            points += 10; // Add points for correct answer
            pointsDisplay.text = "Points: " + points;
        }
        else
        {
            Debug.Log("Wrong choice, try again!");
            wrongAnswerEffect.Play();
            answerOutput.text = "Incorrect, try again!";
            currentWrongAttempts++; // Increment the wrong attempts count
        }

        StartCoroutine(DelayedLevelChange(false)); // Pass false to indicate level was not skipped
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
        levelStartTime = Time.time; // Record the start time of the level
        currentWrongAttempts = 0; // Reset the wrong attempts count for the new level

        if (level == 4) // New level for the array question
        {
            choiceButton1.GetComponentInChildren<TextMeshProUGUI>().text = "<color=#0000ff>int[]</color> numbers = {1, 2, 3, 4, 5};";
            choiceButton2.GetComponentInChildren<TextMeshProUGUI>().text = "<color=#0000ff>int[]</color> numbers = new int[5];";
        }
    }

    void UpdateLevelIndicator(int level)
    {
        levelIndicator.text = "Level " + level + " of " + Levels.Count; // Updates the text to show the current level
    }

    void InitializeAnswers()
    {
        // Example: Initialize correct answers for each level
        correctAnswers.Add(new List<string> { "int", "10" }); // Answers for level 1
        correctAnswers.Add(new List<string> { "int", "5", "int", "10", "int", "num1", "num2" }); // Answers for level 2
        correctAnswers.Add(new List<string> { "int", "20", "age", "18" }); // Answers for level 3
        correctAnswers.Add(new List<string> { "void", "Hello" }); // Answers for level 4
        correctAnswers.Add(new List<string> { "int[] numbers = {1, 2, 3, 4, 5};", "int[] numbers = new int[5];" }); // New answers for the array question
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
            correctAnswerEffect.Play();
            points += 10; // Add points for correct answer
            pointsDisplay.text = "Points: " + points;
            Color greenWithAlpha = new Color(0, 1, 0, 0.5f); // Green with 50% opacity
            backgroundImage.color = greenWithAlpha;
            levelTimes.Add(Time.time - levelStartTime); // Record the time taken to complete the level
            wrongAttempts.Add(currentWrongAttempts); // Record the number of wrong attempts
            skippedLevels.Add("No"); // Mark this level as not skipped

            if (_level == Levels.Count) // Assuming the last level of the first set is the last item in the Levels list
            {
                SaveLevelTimesToCSV(); // Save the times to a CSV file
                EndFirstSetOfQuestions(); // Call this when the last question of the first set is correctly answered
            }
            else
            {
                StartCoroutine(DelayedLevelChange(false)); // Pass false to indicate level was not skipped
            }
        }
        else
        {
            wrongAnswerEffect.Play();
            answerOutput.text = "Incorrect, try again!";
            currentWrongAttempts++; // Increment the wrong attempts count
        }
    }

    IEnumerator DelayedLevelChange(bool skipped)
    {
        yield return new WaitForSeconds(3); // Wait for 3 seconds
        Color colorWithZeroAlpha = new Color(0, 0, 0, 0);
        backgroundImage.color = colorWithZeroAlpha;
        answerOutput.text = "";
        if (skipped)
        {
            skippedLevels.Add("Yes"); // Mark this level as skipped
        }
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

    void SkipLevel()
    {
        if (skipsRemaining > 0)
        {
            levelTimes.Add(Time.time - levelStartTime); // Record the time taken to complete the level
            wrongAttempts.Add(currentWrongAttempts); // Record the number of wrong attempts
            skipsRemaining--; // Decrement the skip counter
            UpdateSkipButton(); // Update the skip button state
            StartCoroutine(DelayedLevelChange(true)); // Skip to the next level and pass true to indicate it was skipped
        }
    }

    void UpdateSkipButton()
    {
        skipButton.interactable = (skipsRemaining > 0);
        skipButton.GetComponentInChildren<TextMeshProUGUI>().text = "Skip (" + skipsRemaining + " left)";
    }

    void SaveLevelTimesToCSV()
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        string filePath = Application.dataPath + "/LevelTimes_" + timestamp + ".csv";
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Level,TimeTaken,WrongAttempts,PointsEarned,CompletionTime,Skipped,SessionID");
            for (int i = 0; i < levelTimes.Count; i++)
            {
                string completionTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                writer.WriteLine((i + 1) + "," + levelTimes[i] + "," + wrongAttempts[i] + "," + ((i + 1) * 10) + "," + completionTime + "," + skippedLevels[i] + "," + timestamp);
            }
        }
        Debug.Log("Level times saved to " + filePath);
    }
}
