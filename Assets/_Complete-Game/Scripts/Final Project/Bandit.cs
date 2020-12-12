using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;

namespace Completed
{
    public class Bandit : MovingObject
    {

        private Animator m_animator;
        public Rigidbody2D banditBody;
        private Sensor_Bandit m_groundSensor;
        private bool m_combatIdle = false;
        private bool m_isDead = false;
        public float restartLevelDelay = 1f;        //Delay time in seconds to restart level.

        CaveGameManager gameManager;

        public Vector2 playerPosition;
        public Vector2 previousPlayerPosition;
        Vector2 velocity;
        Vector2 input = new Vector2();
        public float movementSpeed = 6;

        public int dirChanges;  // Number of Times the direction of motion changes
        public int flippedCount;  // Number of Times the direction of motion flips
        private Vector2 prev_direction;  // Store the previous direction
        public int weaponSwings;
        bool dirFlipped = false;

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
        public AudioClip gameOverSound;				//Audio clip to play when player dies.

        float attackTime = .6f; //Amount of time the attack animation takes
        bool canMove; //Set to false if the attack animation is running
        bool canAttack; //Used so the full attack animation runs before the player can attack again
        public float damage = 1;  //How much damage a player does to an enemy when chopping it.

        float deathTime = 1f; //Amount of time between death animation and Game Over screen



