using UnityEngine;
using System.Collections;

namespace Completed
{
	public class Wall : MonoBehaviour
	{
		public AudioClip chopSound1;				//1 of 2 audio clips that play when the wall is attacked by the player.
		public AudioClip chopSound2;				//2 of 2 audio clips that play when the wall is attacked by the player.
		public Sprite dmgSprite;					//Alternate sprite to display after Wall has been attacked by player.
		public float hp = 20;							//hit points for the wall.
		
		
		private SpriteRenderer spriteRenderer;      //Store a component reference to the attached SpriteRenderer.

		public Vector2 target;
		Bandit player;
		public Vector2 wallPosition;

		void Awake ()
		{
			//Get a component reference to the SpriteRenderer.
			spriteRenderer = GetComponent<SpriteRenderer> ();
		}

		void Update()
		{
			player = GameObject.FindWithTag("Player").GetComponent<Bandit>();
			target = GameObject.FindGameObjectWithTag("Player").transform.position;
			wallPosition = transform.position;
			if (player.playerAttacking == true)
			{
				if (Mathf.Pow(target.x - wallPosition.x, 2) + Mathf.Pow(target.y - wallPosition.y, 2) <= 2)
				{
					if (target.x - wallPosition.x > 0 && player.facingLeft)
					{
						player.playerAttacking = false;
						StartCoroutine(DamageWall(player.damage));
					}
					else if (target.x - wallPosition.x < 0 && player.facingRight)
					{
						player.playerAttacking = false;
						StartCoroutine(DamageWall(player.damage));
					}
                }
            }
		}

		//DamageWall is called when the player attacks a wall.
		IEnumerator DamageWall (float loss)
		{
			//Set spriteRenderer to the damaged wall sprite.
			spriteRenderer.sprite = dmgSprite;
			
			//Subtract loss from hit point total.
			hp -= loss;
			
			yield return new WaitForSeconds(.3f);

			//Call the RandomizeSfx function of SoundManager to play one of two chop sounds.
			SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);

			yield return new WaitForSeconds(.3f);

			//If hit points are less than or equal to zero:
			if (hp <= 0)
			{
                MapGenerator map = GameObject.Find("MapGenerator").GetComponent<MapGenerator>();
				Vector2 newFloorPosition = transform.position;
				//Disable the gameObject.
				gameObject.SetActive(false);
				GameObject floor = (GameObject)Instantiate(map.floorTiles[Random.Range(0, map.floorTiles.Length - 1)], newFloorPosition, Quaternion.identity);
				floor.transform.SetParent(GameObject.Find("Floors").transform);
				map.currentLevelTiles.Add(floor);
			}
        }
	}
}
