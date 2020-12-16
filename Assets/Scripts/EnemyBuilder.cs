using UnityEngine;
using System.Collections;
namespace Completed
{
    using System.Collections.Generic;
    public class EnemyBuilder : EntityMover
    {
        // Start is called before the first frame update
        public AudioClip attackSound1;
        public AudioClip attackSound2;
        private Animator animator;
        private Transform target;
        public Vector2 targetd;
        private bool skipMove;
        public int hp;
        int originalHp;
        public int enemyHealthChange;
        int multiplier;
        public AudioClip chopSound1;                //1 of 2 audio clips that play when the enemy is attacked by the player.
        public AudioClip chopSound2;                //2 of 2 audio clips that play when the enemy is attacked by the player.
        public List<GameObject> currentLevelBosses = new List<GameObject>();
        public Vector2 enemyPosition;

        bool isAttackingPlayer;
        Bandit player;
        ItemSpawn itemSpawn;

        public HealthBarBehavior healthBar;

        protected override void Start()
        {
            CaveGameManager.instance.AddEnemyToList (this);
            animator = GetComponent<Animator> ();
            target = GameObject.FindGameObjectWithTag ("Player").transform;
            enemyPosition = transform.position;
            isAttackingPlayer = false;
            if (CaveGameManager.instance.GetLevel() <= 1)
            {
                hp = Random.Range(10, 50);
                multiplier = 1;
            }
            else if (CaveGameManager.instance.GetLevel() <= 5)
            {
                // 0-9:no reward, 10-19:fruit, 20-29:drink, 30-39: veg, 40-49: meat
                enemyHealthChange = GameObject.FindWithTag("GameManager").GetComponent<AIEnemyHealth>().GetHealthChange(CaveGameManager.instance.GetLevel());
                hp = Random.Range(10, 50) + enemyHealthChange;
                multiplier = 1;
            }
            else if (CaveGameManager.instance.GetLevel() > 5 && CaveGameManager.instance.GetLevel() <= 10)
            {
                enemyHealthChange = GameObject.FindWithTag("GameManager").GetComponent<AIEnemyHealth>().GetHealthChange(CaveGameManager.instance.GetLevel());
                hp = Random.Range(100, 500) + enemyHealthChange;
                multiplier = 10;
            }
            else if (CaveGameManager.instance.GetLevel() > 10 && CaveGameManager.instance.GetLevel() <= 15)
            {
                enemyHealthChange = GameObject.FindWithTag("GameManager").GetComponent<AIEnemyHealth>().GetHealthChange(CaveGameManager.instance.GetLevel());
                hp = Random.Range(500, 1000) + enemyHealthChange;
                multiplier = 20;
            }
            else if (CaveGameManager.instance.GetLevel() > 15 && CaveGameManager.instance.GetLevel() <= 20)
            {
                enemyHealthChange = GameObject.FindWithTag("GameManager").GetComponent<AIEnemyHealth>().GetHealthChange(CaveGameManager.instance.GetLevel());
                hp = Random.Range(1000, 5000) + enemyHealthChange;
                multiplier = 100;
            }
            else
            {
                enemyHealthChange = GameObject.FindWithTag("GameManager").GetComponent<AIEnemyHealth>().GetHealthChange(CaveGameManager.instance.GetLevel());
                hp = Random.Range(5000, 20000) + enemyHealthChange;
                multiplier = 400;
            }

            originalHp = hp;
            healthBar.SetHealth(hp, originalHp);
            player = GameObject.FindWithTag("Player").GetComponent<Bandit>();
            itemSpawn = GameObject.FindWithTag("GameManager").GetComponent<ItemSpawn>();
            base.Start();

        }

        // Update is called once per frame
        void Update()
        {
            player = GameObject.FindWithTag("Player").GetComponent<Bandit>();
            enemyPosition = transform.position;
            if(target.position.x > enemyPosition.x){
                transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);
            }else{
                transform.localScale = new Vector3(-0.8f, 0.8f, 1.0f);
            }


