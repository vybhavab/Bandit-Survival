using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
    using System.Collections.Generic;       //Allows us to use Lists.
    using UnityEngine.UI;                   //Allows us to use UI.

    public class CaveGameManager : MonoBehaviour
    {
		public float levelStartDelay = 2f;                      //Time to wait before starting level, in seconds.
		public float turnDelay = 0.1f;                          //Delay between each Player turn.
		public static CaveGameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
		public int playerFoodPoints;                      //Starting value for Player food points.
		//public static CaveGameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
		[HideInInspector] public bool playersTurn = false;       //Boolean to check if it's players turn, hidden in inspector but public.


		private Text levelText;                                 //Text to display current level number.
		private GameObject levelImage;                          //Image to block out level as levels are being set up, background for levelText.
		private MapGenerator mapGenerator;                       //Store a reference to our MapGenerator which will set up the level.
		private int level = 0;                                  //Current level number, expressed in game as "Day 1".
		private List<EnemyBuilder> enemies;                            //List of all Enemy units, used to issue them move commands.
		private bool enemiesMoving;
		Bandit player;
		//private bool enemiesMoving;                             //Boolean to check if enemies are moving.
		public bool doingSetup = true;                         //Boolean to check if we're setting up board, prevent Player from moving during setup.

		public int prevDirChanges = 0;

		public int GetLevel()
		{
			return level;
		}

		//Awake is always called before any Start functions
		void Awake()
		{
			//Check if instance already exists
			if (instance == null)
			{
				//if not, set instance to this
				instance = this;
			}//If instance already exists and it's not this:
			else if (instance != this)
			{
				//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
				Destroy(gameObject);
			}
			//Sets this to not be destroyed when reloading scene
			//DontDestroyOnLoad(gameObject);

			levelText = GameObject.Find("LevelText").GetComponent<Text>();
			levelImage = GameObject.Find("LevelImage");
			player = GameObject.Find("LightBandit").GetComponent<Bandit>();
			//Assign enemies to a new List of Enemy objects.
			enemies = new List<EnemyBuilder>();

			if(playerFoodPoints <= 0){
				playerFoodPoints = 5000;
			}
			//Get a component reference to the attached BoardManager script
			mapGenerator = GetComponent<MapGenerator>();

			//Call the InitGame function to initialize the first level
			InitGame();
		}

		//Initializes the game for each level.
		public void InitGame()
		{
			//Debug.Log("Init Game Cave");
			PauseMenu.GameIsPaused = false;
			player.enabled = false;
			//While doingSetup is true the player can't move, prevent player from moving while title card is up.
			doingSetup = true;

			level++;
			//Set the text of levelText to the string "Day" and append the current level number.
			levelText.text = "Level " + level;

			//Set levelImage to active blocking player's view of the game board during setup.
			levelImage.SetActive(true);



			//Clear any Enemy objects in our List to prepare for next level.
			foreach (EnemyBuilder enemy in enemies)
			{
				enemy.DeleteEnemy();
				Destroy(enemy.gameObject);
      }


			enemies = new List<EnemyBuilder>();

			mapGenerator.GenerateFirstMap();

			player.playerPosition = mapGenerator.randomStartingLocation;
			player.transform.position = player.playerPosition;


			//Call the HideLevelImage function with a delay in seconds of levelStartDelay.
			Invoke("HideLevelImage", levelStartDelay);
			/*if (!mapInitialized)
			{
				mapGenerator.GenerateFirstMap();
				mapInitialized = true;
			}
			//Call the SetupScene function of the BoardManager script, pass it current level number.
			else
			{
				mapGenerator.GenerateNewMap();
			}
			*/

		}



		//Hides black image used between levels
		void HideLevelImage()
		{
			//Disable the levelImage gameObject.
			levelImage.SetActive(false);

			//Set doingSetup to false allowing player to move again.
			doingSetup = false;

			player.enabled = true;
		}

		//Update is called every frame.
		void Update()
		{
			//Check that playersTurn or enemiesMoving or doingSetup are not currently true.
			if (playersTurn || enemiesMoving || doingSetup){
				//If any of these are true, return and do not start MoveEnemies.
				playersTurn = false;
				return;
			}
			playersTurn = true;
			//Start moving enemies
			StartCoroutine(MoveEnemies());
		}

		//Call this to add the passed in Enemy to the List of Enemy objects.
		public void AddEnemyToList(EnemyBuilder script)
		{
			//Add Enemy to List enemies.
			enemies.Add(script);
		}

		//To check that the game is being quit to save game;
		void OnApplicationQuit()
    {
			if(PlayerPrefs.GetInt("HighScore") < level - 1){
				PlayerPrefs.SetInt("HighScore", level);
				PlayerPrefs.Save();
			}
    }

		//GameOver is called when the player reaches 0 food points
		public void GameOver()
		{
			//Set levelText to display number of levels passed and game over message


			if(PlayerPrefs.HasKey("HighScore")){
				if(PlayerPrefs.GetInt("HighScore") < level){
					PlayerPrefs.SetInt("HighScore", level);
					PlayerPrefs.Save();
					levelText.text = "Game Over\nYou Reached Level: " + level + "\nNEW HIGH SCORE!";
				}else{
					levelText.text = "Game Over\nYou Reached Level: " + level + "\nYour HighScore is: " + PlayerPrefs.GetInt("HighScore");
				}
			}else{
				PlayerPrefs.SetInt("HighScore", level);
				PlayerPrefs.Save();
			}

			//Enable black background image gameObject.
			levelImage.SetActive(true);

			//Disable this GameManager.
			enabled = false;

		}

		public void SetDirChanges(int dirChanges)
		{
			prevDirChanges = dirChanges;
		}

		//Coroutine to move enemies in sequence.
		IEnumerator MoveEnemies()
		{
			//While enemiesMoving is true player is unable to move.
			enemiesMoving = true;
			if(!PauseMenu.GameIsPaused){
				//Wait for turnDelay seconds, defaults to .1 (100 ms).
				//yield return new WaitForSeconds(turnDelay);

				//If there are no enemies spawned (IE in first level):
				if (enemies.Count == 0)
				{
					//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
					//yield return new WaitForSeconds(turnDelay);
				}

				//Loop through List of Enemy objects.
				for (int i = 0; i < enemies.Count; i++)
				{
					//Call the MoveEnemy function of Enemy at index i in the enemies List.
					if(enemies[i].gameObject.activeSelf){
						enemies[i].MoveEnemy();
						yield return new WaitForSeconds(enemies[i].moveTime);
					}else{
						Destroy(enemies[i].gameObject);
						enemies.RemoveAt(i);
					}

					//Wait for Enemy's moveTime before moving next Enemy,
						yield return new WaitForSeconds(0);
				}

				playersTurn = true;
			}
			enemiesMoving = false;

		}

	}
}