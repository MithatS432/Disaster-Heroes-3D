using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class QuestionUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questionText;
    public Button[] buttons;
    public GameObject panel;
    public GameObject wrongPanel;


    [Header("Answer Colors")]
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    private Color[] originalColors;

    private void Awake()
    {
        originalColors = new Color[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            originalColors[i] = buttons[i].image.color;
        }

        wrongPanel.SetActive(false);
    }

    public void SetQuestion(QuestionSO question)
    {
        questionText.text = question.question;

        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;

            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.answers[i];

            buttons[i].onClick.RemoveAllListeners();

            buttons[i].onClick.AddListener(() =>
            {
                QuizManager.Instance.Answer(index);
            });

            buttons[i].image.color = originalColors[i];
            buttons[i].interactable = true;
        }
    }

    public void ShowAnswerResult(int selectedIndex, int correctIndex)
    {
        foreach (Button btn in buttons)
        {
            btn.interactable = false;
        }

        buttons[correctIndex].image.color = correctColor;

        if (selectedIndex == correctIndex)
        {
            GameManager.Instance.GetQuestionScore(10);
        }
        else
        {
            buttons[selectedIndex].image.color = wrongColor;

            GameManager.Instance.GetDamage(10f);
            GameManager.Instance.GetQuestionScore(-5);

            StartCoroutine(WrongFlashEffect());
        }
    }

    private IEnumerator WrongFlashEffect()
    {
        wrongPanel.SetActive(true);

        yield return new WaitForSeconds(0.15f);

        wrongPanel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}