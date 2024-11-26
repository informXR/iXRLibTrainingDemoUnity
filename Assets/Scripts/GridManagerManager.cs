using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridManagerManager : MonoBehaviour
{
    public List<GridManager> gridManagers; // Assign all GridManager instances in the Inspector
    private int maxScore = 0;
    private int currentScore = 0;
    public TMP_Text ScoreText;

    void Start()
    {
        RandomizeEmptySpaces();
        CalculateMaxScore();
        UpdateScore(0); // Initialize the score display
    }

    private void CalculateMaxScore()
    {
        maxScore = 0;
        foreach (var gridManager in gridManagers)
        {
            maxScore += gridManager.blankSpaces;
        }
    }

    private void RandomizeEmptySpaces()
    {
        foreach (var gridManager in gridManagers)
        {
            gridManager.blankSpaces = Random.Range(1, 3); // Randomize between 1 and 4
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScore(currentScore);
        SoundManager.Instance.PlayDropSound();
    }

    private void UpdateScore(int score)
    {
        // Replace this with UI update logic
        Debug.Log($"Current Score: {score}/{maxScore}");
        ScoreText.text = $"Current Score: {score}/{maxScore}";

        if(score == maxScore){
            SoundManager.Instance.PlayGameCompleteSound();
        }
    }
}
