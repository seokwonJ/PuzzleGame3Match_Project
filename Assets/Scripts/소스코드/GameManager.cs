using UnityEngine;
using UnityEngine.InputSystem.XInput;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 🔹 전역 접근용 인스턴스
    public static GameManager Instance { get; private set; }

    // 🔹 게임 전체에서 관리할 공용 변수들 (예시)
    public int score;
    public int moveCount;
    public bool isPaused;
    public UIManager uiManager;

    private void Awake()
    {
        // 이미 인스턴스가 존재하면 새로 생긴 객체는 파괴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 인스턴스 등록
        Instance = this;
    }

    private void Start()
    {
        score = 15;
        moveCount = 10;
        uiManager.moveCount.text = moveCount.ToString();
        uiManager.crownScore.text = score.ToString();
    }

    // 🔹 예시 함수
    public void AddScore(int value)
    {
        score -= value;
        uiManager.crownScore.text = score.ToString();
        if (score <= 0)
        {
            SetClear();
            return;
        }
        Debug.Log($"[GameManager] Score: {score}");
    }

    // 🔹 예시 함수
    public void AddMoveCount(int value)
    {

        moveCount -= value;
        uiManager.moveCount.text = moveCount.ToString();
        if (moveCount <= 0 && score != 0)
        {
            SetGameOver();
            return;
        }
        Debug.Log($"[GameManager] Score: {score}");
    }

    public void SetClear()
    {
        isPaused = true;
        uiManager.endingCanvas.SetActive(true);
        uiManager.endingCanvas.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void SetGameOver()
    {
        isPaused = true;
        uiManager.endingCanvas.SetActive(true);
        uiManager.endingCanvas.transform.GetChild(1).gameObject.SetActive(true);
    }

    public void ReStart()
    {
        SceneManager.LoadScene(0);
    }
}