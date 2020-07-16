using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour {
	public Text score;
	private void Start(){
		score.text = PlayerPrefs.GetInt ("HighScore").ToString();
	}
	public void ToGame(){
		SceneManager.LoadScene ("Game");
	}
}
