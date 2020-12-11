using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawn : MonoBehaviour
{
    public GameObject[] fruitTiles;
    public GameObject[] vegTiles;
    public GameObject[] drinkTiles;
    public GameObject[] meatTiles;
    public GameObject symbolFoodTile;


    public int fruitWeighting = 4;
    public int vegWeighting = 2;
    public int drinkWeighting = 2;
    public int meatWeighting = 1;

    int totalFoodWeighting;

    WeaponManager weaponManager;

    public int foodSpawnPercent = 10;
    public int weaponSpawnPercent = 4;

    public List<GameObject> currentLevelItems = new List<GameObject>();

    private void Start()
    {
        totalFoodWeighting = fruitWeighting + vegWeighting + drinkWeighting + meatWeighting;
    }
    public void SpawnItem(float x, float y, int randomNumber)
    {
        if (randomNumber <= weaponSpawnPercent)
        {
           GameObject.FindWithTag("WeaponManager").GetComponent<WeaponManager>().GenerateWeapon(new Vector2(x, y));
        }
        else if (randomNumber <= weaponSpawnPercent + foodSpawnPercent)
        {
            int randomFoodSpawn = Random.Range(1, totalFoodWeighting);
            if (randomFoodSpawn <= fruitWeighting)
            {
                int fruitToSpawn = Random.Range(0, fruitTiles.Length - 1);
                Vector2 vec = new Vector2(x, y);

                GameObject fruit = (GameObject)Instantiate(fruitTiles[fruitToSpawn], vec, Quaternion.identity);
                currentLevelItems.Add(fruit);
                GameObject symFood = (GameObject)Instantiate(symbolFoodTile, vec, Quaternion.identity);
                currentLevelItems.Add(symFood);
            }
            else if (randomFoodSpawn <= fruitWeighting + vegWeighting)
            {
                int vegToSpawn = Random.Range(0, vegTiles.Length - 1);
                Vector2 vec = new Vector2(x, y);

                GameObject veg = (GameObject)Instantiate(vegTiles[vegToSpawn], vec, Quaternion.identity);
                currentLevelItems.Add(veg);
                GameObject symFood = (GameObject)Instantiate(symbolFoodTile, vec, Quaternion.identity);
                currentLevelItems.Add(symFood);
            }
            else if (randomFoodSpawn <= fruitWeighting + vegWeighting + drinkWeighting)
            {
                int drinkToSpawn = Random.Range(0, drinkTiles.Length - 1);
                Vector2 vec = new Vector2(x, y);

                GameObject drink = (GameObject)Instantiate(drinkTiles[drinkToSpawn], vec, Quaternion.identity);
                currentLevelItems.Add(drink);
                GameObject symFood = (GameObject)Instantiate(symbolFoodTile, vec, Quaternion.identity);
                currentLevelItems.Add(symFood);
            }
            else if (randomFoodSpawn <= fruitWeighting + vegWeighting + drinkWeighting + meatWeighting)
            {
                int meatToSpawn = Random.Range(0, meatTiles.Length - 1);
                Vector2 vec = new Vector2(x, y);

                GameObject meat = (GameObject)Instantiate(meatTiles[meatToSpawn], vec, Quaternion.identity);
                currentLevelItems.Add(meat);
                GameObject symFood = (GameObject)Instantiate(symbolFoodTile, vec, Quaternion.identity);
                currentLevelItems.Add(symFood);
            }

        }
    }

    public void updateFoodPercent(int newFoodPercent)
    {
        foodSpawnPercent = newFoodPercent;
    }

    public void updateWeaponPercent(int newWeaponPercent)
    {
        weaponSpawnPercent = newWeaponPercent;
    }

    public void DeleteItems()
    {
        for (int i = 0; i < currentLevelItems.Count; i++)
        {
            Destroy(currentLevelItems[i]);
        }
        currentLevelItems = new List<GameObject>();
    }





}
