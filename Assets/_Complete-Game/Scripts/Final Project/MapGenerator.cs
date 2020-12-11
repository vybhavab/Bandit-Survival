using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


namespace Completed
{
    public class MapGenerator : MonoBehaviour
    {
        public int baseWidth;
        public int baseHeight;
        public int primaryIterations;
        public int secondaryIterations;
        public int wallRule;

        public GameObject wallParent; //For organizing all wall objects under one parent to avoid cluttering
        public GameObject[] wallTiles;
        public GameObject floorParent; //For organizing all floor objects under one parent to avoid cluttering
        public GameObject[] floorTiles;
        public GameObject exitTile;
        public GameObject symbolExitTile;
        List<GameObject> currentLevelTiles = new List<GameObject>();

        ItemSpawn itemSpawn;

        public string seed;
        public bool randomSeed;
        public bool processMap;

        public List<Coordinate> mapFloorTiles = new List<Coordinate>();
        public List<Coordinate> mapEdgeTiles = new List<Coordinate>();
        List<List<Coordinate>> allMapRooms = new List<List<Coordinate>>();//allMapRooms is all original rooms including rooms 16 tiles or smaller
        public List<Room> originalRooms = new List<Room>();//originalRooms is all rooms larger than 16 tiles
        public Vector2 randomStartingLocation;
        Coordinate exitCoord = new Coordinate();
        List<Coordinate> passageEdgeTiles = new List<Coordinate>(); //to ensure exit isn't generated in a passage which would block players path to the rest of the map
        public int numberofFloorTiles;

        [Range(0, 100)]
        public int randomWallValue; //used for the randomly generated starting point for the cellular automata

        int[,] map;



        public void GenerateFirstMap()
        {
            if (currentLevelTiles.Count > 1)
            {
                DeleteMap();
            }
            exitCoord = new Coordinate();
            mapFloorTiles = new List<Coordinate>();
            mapEdgeTiles = new List<Coordinate>();
            allMapRooms = new List<List<Coordinate>>();
            originalRooms = new List<Room>();
            passageEdgeTiles = new List<Coordinate>();
            randomStartingLocation = new Vector2();
            itemSpawn = GameObject.FindWithTag("GameManager").GetComponent<ItemSpawn>();

            GenerateMap();
            GenerateStartAndExit();
            DrawMap();

            foreach (Coordinate coord in mapFloorTiles)
            {
                int randomNumber = UnityEngine.Random.Range(0, 1000);
                itemSpawn.SpawnItem(-baseWidth / 2 - 0.5f + coord.x, -baseHeight / 2 - 0.5f + coord.y, randomNumber); 
            }

            Debug.Log(currentLevelTiles.Count);
            print(Time.realtimeSinceStartup); //just for seeing how long maps of different size take to generate to find a reasonable upper bound on map size
        }

        public void GenerateNewMap()
        {
            Bandit player = GameObject.Find("LightBandit").GetComponent<Bandit>();
            DeleteMap();
            GenerateMap();
            GenerateStartAndExit();
            DrawMap();
            player.enabled = true;
            player.playerPosition = randomStartingLocation;
            player.transform.position = player.playerPosition;
        }

        void GenerateMap()
        {
            map = new int[baseWidth, baseHeight];
            InitializeMap();

            for (int i = 0; i < primaryIterations; i++)
            {
                ApplyPrimaryCARules();
            }

            for (int i = 0; i < secondaryIterations; i++)
            {
                ApplySecondaryCARules();
            }

            if (processMap)
            {
                ProcessMap();
            }
            else
            {
                originalRooms = RegionsToRooms(0);//If proccess map is disabled, this allows the exit to be placed in a room away from the player. To make a playable experience with process map disabled the player will need a way to destroy walls.
            }
        }

