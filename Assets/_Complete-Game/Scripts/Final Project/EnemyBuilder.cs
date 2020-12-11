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

        public List<GameObject> currentLevelBosses = new List<GameObject>();

        protected override void Start()
        {
            CaveGameManager.instance.AddEnemyToList (this);
            animator = GetComponent<Animator> ();
            target = GameObject.FindGameObjectWithTag ("Player").transform;
            Debug.Log(target);
            base.Start();
        }

        // Update is called once per frame
        void Update()
        {

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
            int xDir = 0;
            int yDir = 0;

            if(Mathf.Abs (target.position.x - target.position.x) < float.Epsilon)
            {
                yDir = target.position.y > transform.position.y ? 1 : -1;
            }else
            {
                xDir = target.position.x > transform.position.x ? 1 : -1;
            }

            AttemptMove <Player> (xDir, yDir);

        }

        protected override void OnCantMove <T> (T component)
        {
			Player hitPlayer = component as Player;

			//Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
			Debug.Log("REDUCE HEALTH");

			animator.SetTrigger ("enemyAttack");

			SoundManager.instance.RandomizeSfx (attackSound1, attackSound2);
        }

    }
}