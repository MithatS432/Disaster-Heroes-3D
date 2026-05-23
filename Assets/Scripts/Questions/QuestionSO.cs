using UnityEngine;

[System.Serializable]
public class QuestionSO
{
    [TextArea]
    public string question;

    public string[] answers; // 4 şık

    public int correctIndex;
}