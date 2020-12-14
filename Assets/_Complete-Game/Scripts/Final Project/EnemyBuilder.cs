using UnityEngine;
using System.Collections;
namespace Completed
{
    using System.Collections.Generic;
    public class EnemyBuilder : MovingObject
    {
        // Start is called before the first frame update
        public int damageToPlayer;
        public AudioClip attackSound1;
        public AudioClip attackSound2;
        private Animator animator;
        private Transform target;
        private bool skipMove;
        public int hp;
        int originalHp;
        public int enemyHealthChange;
        public AudioClip chopSound1;                //1 of 2 audio clips that play when the enemy is attacked by the player.
        public AudioClip chopSound2;                //2 of 2 audio clips that play when the enemy is attacked by the player.
        public List<GameObject> currentLevelBosses = new List<GameObject>();
        public Vector2 enemyPosition;

        public int level;

        Bandit player;
        ItemSpawn itemSpawn;
        WeaponManager wm;

        protected override void Start()
        {
            CaveGameManager.instance.AddEnemyToList (this);
            animator = GetComponent<Animator> ();
            target = GameObject.FindGameObjectWithTag ("Player").transform;
            enemyPosition = transform.position;


            if (level >= 1)
            {
                // 0-9:no reward, 10-19:fruit, 20-29:drink, 30-39: veg, 40-49: meat
                enemyHealthChange = GameObject.FindWithTag("GameManager").GetComponent<AIEnemyHealth>().getHealthChange();
                hp = Random.Range(0, 50) + enemyHealthChange;
            }
            else
            {
                hp = Random.Range(0, 50);
            }
            originalHp = hp;

            player = GameObject.FindWithTag("Player").GetComponent<Bandit>();
            itemSpawn = GameObject.FindWithTag("GameManager").GetComponent<ItemSpawn>();
            wm = GameObject.FindWithTag("WeaponManager").GetComponent<WeaponManager>();

            base.Start();

        }

        void OnGUI()
        {
            var rect = new Rect(0,0,50,100);
            var offset =  new Vector2(-.2f,-1.2f); // height above the target position

            var point = Camera.main.WorldToScreenPoint(enemyPosition + offset);
            rect.x = point.x;
            rect.y = Screen.height - point.y - rect.height; // bottom left corner set to the 3D point
            //var label = "x: " + (target.position.x - transform.position.x).ToString("0.00") + " y: " + (target.position.y - transform.position.y).ToString("0.00");
            if(hp < originalHp){
                GUI.Label(rect, hp.ToString()); // display its name, or other string
            }
        }

        // Update is called once per frame
        void Update()
        {
            player = GameObject.FindWithTag("Player").GetComponent<Bandit>();
            enemyPosition = transform.position;
            if (player.playerAttacking == true)
            {
                if(Mathf.Pow(target.position.x - enemyPosition.x, 2) + Mathf.Pow(target.position.y - enemyPosition.y, 2) <= 1)
                {
                    if (target.position.x - enemyPosition.x > 0 && player.facingLeft)
                    {
                        player.playerAttacking = false;
                        StartCoroutine(DamageEnemy(player.damage));
                    }
                    else if (target.position.x - enemyPosition.x < 0 && player.facingRight)
                    {
                        player.playerAttacking = false;
                        StartCoroutine(DamageEnemy(player.damage));
                    }

                }
            }
        }

        protected override void AttemptMove <T> (int xDir, int yDir)
        {

            if(skipMove){
                skipMove = false;
                return;
            }

            base.AttemptMove <T> (xDir, yDir);

            skipMove = true;

        }

        public void MoveEnemy ()
        {
            //Declare variables for X and Y axis move directions, these range from -1 to 1.
            //These values allow us to choose between the cardinal directions: up, down, left and right.
            int xDir = 0;
            int yDir = 0;

            //If the difference in positions is approximately zero (Epsilon) do the following:

            if(Mathf.Abs(target.position.x - transform.position.x) < 7 || Mathf.Abs(target.position.y - transform.position.y) < 7){
                if(target.position.x - transform.position.x < 0) {
                    xDir = -1;
                }else{
                    xDir = +1;
                }

                if(target.position.y - transform.position.y < 0){
                    yDir = -1;
                }else{
                    yDir = +1;
                }
            }else{
                xDir = Random.Range(-1, 1);
                yDir = Random.Range(-1, 1);
            }

            //Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player
            AttemptMove <Bandit> (xDir, yDir);
        }

        protected override void OnCantMove <T> (T component)
        {
			Bandit hitPlayer = component as Bandit;

			//Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
            Debug.Log("DAMAGE" + hitPlayer);

            hitPlayer.DecrementHealthFromPlayer();

			animator.SetTrigger ("enemyAttack");

			SoundManager.instance.RandomizeSfx (attackSound1, attackSound2);
        }

        //DamageEnemy is called when the player attacks a enemy.
        public IEnumerator DamageEnemy(int loss)
        {
            //Call the RandomizeSfx function of SoundManager to play one of two chop sounds.
            SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);

            //Subtract loss from hit point total.
            hp -= loss;
            //Debug.Log("Enemy attacked, current hp = " + hp);
            yield return new WaitForSeconds(.6f);
            //If hit points are less than or equal to zero:

            if (hp <= 0)
            {
                // spawn food as reward
                SpawnFoodReward();
                //Disable the gameObject.
                DeleteEnemy();
            }
        }

        public void SpawnFoodReward() {
            if(originalHp >= 30){
                float x = transform.position.x + Random.Range(-0.5f, 0.5f);
                float y = transform.position.y + Random.Range(-0.5f, 0.5f);
                itemSpawn.SpawnItem(x, y, Random.Range(0, 4), false);
            }
            // generate fruit
            // int foodType;

            // if (originalHp >= 10 && originalHp < 20) {
            //     foodType = 4;
            // }
            // // generate drinks
            // else if (originalHp >= 20 && originalHp < 30) {
            //     foodType = 6;
            // }
            // // generate veg
            // else if (originalHp >= 30 && originalHp < 40) {
            //     foodType = 8;
            // }
            // // generate meat
            // else if (originalHp >= 40 && originalHp < 50) {
            //     foodType = 9;
            // }
            // else { //generate weapon
            //     foodType = -1;
            // };
            // // Debug.Log("Food Type = " + foodType);
            // if(foodType > 0){
            //     for (int i = 0; i < 4; i++) {
            //         float x = transform.position.x + Random.Range(-0.5f, 0.5f);
            //         float y = transform.position.y + Random.Range(-0.5f, 0.5f);
            //         itemSpawn.SpawnItem(x, y, 5, false, foodType);
            //     }
            // }
        }

        public void DeleteEnemy()
        {
            gameObject.SetActive(false);
            Debug.Log("Enemy Destroyed");
        }

    }
}