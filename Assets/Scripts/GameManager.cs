using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private ParticleSystem completionParticleSystem;
    [SerializeField] private TMP_Dropdown sizeDropdown;

    // Fill-in-the-blank question elements
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button submitButton2;
    [SerializeField] private TextMeshProUGUI feedbackText;

    private List<Transform> pieces;
    private int emptyLocation;
    private int size;
    private bool shuffling = false;
    private bool puzzleCompleted = false;
    private AudioSource audioSource;
    private float startTime;

    private Transform selectedPiece;

    private void CreateGamePieces(float gapThickness)
    {
        float width = 1 / (float)size;
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameTransform);
                pieces.Add(piece);
                piece.localPosition = new Vector3(-1 + (2 * width * col) + width,
                                                  +1 - (2 * width * row) - width,
                                                  0);
                piece.localScale = ((2 * width) - gapThickness) * Vector3.one;
                piece.name = $"{(row * size) + col}";
                if ((row == size - 1) && (col == size - 1))
                {
                    emptyLocation = (size * size) - 1;
                    piece.gameObject.SetActive(false);
                }
                else
                {
                    float gap = gapThickness / 2;
                    Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                    Vector2[] uv = new Vector2[4];
                    uv[0] = new Vector2((width * col) + gap, 1 - ((width * (row + 1)) - gap));
                    uv[1] = new Vector2((width * (col + 1)) - gap, 1 - ((width * (row + 1)) - gap));
                    uv[2] = new Vector2((width * col) + gap, 1 - ((width * row) + gap));
                    uv[3] = new Vector2((width * (col + 1)) - gap, 1 - ((width * row) + gap));
                    mesh.uv = uv;
                }
            }
        }
    }

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        pieces = new List<Transform>();
        sizeDropdown.onValueChanged.AddListener(delegate { OnSizeChanged(); });
        size = 2; // Default size
        CreateGamePieces(0.01f);

        startTime = Time.time;

        if (!shuffling && CheckCompletion())
        {
            shuffling = true;

            StartCoroutine(WaitShuffle(1f));
        }

        // Set up the fill-in-the-blank question
        questionText.text = "What is the puzzle returning?";
        submitButton.onClick.AddListener(CheckAnswer);
        submitButton2.onClick.AddListener(CheckAnswer2);
    }

    void Update()
    {
        if (!shuffling && !puzzleCompleted && CheckCompletion())
        {
            puzzleCompleted = true;
            ShowCompletionMessage();
            SaveCompletionTimeToCSV();
        }

        if (!puzzleCompleted)
        {
            float t = Time.time - startTime;
            string minutes = ((int)t / 60).ToString();
            string seconds = (t % 60).ToString("f2");
            timerText.text = minutes + ":" + seconds;
        }

        if (Input.GetMouseButtonDown(0) && !puzzleCompleted)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit)
            {
                if (selectedPiece != null)
                {
                    selectedPiece.GetComponent<Renderer>().material.color = Color.white;
                }

                selectedPiece = hit.transform;
                selectedPiece.GetComponent<Renderer>().material.color = Color.red;

                for (int i = 0; i < pieces.Count; i++)
                {
                    if (pieces[i] == hit.transform)
                    {
                        if (SwapIfValid(i, -size, size)) { break; }
                        if (SwapIfValid(i, +size, size)) { break; }
                        if (SwapIfValid(i, -1, 0)) { break; }
                        if (SwapIfValid(i, +1, size - 1)) { break; }
                    }
                }
            }
        }
    }

    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        if (((i % size) != colCheck) && ((i + offset) == emptyLocation))
        {
            audioSource.PlayOneShot(moveSound);

            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);
            (pieces[i].localPosition, pieces[i + offset].localPosition) = (pieces[i + offset].localPosition, pieces[i].localPosition);

            emptyLocation = i;
            return true;
        }
        return false;
    }

    private bool CheckCompletion()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].name != $"{i}")
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator WaitShuffle(float duration)
    {
        yield return new WaitForSeconds(duration);
        Shuffle();
        shuffling = false;
    }

    private void Shuffle()
    {
        int count = 0;
        int last = 0;
        while (count < (size * size * size))
        {
            int rnd = Random.Range(0, size * size);
            if (rnd == last) { continue; }
            last = emptyLocation;
            if (SwapIfValid(rnd, -size, size))
            {
                count++;
            }
            else if (SwapIfValid(rnd, +size, size))
            {
                count++;
            }
            else if (SwapIfValid(rnd, -1, 0))
            {
                count++;
            }
            else if (SwapIfValid(rnd, +1, size - 1))
            {
                count++;
            }
        }
    }

    private void ShowCompletionMessage()
    {
        float t = Time.time - startTime;
        string minutes = ((int)t / 60).ToString();
        string seconds = (t % 60).ToString("f2");
        //score = Mathf.Max(0, (int)(1000 - t)); // Example score calculation

        timerText.text = $"You took {minutes} minutes and {seconds} seconds.";

        Debug.Log("Well Done!");

        completionParticleSystem.Play();
    }

    private void SaveCompletionTimeToCSV()
    {
        float t = Time.time - startTime;
        string minutes = ((int)t / 60).ToString();
        string seconds = (t % 60).ToString("f2");
        string timeTaken = minutes + " minutes and " + seconds + " seconds";

        string filePath = Path.Combine(Application.dataPath, "PuzzleCompletionTimes.csv");
        bool fileExists = File.Exists(filePath);

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            if (!fileExists)
            {
                writer.WriteLine("Completion Time");
            }
            writer.WriteLine(timeTaken);
        }

        Debug.Log("Completion time saved to CSV.");
    }

    private void OnSizeChanged()
    {
        size = int.Parse(sizeDropdown.options[sizeDropdown.value].text);
        ResetGame();
    }

    private void ResetGame()
    {
        foreach (Transform piece in pieces)
        {
            Destroy(piece.gameObject);
        }
        pieces.Clear();
        CreateGamePieces(0.01f);
        shuffling = true;
        StartCoroutine(WaitShuffle(1f));
        puzzleCompleted = false;
        startTime = Time.time;
    }

    private void CheckAnswer()
    {
        string correctAnswer = "result";
        if (answerInput.text.Trim().Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase))
        {
            feedbackText.text = "Correct!";
            feedbackText.color = Color.green;
        }
        else
        {
            feedbackText.text = "Incorrect, try again.";
            feedbackText.color = Color.red;
        }
    }

    private void CheckAnswer2()
    {
        string correctAnswer = "true";
        if (answerInput.text.Trim().Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase))
        {
            feedbackText.text = "Correct!";
            feedbackText.color = Color.green;
        }
        else
        {
            feedbackText.text = "Incorrect, try again.";
            feedbackText.color = Color.red;
        }
    }


}
