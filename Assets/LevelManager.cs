using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    private void Start()
    {
        InitializeAnswers(); // Initialize the correct answers for each level
        _level = 1; // Start at level 1
        ChangeLevel(_level); // Set the initial level active
        SubmitButton = GameObject.Find("Submit-Btn"); // Find the submit button
        SubmitButton.GetComponent<Button>().onClick.AddListener(CheckAnswer); // Add listener for the submit button

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
        correctAnswers.Add(new List<string> { "Debug" }); // Answers for level 1
        correctAnswers.Add(new List<string> { "Log" }); // Answers for level 2
        correctAnswers.Add(new List<string> { "Math" }); // Answers for level 3
        correctAnswers.Add(new List<string> { "if" }); // Answers for level 4
        correctAnswers.Add(new List<string> { "PlayerPrefs" }); // Answers for level 5
        // Add more as needed for each level

        outputs.Add("\"Hello world!\""); // Output for level 1
        outputs.Add("\"Another output!\""); // Output for level 2
        outputs.Add("\"7\""); // Output for level 3
        outputs.Add("\"You passed the level!\""); // Output for level 4
        outputs.Add("\"true\""); // Output for level 5
    }

    void CheckAnswer()
    {
        GameObject level = Levels[_level - 1];
        TMP_InputField[] fields = level.GetComponentsInChildren<TMP_InputField>(true);
        List<string> levelAnswers = correctAnswers[_level - 1];
        bool allCorrect = true;

        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i].text != levelAnswers[i]) // Check if the input matches the correct answer
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            answerOutput.text = outputs[_level - 1];
            StartCoroutine(DelayedLevelChange()); // Proceed to next level after delay
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
