using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_FinalProject : MonoBehaviour
{
    Vector2 playerPosition;
    Vector2 velocity;
    public float movementSpeed = 1;
    // Start is called before the first frame update
    void Start()
    {
        MapGenerator map = GameObject.Find("MapGenerator").GetComponent<MapGenerator>();
        playerPosition = map.randomStartingLocation;
        transform.position = playerPosition;
    }

    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 direction = input.normalized;
        velocity = direction * movementSpeed;
        transform.Translate(velocity * Time.deltaTime);
    }



}


