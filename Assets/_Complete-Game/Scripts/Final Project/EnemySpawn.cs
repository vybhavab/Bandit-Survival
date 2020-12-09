using UnityEngine;
using System.Collections;

namespace Completed
{
    public class Enemys : MovingObject
    {
        // Start is called before the first frame update
        public int damageToPlayer;
        public AudioClip attackSound1;
        public AudioClip attackSound2;

        private Animator animator;
        private Transform target;
        private bool skipMove;

        protected override void Start()
        {
            CaveGameManager.instance.AddEnemyToList (this);
            animator = GetComponent<Animator> ();
            target = GameObject.FindGameObjectWithTag ("Player").transform;
            base.Start();
        }

        // Update is called once per frame
        void Update()
        {

        }

        protected override void AttemptMove <T> (int xDir, int yDit)
        {

        }

        public void MoveEnemy ()
        {

        }

        protected override void OnCantMove <T> (T component)
        {

        }

    }
}