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
        StartCoroutine(FadeInAudio());
        if (scoreTxt == null){
            scoreTxt = GameObject.FindWithTag("Score").GetComponent<Text>();
        }
        if(PlayerPrefs.HasKey("HighScore")){
            scoreTxt.text = "Your highest level: " + PlayerPrefs.GetInt("HighScore");
        }else{
            scoreTxt.text = "Play to get a new High Score!";
        }

    }

    public void PlayGame(){
        StartCoroutine(FadeOutAudioAndStart());
    }

    public void ResetScore(){
        PlayerPrefs.DeleteKey("HighScore");
        scoreTxt.text = "Play to get a new High Score!";
    }

    public void QuitGame(){
        Debug.Log("Quitting boiz");
        Application.Quit();
    }

    IEnumerator FadeInAudio()
    {
        // Find Audio Music in scene
        AudioSource audioMusic = GameObject.Find("MenuAudio").GetComponent<AudioSource>();

        // Check Music Volume and Fade Out
        while (audioMusic.volume < 0.5f)
        {
            audioMusic.volume += Time.deltaTime / 5;
            yield return null;
        }

    }

    IEnumerator FadeOutAudioAndStart()
    {
        // Find Audio Music in scene
        AudioSource audioMusic = GameObject.Find("MenuAudio").GetComponent<AudioSource>();

        // Check Music Volume and Fade Out
        while (audioMusic.volume > 0.01f)
        {
            audioMusic.volume -= Time.deltaTime / 3;
            yield return null;
        }

        // Make sure volume is set to 0
        audioMusic.volume = 0;

        // Stop Music
        audioMusic.Stop();

        // Destroy
        Destroy(this);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
