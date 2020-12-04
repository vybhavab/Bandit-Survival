using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Bandit player;

    void Awake()
    {
        player =  GameObject.Find("LightBandit").GetComponent<Bandit>();
    }
    // Start is called before the first frame update
    void Start()
    {
        MapGenerator map = GameObject.Find("MapGenerator").GetComponent<MapGenerator>();
        transform.position = map.randomStartingLocation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3 (player.transform.position.x, player.transform.position.y, -10);
    }
}
