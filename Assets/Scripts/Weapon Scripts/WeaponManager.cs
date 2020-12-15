using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager wm;

    public GameObject blankWeapon;
    public GameObject[] handles, hilts, blades;
    public GameObject symbolWeaponTile;

    public void GenerateWeapon(Vector2 pos)
    {
        GameObject weapon = Instantiate(blankWeapon, (Vector3)pos, Quaternion.identity);

        GameObject handle = Instantiate(handles[Random.Range(0, handles.Length)], weapon.transform);
        handle.transform.localPosition = Vector3.zero;
        handle.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        GameObject hilt = Instantiate(hilts[Random.Range(0, hilts.Length)], weapon.transform);
        hilt.transform.localPosition = Vector3.zero;
        hilt.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        GameObject blade = Instantiate(blades[Random.Range(0, blades.Length)], weapon.transform);
        blade.transform.localPosition = Vector3.zero;
        blade.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

        weapon.GetComponent<Weapon>().SetParts(handle, hilt, blade);
        GameObject.FindWithTag("GameManager").GetComponent<ItemSpawn>().currentLevelItems.Add(weapon);
        GameObject symWeapon = (GameObject)Instantiate(symbolWeaponTile, pos, Quaternion.identity);
        GameObject.FindWithTag("GameManager").GetComponent<ItemSpawn>().currentLevelItems.Add(symWeapon);
    }
    
    void Awake()
    {
        wm = this;
    }
}
