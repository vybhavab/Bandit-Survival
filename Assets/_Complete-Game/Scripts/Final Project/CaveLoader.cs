using UnityEngine;
using System.Collections;

namespace Completed
{
	public class CaveLoader : MonoBehaviour
	{
		public GameObject caveGameManager;          //GameManager prefab to instantiate.
		public GameObject soundManager;         //SoundManager prefab to instantiate.


		void Awake()
		{
			//Check if a GameManager has already been assigned to static variable GameManager.instance or if it's still null
			/*if (CaveGameManager.instance == null)

				//Instantiate gameManager prefab
				Instantiate(caveGameManager);
			*/
			//Check if a SoundManager has already been assigned to static variable GameManager.instance or if it's still null
			if (SoundManager.instance == null)

				//Instantiate SoundManager prefab
				Instantiate(soundManager);
		}
	}
}