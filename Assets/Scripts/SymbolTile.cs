using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed {
    public class SymbolTile : MonoBehaviour {
        // Start is called before the first frame update

        MapGenerator mapGenerator;
        GameObject[] symbolTiles;

        void Start() {
            mapGenerator = GameObject.FindWithTag("GameManager").GetComponent<MapGenerator>();
            symbolTiles = GameObject.FindGameObjectsWithTag("Symbol");

            float size = mapGenerator.baseHeight / 32;
            foreach (GameObject obj in symbolTiles) {
                obj.transform.localScale = new Vector3(size, size, 1);
            }
        }

        // Update is called once per frame
        void Update() {
            symbolTiles = GameObject.FindGameObjectsWithTag("Symbol");

            float size = mapGenerator.baseHeight / 32;
            foreach (GameObject obj in symbolTiles) {
                obj.transform.localScale = new Vector3(size, size, 1);
            }
           
        }
    }
}
