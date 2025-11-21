using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] DataContainer dataContainer;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private float incrementInterval = 0.2f;

    int score = 0;
    bool isGameOver = false;
    Coroutine scoreRoutine;

    void Start()
    {
        UpdateScoreText();
        scoreRoutine = StartCoroutine(IncrementScore());
    }

    IEnumerator IncrementScore()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(incrementInterval);

            score++;

            UpdateScoreText();
        }
    }

    void UpdateScoreText()
    {
        if (scoreText == null) return;

        scoreText.text = "Score : " + score;
    }

    public void GameOver()
    {
        isGameOver = true;
        if (scoreRoutine != null) StopCoroutine(scoreRoutine);
        
        if (dataContainer != null)
        {
            // Update score jika lebih tinggi
            if (score > dataContainer.score)
            {
                dataContainer.UpdateScore(score);
            }
            
            // Update coin
            dataContainer.UpdateCoin(dataContainer.coin + score);
        }
    }
}
