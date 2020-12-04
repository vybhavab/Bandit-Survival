using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager wm;

    public GameObject blankWeapon;
    public GameObject[] handles, hilts, blades;
    public void GenerateWeapon(Vector2 pos)
    {
        GameObject weapon = Instantiate(blankWeapon, (Vector3)pos, Quaternion.identity);

        GameObject handle = Instantiate(handles[Random.Range(0, handles.Length)], weapon.transform);
        handle.transform.localPosition = Vector3.zero;
        GameObject hilt = Instantiate(hilts[Random.Range(0, hilts.Length)], weapon.transform);
        hilt.transform.localPosition = Vector3.zero;
        GameObject blade = Instantiate(blades[Random.Range(0, blades.Length)], weapon.transform);
        blade.transform.localPosition = Vector3.zero;

        weapon.GetComponent<Weapon>().SetParts(handle, hilt, blade);
    }
    
    void Awake()
    {
        wm = this;
    }

    // Start is called before the first frame update
    public void Start()
    {
        for(int i =0; i < 10; i++)
        {
            GenerateWeapon(new Vector2(i, 0));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