        void ProcessMap()
        {
            List<List<Coordinate>> wallChunks = FindRooms(1);
            int wallThresholdSize = 16; //Will delete any wall chunks smaller than 16, this keeps the map looking cleaner
            foreach (List<Coordinate> wallChunk in wallChunks)
            {
                if (wallChunk.Count < wallThresholdSize)
                {
                    foreach (Coordinate tile in wallChunk)
                    {
                        map[tile.x, tile.y] = 0;
                    }
                }
            }

            allMapRooms = FindRooms(0); //For keeping track of all the different regions once the rooms are connected
            int roomThresholdSize = 16;
            List<Room> largeRooms = new List<Room>();

            foreach (List<Coordinate> room in allMapRooms) //Add all rooms larger than 16 to the list of rooms to be connected. There's no point in connecting the small rooms. They're not changed to walls just in case we want to add a way to destroy walls and place good loot in the small unconnected rooms to incentivize exploration of the whole map.
            {
                if (room.Count > roomThresholdSize)
                {
                    largeRooms.Add(new Room(room, map));
                }
            }

            originalRooms = largeRooms; //Keeps track of the original list of large rooms before all the rooms are connected. This is used to make sure the exit isn't placed in the same region as the player and can also be used to make sure each region has something in it, like loot or food.
            bool allRoomsConnected = true;

            if (largeRooms.Count > 1)
            {
                allRoomsConnected = false;
            }

            while (!allRoomsConnected)
            {
                ConnectRooms(largeRooms);
                List<List<Coordinate>> rooms = FindRooms(0); //If there is more than 1 large room, they are connected then FindRooms is called again until all the rooms are connected so there is essentially 1 large room.
                largeRooms = new List<Room>();
                foreach (List<Coordinate> room in rooms)
                {
                    if (room.Count > roomThresholdSize)
                    {
                        largeRooms.Add(new Room(room, map));
                    }
                }
                if (largeRooms.Count == 1)
                {
                    allRoomsConnected = true;
                }

            }
        }

        void ConnectRooms(List<Room> rooms)
        {
            List<Room> roomsA = new List<Room>();
            List<Room> roomsB = new List<Room>();

            //roomsA and roomsB are to compare each room to every other room to find the closest room to make a connection
            roomsA = rooms;
            roomsB = rooms;

            int distance = 0;
            Coordinate closestTileA = new Coordinate();
            Coordinate closestTileB = new Coordinate();
            Room closestRoomA = new Room();
            Room closestRoomB = new Room();
            bool possibleConnectionFound = false;

            foreach (Room roomA in roomsA)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0) //if the room already has a connection (i.e. it was the closestRoomB found in a previous iteration) skip it and go to the next room, otherwise each room could have several connections
                {
                    continue;
                }

                foreach (Room roomB in roomsB)
                {
                    if (roomA == roomB)
                    {
                        continue;
                    }

                    for (int indexA = 0; indexA < roomA.edgeTiles.Count; indexA++)
                    {
                        for (int indexB = 0; indexB < roomB.edgeTiles.Count; indexB++)
                        {
                            Coordinate tileA = roomA.edgeTiles[indexA];
                            Coordinate tileB = roomB.edgeTiles[indexB];
                            int distanceAToB = (int)(Mathf.Pow(tileA.x - tileB.x, 2) + Mathf.Pow(tileA.y - tileB.y, 2)); //don't need to take the square root because this is just used for comparison

                            if (distanceAToB < distance || !possibleConnectionFound)
                            {
                                distance = distanceAToB;
                                possibleConnectionFound = true;
                                closestTileA = tileA;
                                closestTileB = tileB;
                                closestRoomA = roomA;
                                closestRoomB = roomB;
                            }
                        }
                    }
                }

