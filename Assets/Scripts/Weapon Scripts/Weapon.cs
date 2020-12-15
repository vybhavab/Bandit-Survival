using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int damage, speed, knockback;
    public int durability, maxDurability;
    public string weaponName;
    private bool showDam;
    public WeaponTextBehavior weaponText;


    Handle handle;
    Hilt hilt;
    Blade blade;

    public void Start(){
        showDam = false;
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

        showDam = true;
        int currDamage = GameObject.FindWithTag("Player").GetComponent<Bandit>().damage;
        bool isDamageIncrease = damage > currDamage;
        string text;
        if(isDamageIncrease){
            text = "+"
        }
        weaponText.SetText(text, showDam, isDamageIncrease);

    }
}
