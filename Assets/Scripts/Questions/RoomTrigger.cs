using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public RoomType roomType;
    public QuizDataSO quizData;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!GameManager.Instance.allCollectiblesCollected)
            return;

        if (!QuizManager.Instance.CanStartQuiz(roomType))
            return;

        QuizManager.Instance.StartQuiz(quizData);
    }
}