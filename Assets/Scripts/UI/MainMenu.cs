using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Text scoreTxt;
    void Start()
    {
        if(scoreTxt == null){
            scoreTxt = GameObject.FindWithTag("Score").GetComponent<Text>();
        }
        if(PlayerPrefs.HasKey("HighScore")){
            scoreTxt.text = "Current Highest Level " + PlayerPrefs.GetInt("HighScore");
        }else{
            scoreTxt.text = "Play to get a new High Score!";
        }

    }

    public void PlayGame(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ResetScore(){
        PlayerPrefs.DeleteKey("HighScore");
        scoreTxt.text = "Play to get a new High Score!";
    }

    public void QuitGame(){
        Debug.Log("Quitting boiz");
        Application.Quit();
    }
}
