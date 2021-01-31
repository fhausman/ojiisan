using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameManager : MonoBehaviour
{
    [SerializeField]
    private int _score = 0;

    [SerializeField]
    private Furnace[] furnaces = null;

    [SerializeField]
    private TextMeshProUGUI _scoreUi = null;

    [SerializeField]
    private GameObject _gameOver = null;

    private float _timer = 0.0f;

    public float GetTotalHeat()
    {
        float sum = 0.0f;
        foreach (var furnace in furnaces)
        {
            sum += furnace.CurrentHeat;
        }

        return sum / furnaces.Length;
    }

    public float GetNormalizedTotalHeat()
    {

        return (GetTotalHeat() / 100.0f);
    }

    public void OnGameLost()
    {
        _gameOver.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_gameOver.activeSelf)
        {
            if (_timer > 1.0f)
            {
                _score += (int)GetTotalHeat();
                _timer = 0.0f;
            }

            _timer += Time.deltaTime;
            _scoreUi.text = string.Format("{0:00000000}", _score);
        }
        else
        {
            if(!endingGame && Input.anyKey)
            {
                endingGame = true;
                StartCoroutine(RestartGame());
            }
        }
    }

    private bool endingGame = false;
    IEnumerator RestartGame()
    {
        _gameOver.SendMessage("PlayEnd");

        yield return new WaitForSeconds(0.78f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
