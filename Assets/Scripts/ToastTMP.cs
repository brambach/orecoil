using UnityEngine;
using TMPro;

public class ToastTMP : MonoBehaviour
{
    public TMP_Text text;
    float timer;

    void Awake()
    {
        if (text == null) text = GetComponent<TMP_Text>();
        if (text != null) text.alpha = 0f;
    }

    public void Show(string message, float seconds)
    {
        if (!text) return;
        text.text = message;
        text.CrossFadeAlpha(1f, 0.08f, false);
        timer = seconds;
    }

    void Update()
    {
        if (!text) return;
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                text.CrossFadeAlpha(0f, 0.25f, false);
            }
        }
    }
}