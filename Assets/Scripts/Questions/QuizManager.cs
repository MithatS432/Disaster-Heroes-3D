using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;
    public QuestionUI questionUI;
    public System.Action OnAllQuizzesCompleted;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    private QuizDataSO currentQuiz;
    private int currentIndex;

    private HashSet<RoomType> completedRooms = new();
    private bool isAnswering;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool CanStartQuiz(RoomType roomType)
    {
        if (completedRooms.Contains(roomType))
            return false;

        if (!completedRooms.Contains(RoomType.LivingRoom))
            return roomType == RoomType.LivingRoom;

        return true;
    }

    public void StartQuiz(QuizDataSO quiz)
    {
        if (currentQuiz != null) return;

        if (quiz == null || quiz.questions == null || quiz.questions.Length == 0)
            return;

        currentQuiz = quiz;
        currentIndex = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        questionUI.Show();
        ShowQuestion();
    }

    private void ShowQuestion()
    {
        var q = currentQuiz.questions[currentIndex];
        questionUI.SetQuestion(q);

        isAnswering = false;
    }

    public void Answer(int index)
    {
        if (isAnswering)
            return;

        isAnswering = true;

        var q = currentQuiz.questions[currentIndex];

        bool correct = index == q.correctIndex;

        questionUI.ShowAnswerResult(index, q.correctIndex);

        if (correct)
        {
            audioSource.PlayOneShot(correctSound);
        }
        else
        {
            audioSource.PlayOneShot(wrongSound);
        }

        StartCoroutine(NextQuestionCoroutine());
    }

    private IEnumerator NextQuestionCoroutine()
    {
        yield return new WaitForSeconds(2f);

        currentIndex++;

        if (currentIndex >= currentQuiz.questions.Length)
        {
            EndQuiz();
        }
        else
        {
            ShowQuestion();
        }
    }

    private void EndQuiz()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        questionUI.Hide();

        if (currentQuiz != null)
        {
            completedRooms.Add(currentQuiz.roomType);
        }

        currentQuiz = null;

        int totalRooms = System.Enum.GetValues(typeof(RoomType)).Length;

        if (completedRooms.Count >= totalRooms)
        {
            Debug.Log("All quizzes completed → earthquake trigger");
            OnAllQuizzesCompleted?.Invoke();
        }
    }
}