            if(Mathf.Pow(target.position.x - enemyPosition.x, 2) + Mathf.Pow(target.position.y - enemyPosition.y, 2) <= 1)
            {
                if(!isAttackingPlayer && !player.playerAttacking && !CaveGameManager.instance.playersTurn){
                    isAttackingPlayer = true;
                    StartCoroutine(DamagePlayer());
                }
            }

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
            int xDir;
            int yDir;
            //If the difference in positions is approximately zero (Epsilon) do the following:
            if(!isAttackingPlayer && Mathf.Abs(target.position.x - transform.position.x) < 3 && Mathf.Abs(target.position.y - transform.position.y) < 3){
                if(target.position.x - transform.position.x < 0) {
                    xDir = -1;
                }
                else
                {
                    xDir = +1;
                }

                if (target.position.y - transform.position.y < 0)
                {
                    yDir = -1;
                }
                else
                {
                    yDir = +1;
                }
            }
            else
            {
                xDir = Random.Range(-1, 1);
                yDir = Random.Range(-1, 1);
            }
            //Call the AttemptMove function and pass in the generic parameter Bandit, because Enemy is moving and expecting to potentially encounter a Bandit
            AttemptMove <Bandit> (xDir, yDir);

        }

        protected override void OnCantMove <T> (T component)
        {
			Bandit hitPlayer = component as Bandit;

			animator.SetTrigger ("enemyAttack");
            if(hitPlayer){
                GameObject.FindWithTag("Player").GetComponent<Bandit>().DecrementHealthFromPlayer();
            }
            hitPlayer.DecrementHealthFromPlayer();

			SoundManager.instance.RandomizeSfx (attackSound1, attackSound2);
        }

        IEnumerator DamagePlayer()
        {
            animator.SetTrigger("enemyAttack");
            player.DecrementHealthFromPlayer();
            SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);
            yield return new WaitForSeconds(3f);
            isAttackingPlayer = false;
        }

        //DamageEnemy is called when the player attacks a enemy.
        IEnumerator DamageEnemy(int loss)
        {
            //Subtract loss from hit point total.
            hp -= loss;
            yield return new WaitForSeconds(.3f);

            //Call the RandomizeSfx function of SoundManager to play one of two chop sounds.
            SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);

            yield return new WaitForSeconds(.3f);
            //If hit points are less than or equal to zero:
            healthBar.SetHealth(hp, originalHp);
            if (hp <= 0)
            {
                healthBar.SetHealth(0, originalHp);
                //Disable the gameObject.
                DeleteEnemy();

                if (player.firstKill == false)
                {
                    player.firstKill = true;
                    int level = CaveGameManager.instance.GetLevel();
                    if (level == 6 || level == 11 || level == 16 || level == 21)
                    {
                        float x = transform.position.x + Random.Range(-0.5f, 0.5f);
                        float y = transform.position.y + Random.Range(-0.5f, 0.5f);
                        GameObject.FindWithTag("WeaponManager").GetComponent<WeaponManager>().GenerateWeapon(new Vector2(x, y), false);
                        for (int i = 0; i < 4; i++)
                        {
                            x = transform.position.x + Random.Range(-0.5f, 0.5f);
                            y = transform.position.y + Random.Range(-0.5f, 0.5f);
                            itemSpawn.SpawnItem(x, y, 5, false, 9);
                        }
                    }
                    // spawn food as reward
                    else
                    {
                        SpawnFoodReward();
                    }
                }
                else
                {
                    SpawnFoodReward();
                }
            }

        }

        public void SpawnFoodReward() {
            int foodType = 4;
            if (originalHp >= 0 * multiplier && originalHp < 20 * multiplier)
            {
                foodType = 4;
            }
            // generate drinks
            else if (originalHp >= 20 * multiplier && originalHp < 30 * multiplier)
            {
                foodType = 6;
            }
            // generate veg
            else if (originalHp >= 30 * multiplier && originalHp < 40 * multiplier)
            {
                foodType = 8;
                float x = transform.position.x + Random.Range(-0.5f, 0.5f);
                float y = transform.position.y + Random.Range(-0.5f, 0.5f);
                GameObject.FindWithTag("WeaponManager").GetComponent<WeaponManager>().GenerateWeapon(new Vector2(x, y), false);
            }
            // generate meat
            else if (originalHp > 40 * multiplier)
            {
                foodType = 9;
                float x = transform.position.x + Random.Range(-0.5f, 0.5f);
                float y = transform.position.y + Random.Range(-0.5f, 0.5f);
                GameObject.FindWithTag("WeaponManager").GetComponent<WeaponManager>().GenerateWeapon(new Vector2(x, y), true);
            }
            if (foodType > 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float x = transform.position.x + Random.Range(-0.5f, 0.5f);
                    float y = transform.position.y + Random.Range(-0.5f, 0.5f);
                    itemSpawn.SpawnItem(x, y, 5, false, foodType);
                }
            }
        }

        public void DeleteEnemy()
        {
            gameObject.SetActive(false);
        }

        public void RemoveEnemy(){
            Destroy(gameObject);
        }

    }
}