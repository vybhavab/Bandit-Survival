using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Completed
{
    public class Weapon : MonoBehaviour
    {
        public int damage, speed, knockback;
        public int durability, maxDurability;
        public string weaponName;
        public int damageChange;

        Handle handle;
        Hilt hilt;
        Blade blade;

        public Vector2 target;
        Bandit player;
        public Vector2 swordPosition;
        public Text weaponText;
        void Update()
        {
            player = GameObject.FindWithTag("Player").GetComponent<Bandit>();
            target = GameObject.FindGameObjectWithTag("Player").transform.position;
            if (CaveGameManager.instance.GetLevel() > 1)
                damageChange = GameObject.FindWithTag("GameManager").GetComponent<AIPlayerDamage>().GetDamageChange();
            else
                damageChange = 0;
            swordPosition = transform.position;
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (Mathf.Pow(target.x - swordPosition.x, 2) + Mathf.Pow(target.y - swordPosition.y, 2) <= 2)
                {
                    player.damage = damage + damageChange;
                    StartCoroutine(ShowWeaponChange(damage));
                    player.weaponName = weaponName;
                    gameObject.SetActive(false);
                }
            }
        }

        public void GetStats()
        {
            damage = handle.damage + hilt.damage + blade.damage;
            knockback = handle.knockback + hilt.knockback + blade.knockback;
            speed = handle.speed + hilt.speed + blade.speed;
            durability = handle.durability + hilt.durability + blade.durability;
            maxDurability = durability;
        }

        public void GetName()
        {
            weaponName = AINamesGenerator.Utils.GetRandomName();
            name = weaponName;
        }

        public void SetParts(GameObject handles, GameObject hilts, GameObject blades)
        {
            handle = handles.GetComponent<Handle>();
            handle.damage = UnityEngine.Random.Range(1, 5);
            hilt = hilts.GetComponent<Hilt>();
            hilt.damage = UnityEngine.Random.Range(1, 5);
            blade = blades.GetComponent<Blade>();
            blade.damage = UnityEngine.Random.Range(8, 20);

            GetStats();
            GetName();
        }

        IEnumerator ShowWeaponChange(int points)
        {
            weaponText = GameObject.Find("WeaponText").GetComponent<Text>();
            weaponText.text = "Obtained " + weaponName + "! Damage: " + points;
            yield return new WaitForSeconds(1);
        }
    }
}