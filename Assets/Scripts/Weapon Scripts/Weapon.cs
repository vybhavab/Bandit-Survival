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
            bool isDamageIncrease = damage >= player.damage;
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

        public void GetStats(bool toughEnemy)
        {
            if (CaveGameManager.instance.GetLevel() > 1)
                damageChange = GameObject.FindWithTag("GameManager").GetComponent<AIPlayerDamage>().GetDamageChange(CaveGameManager.instance.GetLevel());
            else
                damageChange = 0;
            player = GameObject.FindWithTag("Player").GetComponent<Bandit>();


            damage = handle.damage + hilt.damage + blade.damage + damageChange;
            knockback = handle.knockback + hilt.knockback + blade.knockback;
            speed = handle.speed + hilt.speed + blade.speed;
            durability = handle.durability + hilt.durability + blade.durability;
            maxDurability = durability;

            if (toughEnemy && player.damage >= damage)
            {
                if (CaveGameManager.instance.GetLevel() <= 5) damage = player.damage + 1;
                else if (CaveGameManager.instance.GetLevel() > 5 && CaveGameManager.instance.GetLevel() <= 10) damage = player.damage + 5;
                else if (CaveGameManager.instance.GetLevel() > 10 && CaveGameManager.instance.GetLevel() <= 15) damage = player.damage + 10;
                else if (CaveGameManager.instance.GetLevel() > 15 && CaveGameManager.instance.GetLevel() <= 20) damage = player.damage + 50;
                else damage = player.damage + 100;
            }
        }


        public void SetParts(GameObject handles, GameObject hilts, GameObject blades, bool toughEnemy)
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
                handle.damage = UnityEngine.Random.Range(15, 30);
                hilt.damage = UnityEngine.Random.Range(15, 30);
                blade.damage = UnityEngine.Random.Range(30, 50);
            }
            else if (CaveGameManager.instance.GetLevel() > 10 && CaveGameManager.instance.GetLevel() <= 15)
            {
                handle.damage = UnityEngine.Random.Range(30, 75);
                hilt.damage = UnityEngine.Random.Range(30, 75);
                blade.damage = UnityEngine.Random.Range(50, 150);
            }
            else if (CaveGameManager.instance.GetLevel() > 15 && CaveGameManager.instance.GetLevel() <= 20)
            {
                handle.damage = UnityEngine.Random.Range(75, 200);
                hilt.damage = UnityEngine.Random.Range(75, 200);
                blade.damage = UnityEngine.Random.Range(150, 500);
            }
            else
            {
                handle.damage = UnityEngine.Random.Range(200, 500);
                hilt.damage = UnityEngine.Random.Range(200, 500);
                blade.damage = UnityEngine.Random.Range(500, 1000);
            }

            GetStats(toughEnemy);
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