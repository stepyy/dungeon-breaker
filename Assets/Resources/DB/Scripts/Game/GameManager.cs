/// <summary>
/// Game manager.
/// will collected a player score and try to read an event 
/// what happened in the game and how to do next  
/// </summary>

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GUISkin Skin;
    public Texture2D IconZombie;

    [System.NonSerialized]
    public int Score = 0;
    [System.NonSerialized]
    public bool Playing;
	
	
	void Start()
	{
        Playing = true;
	}

	void Update()
	{
		
	}
	
	// Read message from Game Object and making an event.
	public void GameEvent(string message){
		switch(message){
            case "endgame":
                Time.timeScale = 0;
                Playing = false;
                break;
		}
	}
	
	
	void OnGUI()
	{
        if (Skin != null)
            GUI.skin = Skin;

		if(Playing)
		{
            GUI.DrawTexture(new Rect(40, 40, 64, 64), IconZombie);
            GUI.skin.label.normal.textColor = Color.white;
            GUI.skin.label.fontSize = 35;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(125, 40, 200, 64), Score.ToString());
		}
		else
		{
            Cursor.lockState = CursorLockMode.None;
            if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height / 2, 160, 30), "Exit"))
                SceneManager.LoadScene("Mainmenu");

            GUI.skin.label.fontSize = 25;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.normal.textColor = Color.white;
            GUI.Label(new Rect(0, Screen.height / 2 - 100, Screen.width, 30), "Game Over");
		}
	}
}
