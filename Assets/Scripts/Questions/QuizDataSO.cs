using UnityEngine;

[CreateAssetMenu(menuName = "Quiz/Quiz Data")]
public class QuizDataSO : ScriptableObject
{
    public RoomType roomType;

    public QuestionSO[] questions; // 3 soru
}