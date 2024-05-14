using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameRestartMenu : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject gameOverMenu;

    [Header("GameOver Menu Buttons")]
    public Button restartButton;
    public Button quitButton;

    public List<Button> returnButtons;

     [Header("Data Display")]
    public TMP_Text enemiesEliminatedText;
    public TMP_Text civiliansEliminatedText;
    public TMP_Text shotsFiredText;
    public TMP_Text timeElapsedText;
    // Start is called before the first frame update
    void Start()
    {
        EnableGameOverMenu();

        //Hook events
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);

        DisplayCollectedData();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        HideAll();
        ClearPlayerPrefsData(); // Limpia los datos almacenados en PlayerPrefs
        SceneTransitionManager.singleton.GoToSceneAsync(1);
    }

    public void HideAll()
    {
        gameOverMenu.SetActive(false);
    }

    public void EnableGameOverMenu()
    {
        gameOverMenu.SetActive(true);
    }

    private void DisplayCollectedData()
    {
       // Retrieve collected data from PlayerPrefs or wherever it's stored
        int enemiesEliminated = PlayerPrefs.GetInt("Enemy_Dead_Count", 0);
        int civiliansEliminated = PlayerPrefs.GetInt("Civil_Dead_Count", 0);
        int shotsFired = PlayerPrefs.GetInt("Shots_Fired_Pistol", 0);
        float timeElapsed = PlayerPrefs.GetFloat("Elapsed_Time", 0f);

        // Update UI elements with collected data
        enemiesEliminatedText.text = "Enemies Eliminated: " + enemiesEliminated;
        civiliansEliminatedText.text = "Civilians Eliminated: " + civiliansEliminated;
        shotsFiredText.text = "Shots Fired: " + shotsFired;
        timeElapsedText.text = "Time Elapsed: " + timeElapsed.ToString("F2") + "s";
    }

    public void ClearPlayerPrefsData()
{
    PlayerPrefs.DeleteKey("EnemiesEliminated");
    PlayerPrefs.DeleteKey("CiviliansEliminated");
    PlayerPrefs.DeleteKey("ShotsFired");
    PlayerPrefs.DeleteKey("TimeElapsed");
}

}
