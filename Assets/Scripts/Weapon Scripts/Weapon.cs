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
        public Vector2 target;
        Bandit player;
        public Vector2 swordPosition;
        public Text weaponText;

        private bool showDam;
        public WeaponTextBehavior wText;

        Handle handle;
        Hilt hilt;
        Blade blade;

        public void Start(){
            showDam = false;
        }


        void Update()
        {
            player = GameObject.FindWithTag("Player").GetComponent<Bandit>();
            target = GameObject.FindGameObjectWithTag("Player").transform.position;
            swordPosition = transform.position;
            showDam = true;
            bool isDamageIncrease = damage > player.damage;
            string text;
            if(isDamageIncrease){
                text = "+"+(damage-player.damage);
            }else{
                text = "-"+(player.damage-damage);
            }
            wText.SetText(text, showDam, isDamageIncrease);
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (Mathf.Pow(target.x - swordPosition.x, 2) + Mathf.Pow(target.y - swordPosition.y, 2) <= 2)
                {
                    player.damage = damage;
                    StartCoroutine(ShowWeaponChange(damage));
                    player.weaponName = weaponName;
                    gameObject.SetActive(false);
                }
            }
        }

        public void GetStats()
        {
            if (CaveGameManager.instance.GetLevel() > 1)
                damageChange = GameObject.FindWithTag("GameManager").GetComponent<AIPlayerDamage>().GetDamageChange(CaveGameManager.instance.GetLevel());
            else
                damageChange = 0;
            damage = handle.damage + hilt.damage + blade.damage + damageChange;
            knockback = handle.knockback + hilt.knockback + blade.knockback;
            speed = handle.speed + hilt.speed + blade.speed;
            durability = handle.durability + hilt.durability + blade.durability;
            maxDurability = durability;
        }


        public void SetParts(GameObject handles, GameObject hilts, GameObject blades)
        {
            handle = handles.GetComponent<Handle>();
            hilt = hilts.GetComponent<Hilt>();
            blade = blades.GetComponent<Blade>();

            if (CaveGameManager.instance.GetLevel() <= 5)
            {
                handle.damage = UnityEngine.Random.Range(1, 5);
                hilt.damage = UnityEngine.Random.Range(1, 5);
                blade.damage = UnityEngine.Random.Range(8, 20);
            }
            else if (CaveGameManager.instance.GetLevel() > 5 && CaveGameManager.instance.GetLevel() <= 10)
            {
                handle.damage = UnityEngine.Random.Range(5, 15);
                hilt.damage = UnityEngine.Random.Range(5, 15);
                blade.damage = UnityEngine.Random.Range(20, 50);
            }
            else if (CaveGameManager.instance.GetLevel() > 10 && CaveGameManager.instance.GetLevel() <= 15)
            {
                handle.damage = UnityEngine.Random.Range(15, 50);
                hilt.damage = UnityEngine.Random.Range(15, 50);
                blade.damage = UnityEngine.Random.Range(50, 150);
            }
            else
            {
                handle.damage = UnityEngine.Random.Range(50, 200);
                hilt.damage = UnityEngine.Random.Range(50, 200);
                blade.damage = UnityEngine.Random.Range(150, 500);
            }

            GetStats();
            GetName();
        }

        public void GetName()
        {
            weaponName = AINamesGenerator.Utils.GetRandomName();
            name = weaponName;
        }

        IEnumerator ShowWeaponChange(int points)
        {
            weaponText = GameObject.Find("WeaponText").GetComponent<Text>();
            weaponText.text = "Obtained " + weaponName + "! Damage: " + points;
            yield return new WaitForSeconds(1);
        }
    }
}