        // Use this for initialization
        void Start()
        {

            //
            MapGenerator map = GameObject.Find("MapGenerator").GetComponent<MapGenerator>();
            m_animator = GetComponent<Animator>();
            

            // Reset Values
            prev_direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            prev_direction = prev_direction.normalized;
            dirChanges = 0;
            flippedCount = 0;
            weaponSwings = 0;

            //Store player position to check how far player has moved to decrement food.
            previousPlayerPosition = transform.position;

            //Get the CaveGameManager component to edit food points data
            gameManager = GameObject.FindWithTag("GameManager").GetComponent<CaveGameManager>();
            food = gameManager.playerFoodPoints;

            //foodDecrement = GameObject.FindWithTag("GameManager").GetComponent<AIFoodDecrement>().getFoodDecrement();
            foodDecrement = GameObject.FindWithTag("GameManager").GetComponent<AIFoodDecrementContinuous>().getFoodDecrement();
            GameObject.FindWithTag("GameManager").GetComponent<ItemSpawn>().updateFoodPercent(GameObject.FindWithTag("GameManager").GetComponent<AIFoodCount>().getFoodWeight());
            GameObject.FindWithTag("GameManager").GetComponent<ItemSpawn>().updateWeaponPercent(GameObject.FindWithTag("GameManager").GetComponent<AIWeaponCount>().getWeaponWeight());
            // pointsPerFood = temps.Item2;

            canMove = true;
            canAttack = true;

            weaponName = "Weapon";
            
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

            if (Input.GetMouseButtonDown(0))
            {
                weaponSwings = weaponSwings + 1;
                canMove = false;
            }

            if (canMove)
            {
                // -- Handle input and movement --

                Vector2 direction = input.normalized;

                // Check if current direciton of motion and new direction are the same
                if (direction != Vector2.zero && direction != prev_direction)
                {
                    dirChanges = dirChanges + 1;
                    // Check if the direction is flipped
                    if (Vector2.Dot(direction, prev_direction) == -1)
                    {
                        flippedCount = flippedCount + 1;
                        dirFlipped = true;
                    }
                    prev_direction = direction;
                }

                // Check if the player has moved more than 1 tile from the previous position that was stored or if the player has switched directions to decrement food
                if (Mathf.Pow(transform.position.x - previousPlayerPosition.x, 2) + Mathf.Pow(transform.position.y - previousPlayerPosition.y, 2) > 1 || dirFlipped)
                {
                    previousPlayerPosition = transform.position;
                    food = food + foodDecrement;
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
                    transform.localScale = new Vector3(-0.8f, 0.8f, 1.0f);
                else if (input.x < 0)
                    transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);

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
            if (Input.GetMouseButtonDown(0))
            {
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
            if (triggerCollider.tag == "Exit")
            {
                //Disable the exit collider component so it doesn't continue to trigger
                triggerCollider.GetComponent<Collider2D>().enabled = false;
                
                //Disable the player since the elvel is over
                enabled = false;

                //Stop the player from moving
                banditBody.velocity = Vector2.zero;


                GameObject.FindWithTag("GameManager").GetComponent<CaveGameManager>().setDirChanges(dirChanges);
                //GameObject.FindWithTag("GameManager").GetComponent<AIFoodDecrement>().updateGenerator(dirChanges);
                GameObject.FindWithTag("GameManager").GetComponent<AIFoodDecrementContinuous>().updateGenerator(dirChanges);
                GameObject.FindWithTag("GameManager").GetComponent<AIWeaponCount>().updateGenerator(weaponSwings);
                GameObject.FindWithTag("GameManager").GetComponent<AIFoodCount>().updateGenerator(flippedCount);
                Restart();


            }
            //Check if the tag of the trigger collided with is Food.
            else if (triggerCollider.tag == "Fruit")
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
            else if (triggerCollider.tag == "Drink")
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
            else if (triggerCollider.tag == "Veg")
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
            else if (triggerCollider.tag == "Meat")
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
            else if(triggerCollider.CompareTag("Sword"))
            {
                damage = triggerCollider.GetComponent<Weapon>().damage;
                weaponName = triggerCollider.GetComponent<Weapon>().weaponName;
                //Update foodText to represent current total and notify player that they gained points
                StartCoroutine(ShowWeaponChange(triggerCollider.GetComponent<Weapon>().damage));

                //Disable the weapon object the player collided with.
                triggerCollider.gameObject.SetActive(false);
            }
            else if (triggerCollider.tag == "Symbol") {
                //Disable the symbol object the player collided with.
                triggerCollider.gameObject.SetActive(false);
            }
            else if (triggerCollider.tag == "Enemy")
            {
                if (canAttack == false)
                {
                    Debug.Log("Enemy HP: " + triggerCollider.GetComponent<EnemyBuilder>().hp);
                    triggerCollider.GetComponent<EnemyBuilder>().DamageEnemy(damage);
                    //StartCoroutine(AttackEnemy(triggerCollider));
                    //banditBody.velocity = new Vector2(0, 0);
                }
            }
        }

        private void OnDisable()
        {
            //gameManager.playerFoodPoints = food;
        }

        private void Restart()
        {
            //Load the next level
            CaveGameManager caveGameManager = GameObject.Find("MapGenerator").GetComponent<CaveGameManager>();
            //foodDecrement = GameObject.FindWithTag("GameManager").GetComponent<AIFoodDecrement>().getFoodDecrement();
            foodDecrement = GameObject.FindWithTag("GameManager").GetComponent<AIFoodDecrementContinuous>().getFoodDecrement();
            GameObject.FindWithTag("GameManager").GetComponent<ItemSpawn>().updateFoodPercent(GameObject.FindWithTag("GameManager").GetComponent<AIFoodCount>().getFoodWeight());
            GameObject.FindWithTag("GameManager").GetComponent<ItemSpawn>().updateWeaponPercent(GameObject.FindWithTag("GameManager").GetComponent<AIWeaponCount>().getWeaponWeight());
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

        }

        //OnCantMove overrides the abstract function OnCantMove in MovingObject.
        //It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
        protected override void OnCantMove<T>(T component)
        {
            if (component.tag == "Wall")
            {
                //Set hitWall to equal the component passed in as a parameter
                Wall hitWall = component as Wall;

                //Call the DamageWall function of the Wall we are hitting.
                hitWall.DamageWall(damage);
            }
            if (component.tag == "Enemy")
            {
                EnemyBuilder hitEnemy = component as EnemyBuilder;
                hitEnemy.DamageEnemy(damage);
            }
            //Set the attack trigger of the player's animation controller in order to play the player's attack animation.
            m_animator.SetTrigger("Attack");
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
