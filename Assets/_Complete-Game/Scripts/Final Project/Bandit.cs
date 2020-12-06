using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;

namespace Completed
{
    public class Bandit : MonoBehaviour
    {

        private Animator m_animator;
        public Rigidbody2D banditBody;
        private Sensor_Bandit m_groundSensor;
        private bool m_combatIdle = false;
        private bool m_isDead = false;
        public float restartLevelDelay = 1f;        //Delay time in seconds to restart level.

        public Vector2 playerPosition;
        public Vector2 previousPlayerPosition;
        Vector2 velocity;
        Vector2 input = new Vector2();
        public float movementSpeed = 6;

        public int dirChanges;  // Number of Times the direction of motion changes
        public int flippedCount;  // Number of Times the direction of motion flips
        private Vector2 prev_direction;  // Store the previous direction

        private int food;
        public int foodDecrement;
        public int pointsPerFood;
        public Text foodText;

        float attackTime = .6f; //Amount of time the attack animation takes
        bool canMove; //Set to false if the attack animation is running
        bool canAttack; //Used so the full attack animation runs before the player can attack again


        // Use this for initialization
        void Start()
        {
            MapGenerator map = GameObject.Find("MapGenerator").GetComponent<MapGenerator>();
            m_animator = GetComponent<Animator>();
            

            // Reset Values
            prev_direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            prev_direction = prev_direction.normalized;
            dirChanges = 0;
            flippedCount = 0;

            previousPlayerPosition = transform.position;

            food = CaveGameManager.instance.playerFoodPoints;

            var temps = GameObject.FindWithTag("GameManager").GetComponent<AIFoodDecrement>().getFoodDecrement();
            foodDecrement = temps.Item1;
            pointsPerFood = temps.Item2;

            canMove = true;
            canAttack = true;
        }

        // Update is called once per frame
        void Update()
        {
            MapGenerator map = GameObject.Find("MapGenerator").GetComponent<MapGenerator>();
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (Input.GetMouseButtonDown(0))
            {
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
                    }
                    prev_direction = direction;
                }

                if (Mathf.Pow(transform.position.x - previousPlayerPosition.x, 2) + Mathf.Pow(transform.position.y - previousPlayerPosition.y, 2) > 1)
                {
                    previousPlayerPosition = transform.position;
                    food = food + foodDecrement;
                    foodText.text = "Food: " + food;
                }

                velocity = direction * movementSpeed;

                // Swap direction of sprite depending on walk direction
                if (input.x > 0)
                    transform.localScale = new Vector3(-0.8f, 0.8f, 1.0f);
                else if (input.x < 0)
                    transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);

                // Move
                banditBody.velocity = new Vector2(velocity.x, velocity.y);
                CheckIfGameOver();
            }





            // -- Handle Animations --

            /*
            //Death
            if (Input.GetKeyDown("e")) {
                if(!m_isDead)
                    m_animator.SetTrigger("Death");
                else
                    m_animator.SetTrigger("Recover");

                m_isDead = !m_isDead;
            }

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

            /*
            //Change between idle and combat idle
            else if (Input.GetKeyDown("f"))
                m_combatIdle = !m_combatIdle;

            //Jump
            else if (Input.GetKeyDown("space") && m_grounded) {
                m_animator.SetTrigger("Jump");
                m_grounded = false;
                m_animator.SetBool("Grounded", m_grounded);
                m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
                m_groundSensor.Disable(0.2f);
            }
            */

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
            if (triggerCollider.tag == "Exit")
            {
                triggerCollider.GetComponent<Collider2D>().enabled = false;
                enabled = false;
                Debug.Log("Trigger");
                banditBody.velocity = Vector2.zero;
                GameObject.FindWithTag("GameManager").GetComponent<CaveGameManager>().setDirChanges(dirChanges);
                GameObject.FindWithTag("GameManager").GetComponent<AIFoodDecrement>().updateGenerator(dirChanges);
                //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
                Restart();
                //Invoke("Restart", restartLevelDelay);

                //Disable the player object since level is over.
                

            }
        }

        private void OnDisable()
        {
            CaveGameManager.instance.playerFoodPoints = food;
        }

        private void Restart()
        {
            //Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
            //and not load all the scene object in the current scene.
            CaveGameManager caveGameManager = GameObject.Find("MapGenerator").GetComponent<CaveGameManager>();
            caveGameManager.InitGame();
        }

        private void CheckIfGameOver()
        {
            //Check if food point total is less than or equal to zero.
            if (food <= 0)
            {
                //Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
                //SoundManager.instance.PlaySingle(gameOverSound);

                //Stop the background music.
                //SoundManager.instance.musicSource.Stop();

                //Call the GameOver function of GameManager.
                CaveGameManager.instance.GameOver();
            }
        }

        IEnumerator Attack()
        {
            canMove = false;
            m_animator.SetTrigger("Attack");
            canAttack = false;

            yield return new WaitForSeconds(attackTime);
            canAttack = true;
            canMove = true;

        }
    }
}