                if (possibleConnectionFound)
                {
                    CreatePassage(closestRoomA, closestRoomB, closestTileA, closestTileB);
                    passageEdgeTiles.Add(closestTileA);
                    passageEdgeTiles.Add(closestTileB);
                }
            }


        }

        void CreatePassage(Room roomA, Room roomB, Coordinate tileA, Coordinate tileB) //Changes walls to floors on the line between tileA and tileB based on discretized version of slope formula
        {
            Room.ConnectRooms(roomA, roomB);
            int dY = tileB.y - tileA.y;
            int dX = tileB.x - tileA.x;
            int j;

            //Cases where closest tiles are in line (dY or dX == 0)
            if (dY == 0 && dX > 0)
            {
                for (int i = tileA.x + 1; i <= tileB.x; i++)
                {
                    map[i, tileA.y] = 0;
                }
            }
            else if (dY == 0 && dX < 0)
            {
                for (int i = tileA.x - 1; i >= tileB.x; i--)
                {
                    map[i, tileA.y] = 0;
                }
            }
            else if (dX == 0 && dY > 0)
            {
                for (int i = tileA.y + 1; i <= tileB.y; i++)
                {
                    map[tileA.x, i] = 0;
                }
            }
            else if (dX == 0 && dY < 0)
            {
                for (int i = tileA.y - 1; i >= tileB.y; i--)
                {
                    map[tileA.x, i] = 0;
                }
            }

            //Cases where the slope is 1 or -1
            else if (dX > 0 && dY > 0 && dX == dY)
            {
                j = 0;
                for (int i = tileA.x + 1; i <= tileB.x; i++)
                {
                    map[i, tileA.y + j] = 0;
                    map[i, tileA.y + j + 1] = 0;
                    j++;
                }
            }
            else if (dX > 0 && dY < 0 && dX == -dY)
            {
                j = 0;
                for (int i = tileA.x + 1; i <= tileB.x; i++)
                {
                    map[i, tileA.y - j] = 0;
                    map[i, tileA.y - j - 1] = 0;
                    j++;
                }
            }
            else if (dX < 0 && dY < 0 && dX == dY)
            {
                j = 0;
                for (int i = tileA.x - 1; i >= tileB.x; i--)
                {
                    map[i, tileA.y - j] = 0;
                    map[i, tileA.y - j - 1] = 0;
                    j++;
                }
            }
            else if (dX < 0 && dY > 0 && -dX == dY)
            {
                j = 0;
                for (int i = tileA.x - 1; i >= tileB.x; i--)
                {
                    map[i, tileA.y + j] = 0;
                    map[i, tileA.y + j + 1] = 0;
                    j++;
                }
            }
            //Cases where slope is not equal to 1 or -1 and dY and dX are not equal to 0
            else if (dX > 0 && dY > 0 && dX > dY)
            {
                j = 0;
                for (int i = 1; i <= dX; i++)
                {
                    map[tileA.x + i, tileA.y + j] = 0;
                    j = Mathf.FloorToInt(((float)dY / (float)dX) * i);
                    map[tileA.x + i, tileA.y + j] = 0;
                }
            }
            else if (dX > 0 && dY < 0 && dX > -dY)
            {
                j = 0;
                for (int i = 1; i <= dX; i++)
                {
                    map[tileA.x + i, tileA.y + j] = 0;
                    j = -Mathf.FloorToInt(Mathf.Abs(((float)dY / (float)dX) * i));
                    map[tileA.x + i, tileA.y + j] = 0;
                }
            }
            else if (dX < 0 && dY < 0 && -dX > -dY)
            {
                j = 0;
                for (int i = -1; i >= dX; i--)
                {
                    map[tileA.x + i, tileA.y + j] = 0;
                    j = -Mathf.FloorToInt(Mathf.Abs(((float)dY / (float)dX) * i));
                    map[tileA.x + i, tileA.y + j] = 0;
                }
            }
            else if (dX < 0 && dY > 0 && -dX > dY)
            {
                j = 0;
                for (int i = -1; i >= dX; i--)
                {
                    map[tileA.x + i, tileA.y + j] = 0;
                    j = Mathf.FloorToInt(((float)dY / (float)dX) * i);
                    map[tileA.x + i, tileA.y + j] = 0;
                }
            }

            else if (dX > 0 && dY > 0 && dY > dX)
            {
                j = 0;
                for (int i = 1; i <= dY; i++)
                {
                    map[tileA.x + j, tileA.y + i] = 0;
                    j = Mathf.FloorToInt(((float)dX / (float)dY) * i);
                    map[tileA.x + j, tileA.y + i] = 0;
                }
            }
            else if (dX > 0 && dY < 0 && -dY > dX)
            {
                j = 0;
                for (int i = -1; i >= dY; i--)
                {
                    map[tileA.x + j, tileA.y + i] = 0;
                    j = Mathf.FloorToInt(((float)dX / (float)dY) * i);
                    map[tileA.x + j, tileA.y + i] = 0;
                }
            }
            else if (dX < 0 && dY < 0 && -dY > -dX)
            {
                j = 0;
                for (int i = -1; i >= dY; i--)
                {
                    map[tileA.x + j, tileA.y + i] = 0;
                    j = -Mathf.FloorToInt(Mathf.Abs(((float)dX / (float)dY) * i));
                    map[tileA.x + j, tileA.y + i] = 0;
                }
            }
            else if (dX < 0 && dY > 0 && dY > -dX)
            {
                j = 0;
                for (int i = 1; i <= dY; i++)
                {
                    map[tileA.x + j, tileA.y + i] = 0;
                    j = -Mathf.FloorToInt(Mathf.Abs(((float)dX / (float)dY) * i));
                    map[tileA.x + j, tileA.y + i] = 0;
                }
            }
        }

        List<List<Coordinate>> FindRooms(int tileType)
        {
            List<List<Coordinate>> rooms = new List<List<Coordinate>>();
            int[,] tileChecked = new int[baseWidth, baseHeight];

            for (int x = 0; x < baseWidth; x++)
            {
                for (int y = 0; y < baseHeight; y++)
                {
                    if (tileChecked[x, y] == 0 && map[x, y] == tileType)
                    {
                        List<Coordinate> newRoom = new List<Coordinate>();
                        Queue<Coordinate> queue = new Queue<Coordinate>();
                        queue.Enqueue(new Coordinate(x, y));
                        tileChecked[x, y] = 1;

                        while (queue.Count > 0)
                        {
                            Coordinate tile = queue.Dequeue();
                            newRoom.Add(tile);

                            for (int i = tile.x - 1; i <= tile.x + 1; i++)
                            {
                                for (int j = tile.y - 1; j <= tile.y + 1; j++)
                                {
                                    if (i >= 0 && i < baseWidth && j >= 0 && j < baseHeight && (j == tile.y || i == tile.x))
                                    {
                                        if (tileChecked[i, j] == 0 && map[i, j] == tileType)
                                        {
                                            tileChecked[i, j] = 1;
                                            queue.Enqueue(new Coordinate(i, j));
                                        }
                                    }
                                }
                            }
                        }

                        rooms.Add(newRoom);
                    }
                }
            }
            return rooms;
        }

        void InitializeMap()
        {
            if (randomSeed)
            {
                seed = UnityEngine.Random.Range(-1000, 1000).ToString();
            }

            System.Random mapSeed = new System.Random(seed.GetHashCode());
            for (int x = 0; x < baseWidth; x++)
            {
                for (int y = 0; y < baseHeight; y++)
                {
                    if (x == 0 || x == baseWidth - 1 || y == 0 || y == baseHeight - 1)
                    {
                        map[x, y] = 1;
                    }
                    else
                    {
                        if (mapSeed.Next(0, 100) < randomWallValue)
                        {
                            map[x, y] = 1;
                        }
                        else
                        {
                            map[x, y] = 0;
                        }
                    }
                }
            }
        }

        void ApplyPrimaryCARules()
        {
            int[,] newmap = new int[baseWidth, baseHeight];
            for (int x = 0; x < baseWidth; x++)
            {
                for (int y = 0; y < baseHeight; y++)
                {
                    int neighborWallTiles = CountNeighborWalls(x, y, 1);
                    int openSpaceWallTiles = CountNeighborWalls(x, y, 2);

                    if (neighborWallTiles >= wallRule || openSpaceWallTiles <= 2)
                    {
                        newmap[x, y] = 1;
                    }
                    else if (neighborWallTiles < wallRule)
                    {
                        newmap[x, y] = 0;
                    }
                }
            }
            map = newmap;
        }

        void ApplySecondaryCARules()
        {
            int[,] newmap = map;
            for (int x = 0; x < baseWidth; x++)
            {
                for (int y = 0; y < baseHeight; y++)
                {
                    int neighborWallTiles = CountNeighborWalls(x, y, 1);

                    if (neighborWallTiles > wallRule)
                    {
                        newmap[x, y] = 1;
                    }
                    else if (neighborWallTiles < wallRule)
                    {
                        newmap[x, y] = 0;
                    }
                }
            }
            map = newmap;
        }

        int CountNeighborWalls(int baseX, int baseY, int n)
        {
            int wallCount = 0;
            for (int x = baseX - n; x <= baseX + n; x++)
            {
                for (int y = baseY - n; y <= baseY + n; y++)
                {
                    if (x >= 0 && x < baseWidth && y >= 0 && y < baseHeight) //make sure the neighboring tile is actually on the map
                    {
                        if (x != baseX || y != baseY)
                        {
                            wallCount += map[x, y];
                        }
                    }
                    else
                    {
                        wallCount++; //If neighboring tile is outside of the map, treat it as a wall so a border tile doesn't get changed to a floor tile
                    }
                }
            }

            return wallCount;
        }

        void GenerateStartAndExit()
        {
            //If map is processed, set fullMapRoom equal to the entire room and make sure randomStartingLocation isn't in one of the small regions that aren't connected. If map isn't proccessed, set fullMapRoom equal to largest room
            Room fullMapRoom = new Room(allMapRooms[0], map);
            foreach (List<Coordinate> room in allMapRooms)
            {
                if (room.Count > fullMapRoom.tiles.Count)
                {
                    fullMapRoom = new Room(room, map);
                }
            }
            mapFloorTiles = fullMapRoom.tiles;
            mapEdgeTiles = fullMapRoom.edgeTiles;
            int randomStartRoomIndex = UnityEngine.Random.Range(0, originalRooms.Count - 1);
            Room randomStartRoom = originalRooms[randomStartRoomIndex];
            int randomStartTileIndex = UnityEngine.Random.Range(0, randomStartRoom.tiles.Count - 1);
            Coordinate randomStartTile = randomStartRoom.tiles[randomStartTileIndex];
            randomStartingLocation = new Vector2(-baseWidth / 2 - 0.5f + randomStartTile.x, -baseHeight / 2 - 0.5f + randomStartTile.y);

            //if there was more than 1 original room before processing, place exit in a room separate from the starting location so that it isn't too close to the player
            int randomExitPicker;
            List<Room> possibleExitRooms = originalRooms;
            Room exitRoom = new Room();
            Room roomToRemove = new Room();
            bool startingRoomFound = false;
            if (originalRooms.Count > 1)
            {
                foreach (Room room in possibleExitRooms)
                {
                    if (room == randomStartRoom)
                    {
                        roomToRemove = room;
                        startingRoomFound = true;
                    }

                }
                if (startingRoomFound)
                {
                    possibleExitRooms.Remove(roomToRemove);
                }


                randomExitPicker = UnityEngine.Random.Range(0, possibleExitRooms.Count - 1);
                exitRoom = possibleExitRooms[randomExitPicker];
                Coordinate edgeTileToRemove = new Coordinate();
                bool passageEdgeTileFound = false;

                foreach (Coordinate edgeTile in exitRoom.edgeTiles)
                {
                    if (passageEdgeTiles.Contains(edgeTile))
                    {
                        edgeTileToRemove = edgeTile;
                        passageEdgeTileFound = true;
                    }
                }

                if (passageEdgeTileFound)
                {
                    exitRoom.edgeTiles.Remove(edgeTileToRemove);
                }
                exitCoord = exitRoom.edgeTiles[UnityEngine.Random.Range(0, exitRoom.edgeTiles.Count - 1)];
            }
            else
            {
                exitRoom = originalRooms[0];
                exitCoord = exitRoom.edgeTiles[UnityEngine.Random.Range(0, exitRoom.edgeTiles.Count - 1)];
            }
        }

        List<Room> RegionsToRooms(int tileType)
        {
            allMapRooms = FindRooms(tileType);
            int roomThresholdSize = 16;
            List<Room> largeRooms = new List<Room>();


            foreach (List<Coordinate> room in allMapRooms)
            {
                if (room.Count > roomThresholdSize)
                {
                    largeRooms.Add(new Room(room, map));
                }
            }
            return largeRooms;
        }
        void DrawMap()
        {
            numberofFloorTiles = 0;
            int randomFloor = floorTiles.Length;
            int randomWall = wallTiles.Length;
            wallParent = GameObject.Find("Walls");
            floorParent = GameObject.Find("Floors");
            if (map != null)
            {
                for (int x = 0; x < baseWidth; x++)
                {
                    for (int y = 0; y < baseHeight; y++)
                    {
                        Vector2 tilePosition = new Vector2(-baseWidth / 2 - 0.5f + x, -baseHeight / 2 - 0.5f + y);
                        if (map[x, y] == 1)
                        {
                            GameObject wall = (GameObject)Instantiate(wallTiles[UnityEngine.Random.Range(0, randomWall - 1)], tilePosition, Quaternion.identity);
                            wall.transform.SetParent(wallParent.transform);
                            currentLevelTiles.Add(wall);
                        }
                        else if (map[x, y] == 0)
                        {
                            if (x == exitCoord.x && y == exitCoord.y)
                            {
                                GameObject exit = (GameObject)Instantiate(exitTile, tilePosition, Quaternion.identity);
                                currentLevelTiles.Add(exit);

                                GameObject symExit = (GameObject)Instantiate(symbolExitTile, tilePosition, Quaternion.identity);
                                GameObject.FindWithTag("GameManager").GetComponent<ItemSpawn>().currentLevelItems.Add(symExit);
                            }
                            else
                            {
                                GameObject floor = (GameObject)Instantiate(floorTiles[UnityEngine.Random.Range(0, randomFloor - 1)], tilePosition, Quaternion.identity);
                                floor.transform.SetParent(floorParent.transform);
                                currentLevelTiles.Add(floor);
                                numberofFloorTiles += 1;
                            }
                        }

                    }
                }
            }
        }

        void DeleteMap()
        {
            for (int i = 0; i < currentLevelTiles.Count; i++)
            {
                Destroy(currentLevelTiles[i]);
            }
            currentLevelTiles = new List<GameObject>();

            itemSpawn = GameObject.Find("MapGenerator").GetComponent<ItemSpawn>();
            itemSpawn.DeleteItems();
        }

        public void UpdateMapSize(int explorationCount)
        {
            float mapExplorationPercentage = (float)explorationCount / numberofFloorTiles;
            if (mapExplorationPercentage > 0.3f)
            {
                if (baseHeight + 10 > 128)
                {
                    baseHeight = 128;
                }
                else
                {
                    baseHeight = baseHeight + 10;
                }

                if (baseWidth + 10 > 128)
                {
                    baseWidth = 128;
                }
                else
                {
                    baseWidth = baseWidth + 10;
                }
            }

            else if (mapExplorationPercentage > 0.25f)
            {
                if (baseHeight + 5 > 128)
                {
                    baseHeight = 128;
                }
                else
                {
                    baseHeight = baseHeight + 5;
                }

                if (baseWidth + 5 > 128)
                {
                    baseWidth = 128;
                }
                else
                {
                    baseWidth = baseWidth + 5;
                }
            }

            else if (mapExplorationPercentage < 0.15f)
            {
                if (baseHeight - 10 < 32)
                {
                    baseHeight = 32;
                }
                else
                {
                    baseHeight = baseHeight - 10;
                }

                if (baseWidth - 10 < 32)
                {
                    baseWidth = 32;
                }
                else
                {
                    baseWidth = baseWidth - 10;
                }
                
            }

            else
            {
                if (baseHeight - 5 < 32)
                {
                    baseHeight = 32;
                }
                else
                {
                    baseHeight = baseHeight - 5;
                }

                if (baseWidth - 5 < 32)
                {
                    baseWidth = 32;
                }
                else
                {
                    baseWidth = baseWidth - 5;
                }
            }

        }

        public struct Coordinate
        {
            public int x;
            public int y;

            public Coordinate(int xIn, int yIn)
            {
                x = xIn;
                y = yIn;
            }
        }

        public class Room
        {
            public List<Coordinate> tiles;
            public List<Coordinate> edgeTiles;
            public List<Room> connectedRooms;

            public Room()
            {

            }

            public Room(List<Coordinate> tilesIn, int[,] map)
            {
                tiles = tilesIn;
                connectedRooms = new List<Room>();

                edgeTiles = new List<Coordinate>();
                foreach (Coordinate tile in tiles)
                {
                    int xLeft = tile.x - 1;
                    int xRight = tile.x + 1;
                    int yUp = tile.y + 1;
                    int yDown = tile.y - 1;

                    //check if there is a wall adjacent to the tile, if there is add the tile to the list of edge tiles.
                    if (map[xLeft, tile.y] == 1 || map[xRight, tile.y] == 1 || map[tile.x, yUp] == 1 || map[tile.x, yDown] == 1)
                    {
                        edgeTiles.Add(tile);
                    }
                }
            }

            public static void ConnectRooms(Room roomA, Room roomB)
            {
                roomA.connectedRooms.Add(roomB);
                roomB.connectedRooms.Add(roomA);
            }
        }
    }
}
