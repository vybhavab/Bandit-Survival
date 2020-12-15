using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed {
    public class MinimapFollow : MonoBehaviour {

        MapGenerator mapGenerator;
        Camera cam;

        private void Start() {
            mapGenerator = GameObject.FindWithTag("GameManager").GetComponent<MapGenerator>();
            cam = GameObject.FindWithTag("MiniCamera").GetComponent<Camera>();

            cam.transform.position = new Vector3(-1, -1, -20);
            cam.orthographicSize = mapGenerator.baseHeight / 2;
        }

        private void LateUpdate() {
            cam.transform.position = new Vector3(-1, -1, -20);
            cam.orthographicSize = mapGenerator.baseHeight / 2;
        }
    }
}

