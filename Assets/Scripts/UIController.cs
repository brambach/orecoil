using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Text")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI infoText;

    [Header("Shield Pips")]
    public Image pip1;
    public Image pip2;
    public Image pip3;

    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    public void UpdateShield(int pips, int max, bool active)
    {
        if (pip1 != null) pip1.enabled = max >= 1;
        if (pip2 != null) pip2.enabled = max >= 2;
        if (pip3 != null) pip3.enabled = max >= 3;

        if (pip1 != null) pip1.color = pips >= 1 ? Color.white : new Color(1, 1, 1, 0.2f);
        if (pip2 != null) pip2.color = pips >= 2 ? Color.white : new Color(1, 1, 1, 0.2f);
        if (pip3 != null) pip3.color = pips >= 3 ? Color.white : new Color(1, 1, 1, 0.2f);

    }



    public void ShowTitle(bool on)
    {
        if (infoText != null)
        {
            if (on) infoText.text = "Orecoil\nPress Enter to Start\nArrows/WASD to move, Space for Shield";
            infoText.gameObject.SetActive(on);
        }
    }

    public void ShowGameOver(bool on, int finalScore = 0)
    {
        if (infoText != null)
        {
            if (on) infoText.text = $"Game Over\nScore: {finalScore}\nPress Enter to Retry";
            infoText.gameObject.SetActive(on);
        }
    }
}