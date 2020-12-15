using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Completed
{
    public class Bandit : MonoBehaviour
    {

        private Animator m_animator;
        public Rigidbody2D banditBody;
        private bool m_combatIdle = false;
        private bool m_isDead = false;
        public float restartLevelDelay = 1f;        //Delay time in seconds to restart level.

        CaveGameManager gameManager;

        public Vector2 playerPosition;
        public Vector2 previousPlayerPosition;
        Vector2 velocity;
        Vector2 input = new Vector2();
        public int movementSpeed = 6;

        public int dirChanges;  // Number of Times the direction of motion changes
        public bool facingLeft;
        public bool facingRight;
        public int flippedCount;  // Number of Times the direction of motion flips
        private Vector2 prev_direction;  // Store the previous direction
        public int weaponSwings;
        bool dirFlipped = false;
        public int explorationCount; //Number of Floor Tiles in the map the player has visited
        public float percentMapVisited; //Percent of Floor Tiles that were interacted with


        private int food;               //For getting the player food value from the CaveGameManager
        public int foodDecrement;       //Amount food is decreased by when player moves.
        public int pointsPerFood;
        public int pointsPerFruit = 10; //Number of points to add to player food points when picking up a food object.
        public int pointsPerDrink = 20; //Number of points to add to player food points when picking up a soda object.
        public int pointsPerVeg = 15;
        public int pointsPerMeat = 50;

        public Text foodText;
        public Text weaponText;
        public string weaponName;
        public AudioClip moveSound1;                //1 of 2 Audio clips to play when player moves.
        public AudioClip moveSound2;                //2 of 2 Audio clips to play when player moves.
        public AudioClip eatSound1;                 //1 of 2 Audio clips to play when player collects a food object.
        public AudioClip eatSound2;                 //2 of 2 Audio clips to play when player collects a food object.
        public AudioClip drinkSound1;               //1 of 2 Audio clips to play when player collects a soda object.
        public AudioClip drinkSound2;               //2 of 2 Audio clips to play when player collects a soda object.
        public AudioClip gameOverSound;             //Audio clip to play when player dies.

        readonly float attackTime = .6f; //Amount of time the attack animation takes
        public bool canMove; //Set to false if the attack animation is running
        public bool canAttack; //Used so the full attack animation runs before the player can attack again
        public bool playerAttacking;
        public int damage;  //How much damage a player does to an enemy when chopping it.
        public int damageChange;
        readonly float deathTime = 1f; //Amount of time between death animation and Game Over screen

        public EnemyBuilder enemy1;
        public EnemyBuilder enemy2;

        public int enemyDamage;

        // Use this for initialization
        void Start()
        {

            m_animator = GetComponent<Animator>();


            // Reset Values
            prev_direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            prev_direction = prev_direction.normalized;
            dirChanges = 0;
            flippedCount = 0;
            weaponSwings = 0;
            explorationCount = 0;

            //Store player position to check how far player has moved to decrement food.
            previousPlayerPosition = transform.position;

            //Get the CaveGameManager component to edit food points data
            gameManager = GameObject.FindWithTag("GameManager").GetComponent<CaveGameManager>();
            food = gameManager.playerFoodPoints;

            //foodDecrement = GameObject.FindWithTag("GameManager").GetComponent<AIFoodDecrement>().getFoodDecrement();
            foodDecrement = GameObject.FindWithTag("GameManager").GetComponent<AIFoodDecrementContinuous>().GetFoodDecrement();
            GameObject.FindWithTag("GameManager").GetComponent<ItemSpawn>().updateFoodPercent(GameObject.FindWithTag("GameManager").GetComponent<AIFoodCount>().GetFoodWeight());
            GameObject.FindWithTag("GameManager").GetComponent<ItemSpawn>().updateWeaponPercent(GameObject.FindWithTag("GameManager").GetComponent<AIWeaponCount>().GetWeaponWeight());
            // pointsPerFood = temps.Item2;

            canMove = true;
            canAttack = true;
            damage = 10;
            weaponName = "Weapon";
            enemyDamage = 50;
        }

        // Update is called once per frame
        void Update()
        {

            //If player is dead, don't process input
            if (m_isDead)
            {
                return;
            }

            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (canMove)
            {
                // -- Handle input and movement --

                Vector2 direction = input.normalized;

                // Check if current direciton of motion and new direction are the same
                if (direction != Vector2.zero && direction != prev_direction)
                {
                    dirChanges += 1;
                    // Check if the direction is flipped
                    if (Vector2.Dot(direction, prev_direction) == -1)
                    {
                        flippedCount += 1;
                        dirFlipped = true;
                    }
                    prev_direction = direction;
                }

                // Check if the player has moved more than 1 tile from the previous position that was stored or if the player has switched directions to decrement food
                if (Mathf.Pow(transform.position.x - previousPlayerPosition.x, 2) + Mathf.Pow(transform.position.y - previousPlayerPosition.y, 2) > 1 || dirFlipped)
                {
                    previousPlayerPosition = transform.position;
                    food += foodDecrement;
                    foodText.text = "Food: " + food;
                    dirFlipped = false;
                    weaponText.text = weaponName + " Damage: " + damage;
                    //Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
                    SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
                }

                //Set velocity on players rigidbody component to move the player
                velocity = direction * movementSpeed;

                // Swap direction of sprite depending on walk direction
                if (input.x > 0)
                {
                    facingLeft = false;
                    facingRight = true;
                    transform.localScale = new Vector3(-0.8f, 0.8f, 1.0f);
                }
                else if (input.x < 0)
                {
                    facingLeft = true;
                    facingRight = false;
                    transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);
                }

                // Move
                banditBody.velocity = new Vector2(velocity.x, velocity.y);



                //Check if the game is over since the player has lost food points
                CheckIfGameOver();
            }





            // -- Handle Animations --

            /*
            //Hurt
            else if (Input.GetKeyDown("q"))
                m_animator.SetTrigger("Hurt");
            */

            //Attack
            if ((Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0)) & canAttack)
            {

                weaponSwings += 1;
                canMove = false;

                playerAttacking = true;
                StartCoroutine(Attack());
                banditBody.velocity = new Vector2(0, 0);
            }

            //Run
            else if ((Mathf.Abs(input.x) > Mathf.Epsilon || Mathf.Abs(input.y) > Mathf.Epsilon) && canMove == true)
                m_animator.SetInteger("AnimState", 2);

            //Combat Idle
            else if (m_combatIdle)
                m_animator.SetInteger("AnimState", 1);

            //Idle
            else
                m_animator.SetInteger("AnimState", 0);
        }

        void OnTriggerEnter2D(Collider2D triggerCollider)
        {
            //Check if the tag of the trigger is exit and load the next level
            if (triggerCollider.CompareTag("Exit"))
            {
                MapGenerator map = GameObject.Find("MapGenerator").GetComponent<MapGenerator>();
                map.UpdateMapSize(explorationCount);
                //Debug.Log("New Map Size: " + map.baseHeight);

                //Disable the exit collider component so it doesn't continue to trigger
                triggerCollider.GetComponent<Collider2D>().enabled = false;

                //Disable the player since the level is over
                enabled = false;

                //Stop the player from moving
                banditBody.velocity = Vector2.zero;

                //Reset explorationCount for the next level
                explorationCount = 0;

                Debug.Log("Weapon swings:" + weaponSwings);

                GameObject.FindWithTag("GameManager").GetComponent<CaveGameManager>().setDirChanges(dirChanges);
                //GameObject.FindWithTag("GameManager").GetComponent<AIFoodDecrement>().updateGenerator(dirChanges);
                GameObject.FindWithTag("GameManager").GetComponent<AIPlayerDamage>().updateGenerator(weaponSwings);
                GameObject.FindWithTag("GameManager").GetComponent<AIFoodDecrementContinuous>().updateGenerator(dirChanges);
                GameObject.FindWithTag("GameManager").GetComponent<AIWeaponCount>().updateGenerator(weaponSwings);
                GameObject.FindWithTag("GameManager").GetComponent<AIFoodCount>().updateGenerator(flippedCount);
                GameObject.FindWithTag("GameManager").GetComponent<AIEnemyHealth>().updateGenerator(weaponSwings);
                Restart();
            }
            //Check if the tag of the trigger collided with is Food.
            else if (triggerCollider.CompareTag("Fruit"))
            {
                //Add pointsPerFruit to the players current food total.
                food += pointsPerFruit;

                //Update foodText to represent current total and notify player that they gained points
                StartCoroutine(ShowFoodGain(pointsPerFruit));

                //Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

                //Disable the food object the player collided with.
                triggerCollider.gameObject.SetActive(false);
            }

            //Check if the tag of the trigger collided with is Soda.
            else if (triggerCollider.CompareTag("Drink"))
            {
                //Add pointsPerDrink to players food points total
                food += pointsPerDrink;

                //Update foodText to represent current total and notify player that they gained points
                StartCoroutine(ShowFoodGain(pointsPerDrink));

                //Call the RandomizeSfx function of SoundManager and pass in two drinking sounds to choose between to play the drinking sound effect.
                SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

                //Disable the soda object the player collided with.
                triggerCollider.gameObject.SetActive(false);
            }
            else if (triggerCollider.CompareTag("Veg"))
            {
                //Add pointsPerVeg to players food points total
                food += pointsPerVeg;

                //Update foodText to represent current total and notify player that they gained points
                StartCoroutine(ShowFoodGain(pointsPerVeg)); ;

                //Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

                //Disable the soda object the player collided with.
                triggerCollider.gameObject.SetActive(false);
            }
            else if (triggerCollider.CompareTag("Meat"))
            {
                //Add pointsPerMeat to players food points total
                food += pointsPerMeat;

                //Update foodText to represent current total and notify player that they gained points
                StartCoroutine(ShowFoodGain(pointsPerMeat));

                //Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

                //Disable the soda object the player collided with.
                triggerCollider.gameObject.SetActive(false);
            }
            else if (triggerCollider.CompareTag("Sword"))
            {
                damage = triggerCollider.GetComponent<Weapon>().damage + damageChange;
                weaponName = triggerCollider.GetComponent<Weapon>().weaponName;
                //Update foodText to represent current total and notify player that they gained points
                StartCoroutine(ShowWeaponChange(triggerCollider.GetComponent<Weapon>().damage));

                //Disable the weapon object the player collided with.
                triggerCollider.gameObject.SetActive(false);
            }
            else if (triggerCollider.CompareTag("Floor"))
            {
                //Add this tile to the exploration count
                explorationCount += 1;

                //Disable the trigger on this tile so it doesn't get added to the count again if the player revisits the tile
                triggerCollider.GetComponent<Collider2D>().enabled = false;
            }

            else if (triggerCollider.CompareTag("Symbol")) {
                //Disable the symbol object the player collided with.
                triggerCollider.gameObject.SetActive(false);
            }
        }

        private void Restart()
        {
            //Load the next level
            CaveGameManager caveGameManager = GameObject.Find("MapGenerator").GetComponent<CaveGameManager>();
            //foodDecrement = GameObject.FindWithTag("GameManager").GetComponent<AIFoodDecrement>().getFoodDecrement();
            foodDecrement = GameObject.FindWithTag("GameManager").GetComponent<AIFoodDecrementContinuous>().GetFoodDecrement();
            GameObject.FindWithTag("GameManager").GetComponent<ItemSpawn>().updateFoodPercent(GameObject.FindWithTag("GameManager").GetComponent<AIFoodCount>().GetFoodWeight());
            GameObject.FindWithTag("GameManager").GetComponent<ItemSpawn>().updateWeaponPercent(GameObject.FindWithTag("GameManager").GetComponent<AIWeaponCount>().GetWeaponWeight());

            damageChange = GameObject.FindWithTag("GameManager").GetComponent<AIPlayerDamage>().GetDamageChange();
            damage += damageChange;
            if (damage < 5) damage = 5;

            dirChanges = 0;
            flippedCount = 0;
            weaponSwings = 0;
            caveGameManager.InitGame();
        }

        private void CheckIfGameOver()
        {
            //Check if food point total is less than or equal to zero.
            if (food <= 0)
            {
                //Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
                SoundManager.instance.PlaySingle(gameOverSound);

                //Stop the background music.
                SoundManager.instance.musicSource.Stop();

                //Set m_isDead to true so the player can't move
                m_isDead = true;

                //Set player velocity to 0 and play death animation
                banditBody.velocity = Vector2.zero;
                m_animator.SetTrigger("Death");

                //Call the GameOver function of GameManager.
                gameManager.Invoke("GameOver", deathTime);
            }
        }

        IEnumerator Attack()
        {
            //Stop the player from moving while they are attacking
            canMove = false;
            m_animator.SetTrigger("Attack");
            canAttack = false;

            yield return new WaitForSeconds(attackTime);

            canAttack = true;
            canMove = true;
            playerAttacking = false;

        }

        public void DecrementHealthFromPlayer()
        {
            //Reduce player health
            food -= enemyDamage;

			//Update the food display with the new total.
			foodText.text = "-"+ enemyDamage + " Food: " + food;
			//Check to see if game has ended.
			CheckIfGameOver ();
        }

        IEnumerator ShowFoodGain(int points)
        {
            foodText.text = "+" + points + " Food: " + food;
            yield return new WaitForSeconds(1);
        }

        IEnumerator ShowWeaponChange(float points)
        {
            weaponText.text = "Obtained " + weaponName + "! Damage: " + points;
            yield return new WaitForSeconds(1);
        }
    }
}
