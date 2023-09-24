using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.AI;
using Unity.MLAgents;

// public class Room
// {
//     public int x = 0;
//     public int y = 0;
//     public int w = 0;
//     public int h = 0;
//     public Room connectedTo = null;
//     public int branch = 0;
//     public string relative_positioning = "x";
//     public bool dead_end = false;
//     public int room_id = 0;

//     public int room_height = 0;
// }

// public class SpawnList
// {
//     public int x;
//     public int y;
//     public bool byWall;
//     public string wallLocation;
//     public bool inTheMiddle;
//     public bool byCorridor;

//     public int asDoor = 0;
//     public Room room = null;
//     public bool spawnedObject;
// }

// [System.Serializable]
// public class SpawnOption
// {
//     public int minSpawnCount;
//     public int maxSpawnCount;
//     public bool spawnByWall;
//     public bool spawmInTheMiddle;
//     public bool spawnRotated;
//     //public bool byCorridor;
//     [Tooltip("This is for make spawned object will be higher than ground.")]
//     public float heightFix = 0;

//     public GameObject gameObject;
//     [Tooltip("Use 0 for random room, make sure spawn room isnt bigger than your room count")]
//     public int spawnRoom = 0;
// }

// [System.Serializable]
// public class CustomRoom
// {
//     [Tooltip("make sure room id isnt bigger than your room count")]
//     public int roomId = 1;
//     public GameObject floorPrefab;
//     public GameObject wallPrefab;
//     public GameObject doorPrefab;
//     public GameObject cornerPrefab;
// }

// public class MapTile
// {
//     public int x = 0;
//     public int y = 0;
//     public int z = 0;

//     public int type = 0; //Default = 0 , Room Floor = 1, Wall = 2, Corridor Floor 3, Room Corners = 4, 5, 6 , 7
//     public int orientation = 0;
//     public Room room = null;
//     public int tile_height = 0;

//     public bool isCorner = false;
//     public bool isEdge = false;
//     public bool isDoor = false;
//     public int doorDirection = 0;
//     public bool byCorridor = false;
//     public string edgeLocation = "";

//     public bool isWall()
//     {
//         if (this.type >= 8 && this.type <= 11) return true;
//         return false;
//     }

// }

public class Corridor
{
    public int StartX { get; set; }
    public int StartY { get; set; }
    public int EndX { get; set; }
    public int EndY { get; set; }
    public int Width { get; set; }
    public int Length { get; set; }

    public void CalculateLength()
    {
        if (StartX == EndX)
            Length = Mathf.Abs(EndY - StartY);
        else
            Length = Mathf.Abs(EndX - StartX);
    }
}

public class ModernRoomGenerator : MonoBehaviour
{
    [Tooltip("This prefab will be instantiate on dungeons entrance. You can put your character, or character spawner here.")]
    public GameObject startPrefab;

    [Tooltip("This will be end of level. ")]
    public GameObject exitPrefab;

    public List<SpawnList> spawnedObjectLocations = new List<SpawnList>();
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject doorPrefab;
    //public GameObject doorCorners;
    public GameObject corridorFloorPrefab;
    public GameObject corridorWallPrefab;

    public GameObject cornerPrefab;
    public bool cornerRotation = false;

    public int maximumRoomCount = 10;
    [Header("Room Count Randomization")]
    public bool randomizeRoomCount = false;
    public int maxRandomRooms = -1;
    [Tooltip("Min gap between rooms. Also affects corridor lengths ")]
    public int minimumRoomMargin = 0;

    [Tooltip("Maximum gap between rooms. Also affects corridor lengths ")]
    public int roomMargin = 3;

    [Tooltip("Width of Corridor.")]
    public int corridorWidth = 2; // Change to desired width

    [Tooltip("If Checked: makes dungeon reset on every time level loads.")]
    public bool generate_on_load = true;
    public int minRoomSize = 5;
    public int maxRoomSize = 10;

    [Tooltip("How big are your tiles? (Affects corridor and room sizes)")]
    public float tileScaling = 1f;
    public List<SpawnOption> spawnOptions = new List<SpawnOption>();
    public List<CustomRoom> customRooms = new List<CustomRoom>();
    public bool makeIt3d = false;

    //public NavMeshSurface surface;

    public float parentOffsetHeight = 2f;
    GameObject exitGameObject;
    public DungeonAgentFire agent;
    RoomManager roomManager;
    GridManager gridManager;
    public enum DoorGenerationOption
    {
        NoDoor,
        AllDoors,
        RandomDoors
    }
    [Tooltip("Enable Door Generation")]
    public DoorGenerationOption generateDoor = DoorGenerationOption.AllDoors;

    class Dungeon
    {
        public static int map_size;
        public static int map_size_x;
        public static int map_size_y;


        public static List<MapTile> map;

        public static List<Room> rooms = new List<Room>();
        public static List<Corridor> corridors = new List<Corridor>();
        public static Room goalRoom;
        public static Room startRoom;

        public int min_size;
        public int max_size;

        public int maximumRoomCount;
        public int minimumRoomMargin;
        public int roomMargin;
        public int roomMarginTemp;
        public int corridorWidth;

        //tile types for ease
        public static List<int> roomsandfloors = new List<int> { 1, 3 };
        public static List<int> corners = new List<int> { 4, 5, 6, 7 };
        public static List<int> walls = new List<int> { 8, 9, 10, 11 };
        public static List<int> corridor_walls = new List<int> { 108, 109, 1010, 1011 };
        private static List<string> directions = new List<string> { "x", "y", "-y", "-x" }; //,"-y"};
        public static Vector3 startPoint;
        public MapTile createTile(int type, int x, int y, Room room = null)
        {
            MapTile newRoomTile = new MapTile();
            newRoomTile.type = type;
            newRoomTile.room = room;
            newRoomTile.x = x;
            newRoomTile.y = y;
            if (room != null)
            {
                newRoomTile.z = room.room_height;
                newRoomTile.tile_height = room.room_height;
            }
            return newRoomTile;
        }


        public void Generate()
        {
            int roomWidth = this.corridorWidth;
            int room_count = this.maximumRoomCount;
            int min_size = this.min_size;
            int max_size = this.max_size;
            map = new List<MapTile>();
            rooms = new List<Room>();
            corridors = new List<Corridor>();


            int collision_count = 0;
            string direction = "set";
            string oldDirection = "set";
            Room lastRoom;


            for (var i = 0; i < room_count; i++)
            {
                Room room = new Room();
                if (rooms.Count == 0)
                {
                    //first room
                    room.x = (int)Mathf.Floor(map_size / 2f);
                    room.y = (int)Mathf.Floor(map_size / 2f);

                    room.w = Random.Range(min_size, max_size);
                    if (room.w % 2 == 0) room.w += 1;
                    room.h = Random.Range(min_size, max_size);
                    if (room.h % 2 == 0) room.h += 1;

                    room.branch = 0;
                    lastRoom = room;
                }
                else
                {
                    int branch = 0;
                    if (collision_count == 0)
                    {
                        branch = Random.Range(5, 20); //complexity
                    }
                    room.branch = branch;

                    if (rooms.Count > 1)
                    {
                        lastRoom = rooms[rooms.Count - 1];
                    }
                    else
                    {
                        lastRoom = rooms[0];
                    }
                    int lri = 1;

                    while (lastRoom.dead_end)
                    {
                        lastRoom = rooms[rooms.Count - lri++];
                    }


                    if (direction == "set")
                    {
                        string newRandomDirection = directions[Random.Range(0, directions.Count)];
                        direction = newRandomDirection;
                        while (direction == oldDirection)
                        {
                            newRandomDirection = directions[Random.Range(0, directions.Count)];
                            direction = newRandomDirection;
                        }
                    }
                    this.roomMarginTemp = Random.Range(0, this.roomMargin - 1);

                    room.w = Random.Range(min_size, max_size);
                    if (room.w % 2 == 0) room.w += 1;

                    room.h = Random.Range(min_size, max_size);
                    if (room.h % 2 == 0) room.h += 1;



                    if (direction == "y")
                    {
                        room.x = lastRoom.x + lastRoom.w + this.roomMarginTemp + this.minimumRoomMargin + 2;
                        room.y = lastRoom.y;
                    }
                    else if (direction == "-y")
                    {
                        room.x = lastRoom.x - room.w - this.roomMarginTemp - this.minimumRoomMargin - 2;
                        room.y = lastRoom.y;
                    }
                    else if (direction == "x")
                    {
                        room.y = lastRoom.y + lastRoom.h + this.roomMarginTemp + this.minimumRoomMargin + 2;
                        room.x = lastRoom.x;
                    }
                    else if (direction == "-x")
                    {
                        room.y = lastRoom.y - room.h - this.roomMarginTemp - this.minimumRoomMargin - 2;
                        room.x = lastRoom.x;
                    }
                    // room.room_height = roomMarginTemp;
                    room.room_height = 3;





                    //Debug.Log(room.room_id + "- x: " + room.x + " y: " + room.y);

                    room.connectedTo = lastRoom;
                }


                bool doesCollide = this.DoesCollide(room, 0);
                if (doesCollide)
                {
                    i--;
                    collision_count += 1;
                    if (collision_count > 3)
                    {
                        lastRoom.branch = 1;
                        lastRoom.dead_end = true;
                        collision_count = 0;
                    }
                    else
                    {
                        oldDirection = direction;
                        direction = "set";
                    }
                }
                else
                {
                    room.room_id = i;
                    rooms.Add(room);
                    oldDirection = direction;
                    direction = "set";
                }
            }

            //room making
            for (int i = 0; i < rooms.Count; i++)
            {
                Room room = rooms[i];
                for (int x = room.x; x < room.x + room.w; x++)
                {
                    for (int y = room.y; y < room.y + room.h; y++)
                    {
                        MapTile newRoomTile = new MapTile();
                        newRoomTile.type = 1;
                        newRoomTile.room = room;
                        newRoomTile.x = x;
                        newRoomTile.y = y;
                        newRoomTile.z = room.room_height;
                        newRoomTile.tile_height = room.room_height;

                        //mark edges:
                        if (y == room.y + room.h - 1) { newRoomTile.isEdge = true; newRoomTile.edgeLocation = "n"; }
                        if (y == room.y) { newRoomTile.isEdge = true; newRoomTile.edgeLocation = "s"; }
                        if (x == room.x) { newRoomTile.isEdge = true; newRoomTile.edgeLocation = "w"; }
                        if (x == room.x + room.w - 1) { newRoomTile.isEdge = true; newRoomTile.edgeLocation = "e"; }

                        //mark corners:
                        if (x == room.x && y == room.y) { map.Add(this.createTile(4, room.x - 1, room.y - 1, room)); }
                        if (x == room.x && y == room.y + room.h - 1) { map.Add(this.createTile(5, room.x - 1, room.y + room.h, room)); }
                        if (x == room.x + room.w - 1 && y == room.y) { map.Add(this.createTile(7, room.x + room.w, room.y - 1, room)); }
                        if (x == room.x + room.w - 1 && y == room.y + room.h - 1) { map.Add(this.createTile(6, room.x + room.w, room.y + room.h, room)); }

                        map.Add(newRoomTile);
                    }
                }

                /* these 4 loops creates room walls */
                for (int j = 0; j < room.h; j++)
                {
                    map.Add(createTile(11, room.x - 1, room.y + j, room));
                }

                for (int j = 0; j < room.w; j++)
                {
                    map.Add(createTile(10, room.x + j, room.y - 1, room));
                }

                for (int j = 0; j < room.h; j++)
                {
                    map.Add(createTile(9, room.x + room.w, room.y + j, room));
                }

                for (int j = 0; j < room.w; j++)
                {
                    map.Add(createTile(8, room.x + j, room.y + room.h, room));
                }

            }


            //find far far away room
            goalRoom = rooms[rooms.Count - 1];
            //starting point
            startRoom = rooms[0];


            //corridor making
            // List<Corridor> corridors = new List<Corridor>();
            for (int i = 0; i < rooms.Count; i++)
            {
                Room roomA = rooms[i];
                Room roomB = rooms[i].connectedTo;

                if (roomB != null)
                {
                    var pointA = new Room(); //start
                    var pointB = new Room();
                    bool horizontalCorridor = false;
                    bool nextTileBlocksDoor = false;

                    int doorDirection = 0;


                    pointA.x = roomA.x + (int)Mathf.Floor(roomA.w / 2);
                    pointB.x = roomB.x + (int)Mathf.Floor(roomB.w / 2);

                    pointA.y = roomA.y + (int)Mathf.Floor(roomA.h / 2);
                    pointB.y = roomB.y + (int)Mathf.Floor(roomB.h / 2);

                    if (Mathf.Abs(pointA.x - pointB.x) > Mathf.Abs(pointA.y - pointB.y))
                    {
                        //horizontal
                        horizontalCorridor = true;
                        if (roomA.h > roomB.h)
                        {
                            pointA.y = pointB.y;
                        }
                        else
                        {
                            pointB.y = pointA.y;
                        }
                    }
                    else
                    {
                        //vertical
                        if (roomA.w > roomB.w)
                        {
                            pointA.x = pointB.x;
                        }
                        else
                        {
                            pointB.x = pointA.x;
                        }
                    }

                    MapTile currentTile = null;
                    // At the start of your corridor generation
                    // Capture the initial position of pointB as the end of the corridor
                    int initialPointAX = pointA.x;
                    int initialPointAY = pointA.y;
                    int initialPointBX = pointB.x;
                    int initialPointBY = pointB.y;

                    // Adjusting the end points of the corridor to be the nearest edges of the two rooms
                    if (horizontalCorridor)
                    {
                        if (roomA.x + roomA.w < roomB.x)
                        {
                            initialPointAX = roomA.x + roomA.w; // Right edge of roomA
                            initialPointBX = roomB.x; // Left edge of roomB
                        }
                        else if (roomB.x + roomB.w < roomA.x)
                        {
                            initialPointBX = roomB.x + roomB.w; // Right edge of roomB
                            initialPointAX = roomA.x; // Left edge of roomA
                        }
                    }
                    else
                    {
                        if (roomA.y + roomA.h < roomB.y)
                        {
                            initialPointAY = roomA.y + roomA.h; // Bottom edge of roomA
                            initialPointBY = roomB.y; // Top edge of roomB
                        }
                        else if (roomB.y + roomB.h < roomA.y)
                        {
                            initialPointBY = roomB.y + roomB.h; // Bottom edge of roomB
                            initialPointAY = roomA.y; // Top edge of roomA
                        }
                    }

                    Corridor corridor = new Corridor
                    {
                        StartX = initialPointAX,
                        StartY = initialPointAY,
                        Width = corridorWidth,
                        EndX = initialPointBX,
                        EndY = initialPointBY
                    };

                    corridor.CalculateLength();
                    corridors.Add(corridor);
                    int centerOffset;
                    if (corridorWidth % 2 == 0) // Even
                    {
                        centerOffset = corridorWidth / 2 - 1; // one tile to the left (or top) of the "true" center
                    }
                    else // Odd
                    {
                        centerOffset = corridorWidth / 2; // true center
                    }
                    bool doorPlaced = false;
                    while ((pointB.x != pointA.x) || (pointB.y != pointA.y))
                    {
                        if (pointB.x != pointA.x)
                        {
                            if (pointB.x > pointA.x)
                            {
                                pointB.x--;
                                doorDirection = 2;
                            }
                            else
                            {
                                pointB.x++;
                                doorDirection = 4;
                            }
                        }
                        else if (pointB.y != pointA.y)
                        {
                            if (pointB.y > pointA.y)
                            {
                                pointB.y--;
                                doorDirection = 1;
                            }
                            else
                            {
                                pointB.y++;
                                doorDirection = 3;
                            }
                        }

                        //This code checks if corridor hits a wall. Also saves it for later to create door if needed.
                        //That means ModernRoomGenerator will try not to spawn anything blocks corridors and doors.					
                        bool isWall = map.Find(item => (item.x == pointB.x && item.y == pointB.y && item.isWall())) != null;
                        map.RemoveAll(item => (item.x == pointB.x && item.y == pointB.y && item.isWall()));

                        if (isWall && currentTile != null)
                        {
                            currentTile.byCorridor = true; //this is actually previous tile
                        }

                        //dont spawn anything if there is a floor already
                        currentTile = map.Find(item => (pointB.x == item.x && pointB.y == item.y && item.type == 1));
                        if (currentTile != null && nextTileBlocksDoor)
                        {
                            currentTile.byCorridor = true;
                            nextTileBlocksDoor = false;
                            continue;
                        }
                        else if (currentTile != null)
                        {
                            continue;
                        }



                        for (int w = 0; w < corridorWidth; w++)
                        {
                            MapTile newCorridorTile = new MapTile();
                            newCorridorTile.type = 3; // corridor type
                            newCorridorTile.room = roomA;

                            int corridorTileX, corridorTileY;
                            if (horizontalCorridor)
                            {
                                corridorTileX = pointB.x;
                                corridorTileY = pointB.y + w;
                            }
                            else
                            {
                                corridorTileX = pointB.x + w;
                                corridorTileY = pointB.y;
                            }

                            // Remove any tile (wall, corridor, door, etc.) at the target position
                            map.RemoveAll(item => (corridorTileX == item.x && corridorTileY == item.y));

                            newCorridorTile.x = corridorTileX;
                            newCorridorTile.y = corridorTileY;

                            if (isWall && w == centerOffset && !doorPlaced)
                            {
                                newCorridorTile.isDoor = true;
                                newCorridorTile.doorDirection = doorDirection;
                                doorPlaced = true; // Mark that the door has been placed
                            }

                            map.Add(newCorridorTile);
                        }




                        //Corridor wall locations

                        if (horizontalCorridor)
                        {
                            currentTile = map.Find(item => (pointB.x == item.x && pointB.y + corridorWidth == item.y && Dungeon.walls.Contains(item.type)));
                            if (currentTile == null)
                            {
                                map.Add(createTile(108, pointB.x, pointB.y + corridorWidth));
                            }

                            currentTile = map.Find(item => (pointB.x == item.x && pointB.y - 1 == item.y && Dungeon.walls.Contains(item.type)));
                            if (currentTile == null)
                            {
                                map.Add(createTile(1010, pointB.x, pointB.y - 1));
                            }
                        }
                        else
                        {
                            currentTile = map.Find(item => (pointB.x + corridorWidth == item.x && pointB.y == item.y && Dungeon.walls.Contains(item.type)));
                            if (currentTile == null)
                            {
                                map.Add(createTile(109, pointB.x + corridorWidth, pointB.y));
                            }
                            currentTile = map.Find(item => (pointB.x - 1 == item.x && pointB.y == item.y && Dungeon.walls.Contains(item.type)));
                            if (currentTile == null)
                            {
                                map.Add(createTile(1011, pointB.x - 1, pointB.y));
                            }
                        }



                    }

                }
            }

        }

        private bool DoesCollide(Room room, int ignore)
        {
            int random_blankliness = 0;

            for (int i = 0; i < rooms.Count; i++)
            {
                //if (i == ignore) continue;
                var check = rooms[i];
                if (
                    !((room.x + room.w + random_blankliness < check.x) ||
                     (room.x > check.x + check.w + random_blankliness) ||
                     (room.y + room.h + random_blankliness < check.y) ||
                     (room.y > check.y + check.h + random_blankliness)))
                    return true;
            }

            return false;
        }


        private float lineDistance(Room point1, Room point2)
        {
            var xs = 0;
            var ys = 0;

            xs = point2.x - point1.x;
            xs = xs * xs;

            ys = point2.y - point1.y;
            ys = ys * ys;

            return Mathf.Sqrt(xs + ys);
        }



    }

    public void ClearOldDungeon(bool immediate = false)
    {
        int childs = transform.childCount;
        for (var i = childs - 1; i >= 0; i--)
        {
            if (immediate)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            else
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        if (randomizeRoomCount)
        {
            maximumRoomCount = Random.Range(2, maxRandomRooms + 1); // NOT INCLUDING MAX!
            Debug.Log("New Room Count: " + maximumRoomCount);
        }
        agent.m_Configuration = maximumRoomCount;

    }


    public void Generate()
    {

        roomManager = GetComponent<RoomManager>();
        gridManager = GetComponent<GridManager>();

        Dungeon dungeon = new Dungeon();

        dungeon.min_size = minRoomSize;
        dungeon.max_size = maxRoomSize;
        dungeon.maximumRoomCount = maximumRoomCount;
        dungeon.roomMargin = roomMargin;
        dungeon.minimumRoomMargin = minimumRoomMargin;
        dungeon.corridorWidth = corridorWidth;


        dungeon.Generate(); //Calculates all static object locations (walls, corridors, doors, corners etc.)
        roomManager.allRooms = Dungeon.rooms;
        roomManager.parentOffsetHeight = parentOffsetHeight;
        roomManager.allCorridors = Dungeon.corridors;
        roomManager.tileScaling = tileScaling;
        roomManager.GenerateCollidersForAllRooms(tileScaling);
        roomManager.GenerateCollidersForAllCorridors(tileScaling);
        // gridManager.parentOffsetHeight = parentOffsetHeight;

        //after this line, modernroomcreator will instantiate objects in their calculated locations.		
        foreach (MapTile mapTile in Dungeon.map)
        {
            int tile = mapTile.type;
            int tile_height = mapTile.tile_height;

            int orientation = mapTile.orientation;
            GameObject created_tile;
            Vector3 tile_location;

            if (!makeIt3d)
            {
                tile_location = new Vector3(mapTile.x * tileScaling, mapTile.y * tileScaling, 0);
            }
            else
            {
                tile_location = new Vector3(mapTile.x * tileScaling, 0, mapTile.y * tileScaling) + new Vector3(0f, parentOffsetHeight, 0f);
            }

            created_tile = null;
            if (tile == 1)
            {
                GameObject floorPrefabToUse = floorPrefab;
                Room room = mapTile.room;
                // Debug.Log("mapTile room: " + room);
                if (room != null)
                {
                    foreach (CustomRoom customroom in customRooms)
                    {
                        if (customroom.roomId == room.room_id)
                        {
                            floorPrefabToUse = customroom.floorPrefab;
                            break;
                        }
                    }
                }
                created_tile = GameObject.Instantiate(floorPrefabToUse, tile_location, Quaternion.identity) as GameObject;
            }

            ModernRoomGenerator modernroomcreator = this;
            if (Dungeon.walls.Contains(tile) || (Dungeon.corridor_walls.Contains(tile) && !modernroomcreator.corridorWallPrefab))
            {
                GameObject wallPrefabToUse = wallPrefab;
                Room room = mapTile.room;
                if (room != null)
                {
                    foreach (CustomRoom customroom in customRooms)
                    {
                        if (customroom.roomId == room.room_id)
                        {
                            wallPrefabToUse = customroom.wallPrefab;
                            break;
                        }
                    }
                }


                created_tile = GameObject.Instantiate(wallPrefabToUse, tile_location, Quaternion.identity) as GameObject;
                if (!makeIt3d)
                { //wall rotation stuff here.
                    created_tile.transform.Rotate(Vector3.forward * (90 * (tile - 4)));
                }
                else
                {
                    created_tile.transform.Rotate(Vector3.up * (90 * (tile - 4)));
                }
            }
            else if (Dungeon.corridor_walls.Contains(tile))
            {
                GameObject wallPrefabToUse = corridorWallPrefab;
                Room room = mapTile.room;
                if (room != null)
                {
                    foreach (CustomRoom customroom in customRooms)
                    {
                        if (customroom.roomId == room.room_id)
                        {
                            wallPrefabToUse = customroom.wallPrefab;
                            break;
                        }
                    }
                }


                created_tile = GameObject.Instantiate(wallPrefabToUse, tile_location, Quaternion.identity) as GameObject;
                if (!makeIt3d)
                { //wall rotation stuff here.
                    created_tile.transform.Rotate(Vector3.forward * (90 * (tile - 4)));
                }
                else
                {
                    created_tile.transform.Rotate(Vector3.up * (90 * (tile - 4)));
                }
            }

            if (tile == 3)
            {
                if (corridorFloorPrefab)
                {
                    created_tile = GameObject.Instantiate(corridorFloorPrefab, tile_location, Quaternion.identity) as GameObject;
                }
                else
                {
                    created_tile = GameObject.Instantiate(floorPrefab, tile_location, Quaternion.identity) as GameObject;
                }

                if (orientation == 1 && makeIt3d)
                {
                    created_tile.transform.Rotate(Vector3.up * (-90));
                }

            }

            if (Dungeon.corners.Contains(tile))
            {
                GameObject cornerPrefabToUse = cornerPrefab;
                Room room = mapTile.room;
                if (room != null)
                {
                    foreach (CustomRoom customroom in customRooms)
                    {
                        if (customroom.roomId == room.room_id)
                        {
                            cornerPrefabToUse = customroom.cornerPrefab;
                            break;
                        }
                    }
                }


                if (cornerPrefabToUse)
                { //there was a bug in this line. A good man helped for fix.
                    created_tile = GameObject.Instantiate(cornerPrefabToUse, tile_location, Quaternion.identity) as GameObject;
                    if (cornerRotation)
                    {
                        if (!makeIt3d)
                        {
                            created_tile.transform.Rotate(Vector3.forward * (-90 * (tile - 4)));
                        }
                        else
                        {
                            created_tile.transform.Rotate(Vector3.up * (-90 * (tile - 4)));
                        }
                    }
                }
                else
                {
                    created_tile = GameObject.Instantiate(wallPrefab, tile_location, Quaternion.identity) as GameObject;
                }
            }

            if (created_tile)
            {
                created_tile.transform.parent = transform;
            }
        }


        GameObject end_point;
        GameObject start_point;
        if (!makeIt3d)
        {
            end_point = GameObject.Instantiate(exitPrefab, new Vector3(Dungeon.goalRoom.x * tileScaling, Dungeon.goalRoom.y * tileScaling, 0), Quaternion.identity) as GameObject;
            start_point = GameObject.Instantiate(startPrefab, new Vector3(Dungeon.startRoom.x * tileScaling, Dungeon.startRoom.y * tileScaling, 0), Quaternion.identity) as GameObject;

        }
        else
        {
            Vector3 randomFirePosition = roomManager.GetRandomGoalPosition();
            // symbolOGoal.transform.position = randomFirePosition;
            Vector3 endPointPosition = new Vector3((Dungeon.goalRoom.x + Mathf.FloorToInt(Dungeon.goalRoom.w / 2)) * tileScaling, 0.5f + parentOffsetHeight, (Dungeon.goalRoom.y + Mathf.FloorToInt(Dungeon.goalRoom.h / 2)) * tileScaling);
            Vector3 startPointPosition = new Vector3((Dungeon.startRoom.x + Mathf.FloorToInt(Dungeon.startRoom.w / 2)) * tileScaling, 0.5f + parentOffsetHeight, (Dungeon.startRoom.y + Mathf.FloorToInt(Dungeon.startRoom.h / 2)) * tileScaling);
            // end_point = GameObject.Instantiate(exitPrefab, endPointPosition, Quaternion.identity) as GameObject;
            end_point = GameObject.Instantiate(exitPrefab, randomFirePosition, Quaternion.identity) as GameObject;

            start_point = GameObject.Instantiate(startPrefab, startPointPosition, Quaternion.identity) as GameObject;
            roomManager.startPoint = startPointPosition;
            roomManager.endPoint = endPointPosition;
            agent.transform.position = startPointPosition + new Vector3(0f, 0.5f, 0f);

        }


        end_point.transform.parent = transform;
        start_point.transform.parent = transform;
        exitGameObject = end_point;

        agent.symbolOGoal = end_point;
        //Spawn Objects;
        List<SpawnList> spawnedObjectLocations = new List<SpawnList>();

        //looks for suitable locations to spawn stuff. (like monsters, chests..)
        foreach (MapTile mapTile in Dungeon.map)
        {
            if (mapTile.type == 1
                    //do not spawn anything on players start location or finish.
                    && !(mapTile.x == Dungeon.startRoom.x + Mathf.FloorToInt(Dungeon.startRoom.w / 2) && mapTile.y == Dungeon.startRoom.y + Mathf.FloorToInt(Dungeon.startRoom.h / 2))
                    && !(mapTile.x == Dungeon.goalRoom.x + Mathf.FloorToInt(Dungeon.goalRoom.w / 2) && mapTile.y == Dungeon.goalRoom.y + Mathf.FloorToInt(Dungeon.goalRoom.h / 2))
                 )
            {
                // Debug.Log("mapTile.room: " + mapTile.room);

                var location = new SpawnList();
                location.byWall = mapTile.isEdge;
                location.wallLocation = mapTile.edgeLocation;
                location.x = mapTile.x;
                location.y = mapTile.y;


                if (mapTile.byCorridor)
                {
                    location.byCorridor = true;
                }


                location.room = mapTile.room;

                int roomCenterX = (int)Mathf.Floor(location.room.w / 2) + location.room.x;
                int roomCenterY = (int)Mathf.Floor(location.room.h / 2) + location.room.y;

                if (mapTile.x == roomCenterX + 1 && mapTile.y == roomCenterY + 1)
                {
                    location.inTheMiddle = true;
                }
                spawnedObjectLocations.Add(location);
            }
            else if (mapTile.type == 3)
            {

                var location = new SpawnList();
                location.x = mapTile.x;
                location.y = mapTile.y;


                if (mapTile.isDoor)
                {
                    // Debug.Log("mapTile.room: " + mapTile.room);

                    location.byCorridor = true;
                    location.asDoor = mapTile.doorDirection;
                    location.room = mapTile.room;
                    spawnedObjectLocations.Add(location);
                }

            }
        }

        for (int i = 0; i < spawnedObjectLocations.Count; i++)
        {
            SpawnList temp = spawnedObjectLocations[i];
            int randomIndex = Random.Range(i, spawnedObjectLocations.Count);
            spawnedObjectLocations[i] = spawnedObjectLocations[randomIndex];
            spawnedObjectLocations[randomIndex] = temp;
        }

        int objectCountToSpawn = 0;


        //you will need below 2 lines if you are going to use dynamic pathfinding. And a NavMeshSurface component attached to same gameobject.
        //surface = GetComponent<NavMeshSurface>();
        //surface.BuildNavMesh();




        //Now instantiating gameobjects wanted to "spawn"
        foreach (SpawnOption objectToSpawn in spawnOptions)
        {
            objectCountToSpawn = Random.Range(objectToSpawn.minSpawnCount, objectToSpawn.maxSpawnCount);
            if (objectToSpawn.spawnRandomly)
            {
                objectToSpawn.spawnRoom = Random.Range(0, maximumRoomCount);
            }
            while (objectCountToSpawn > 0)
            {
                Debug.Log("objectCountToSpawn" + objectCountToSpawn);
                bool created = false;
                if (!objectToSpawn.spawnRandomly)
                {
                    for (int i = 0; i < spawnedObjectLocations.Count; i++)
                    {
                        bool createHere = false;

                        if (!spawnedObjectLocations[i].spawnedObject && !spawnedObjectLocations[i].byCorridor)
                        {

                            if (objectToSpawn.spawnRoom > maximumRoomCount)
                            {
                                objectToSpawn.spawnRoom = 0;
                            }


                            if (objectToSpawn.spawnRoom == 0)
                            {
                                if (objectToSpawn.spawnByWall)
                                {
                                    if (spawnedObjectLocations[i].byWall)
                                    {
                                        createHere = true;
                                    }
                                }
                                else if (objectToSpawn.spawmInTheMiddle)
                                {
                                    if (spawnedObjectLocations[i].inTheMiddle)
                                    {
                                        createHere = true;
                                    }
                                }
                                else
                                {
                                    createHere = true;
                                }
                            }
                            else
                            {
                                if (spawnedObjectLocations[i].room.room_id == objectToSpawn.spawnRoom)
                                {
                                    if (objectToSpawn.spawnByWall)
                                    {
                                        if (spawnedObjectLocations[i].byWall)
                                        {
                                            createHere = true;
                                        }
                                    }
                                    else
                                    {
                                        createHere = true;
                                    }
                                }
                            }
                        }


                        if (createHere)
                        { //means modernroomcreator found a suitable place to put object.
                            SpawnList spawnLocation = spawnedObjectLocations[i];
                            GameObject newObject;
                            Quaternion spawnRotation = Quaternion.identity;

                            if (!makeIt3d)
                            {
                                newObject = GameObject.Instantiate(objectToSpawn.gameObject, new Vector3(spawnLocation.x * tileScaling, spawnLocation.y * tileScaling, 0), spawnRotation) as GameObject;
                            }
                            else
                            {
                                if (spawnLocation.byWall)
                                {
                                    if (spawnLocation.wallLocation == "s")
                                    {
                                        spawnRotation = Quaternion.Euler(new Vector3(0, 270, 0));
                                    }
                                    else if (spawnLocation.wallLocation == "n")
                                    {
                                        spawnRotation = Quaternion.Euler(new Vector3(0, 90, 0));
                                    }
                                    else if (spawnLocation.wallLocation == "e")
                                    {
                                        spawnRotation = Quaternion.Euler(new Vector3(0, 180, 0));
                                    }
                                    else if (spawnLocation.wallLocation == "w")
                                    {
                                        spawnRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                                    }
                                    Debug.Log("wallLocation" + spawnLocation.wallLocation);
                                }
                                else
                                {
                                    if (objectToSpawn.spawnRotated)
                                    {
                                        spawnRotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
                                    }
                                    else
                                    {
                                        spawnRotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 2) * 90, 0));
                                    }
                                }
                                float margin = 0f;

                                newObject = GameObject.Instantiate(objectToSpawn.gameObject, new Vector3(spawnLocation.x * tileScaling + margin, 0 + objectToSpawn.heightFix, spawnLocation.y * tileScaling + margin), spawnRotation) as GameObject;
                            }
                            newObject.transform.parent = transform;
                            spawnedObjectLocations[i].spawnedObject = newObject;
                            objectCountToSpawn--;
                            created = true;
                            break;
                        }
                    }
                }
                else
                {
                    Vector3 randomPosition = roomManager.GetRandomObjectPosition();
                    GameObject newObject = GameObject.Instantiate(objectToSpawn.gameObject, randomPosition, Quaternion.identity) as GameObject;
                    newObject.transform.parent = transform;
                    created = true;
                    objectCountToSpawn--;
                }
                if (!created) //if cant find anywhere to put, dont put. (prevents endless loops)
                {
                    objectCountToSpawn--;
                }

            }
        }

        // DOORS

        if (doorPrefab && generateDoor != DoorGenerationOption.NoDoor)
        {
            for (int i = 0; i < spawnedObjectLocations.Count; i++)
            {
                if (spawnedObjectLocations[i].asDoor > 0)
                {
                    // Handle Door Generation Option:
                    if (generateDoor == DoorGenerationOption.RandomDoors && Random.Range(0f, 1f) > 0.5f)
                    {
                        continue; // Skip to next iteration.
                    }
                    GameObject newObject;
                    SpawnList spawnLocation = spawnedObjectLocations[i];

                    // Debug.Log("Door location: " + spawnLocation.x + ", " + spawnLocation.y + ", direction: " + spawnLocation.asDoor);
                    GameObject doorPrefabToUse = doorPrefab;
                    Room room = spawnLocation.room;
                    // Debug.Log("room: " + room);
                    // Debug.Log("spawnLocation.room:" + (spawnLocation.room == null));
                    if (room != null)
                    {
                        foreach (CustomRoom customroom in customRooms)
                        {
                            // Debug.Log("customRoomID: " + customroom.roomId + " RoomID: " + room.room_id);

                            if (customroom.roomId == room.room_id)
                            {
                                // Debug.Log("using " + customroom.doorPrefab);
                                doorPrefabToUse = customroom.doorPrefab;
                                break;
                            }
                        }
                    }

                    float offsetX = 0f;
                    float offsetZ = 0f;
                    float halfDoorWidth = doorPrefabToUse.transform.localScale.x / 2f - 0.4f; // assuming your door's scale in the prefab is set correctly



                    switch (spawnLocation.asDoor)
                    {
                        case 1:
                            offsetX = -halfDoorWidth;
                            break;
                        case 2:
                            offsetZ = -halfDoorWidth;
                            break;
                        case 3:
                            offsetX = halfDoorWidth;
                            break;
                        case 4:
                            offsetZ = halfDoorWidth;
                            break;
                    }

                    if (!makeIt3d)
                    {
                        newObject = GameObject.Instantiate(doorPrefabToUse, new Vector3((spawnLocation.x + offsetX) * tileScaling, spawnLocation.y * tileScaling, 0), Quaternion.identity) as GameObject;
                    }
                    else
                    {
                        newObject = GameObject.Instantiate(doorPrefabToUse, new Vector3((spawnLocation.x * tileScaling) + offsetX, 0 + parentOffsetHeight, (spawnLocation.y * tileScaling) + offsetZ), Quaternion.identity) as GameObject;
                    }

                    DoorController doorController = newObject.GetComponent<DoorController>();
                    doorController.doorOrientation = spawnLocation.asDoor;

                    if (!makeIt3d)
                    {
                        newObject.transform.Rotate(Vector3.forward * (-90 * (spawnedObjectLocations[i].asDoor - 1)));
                    }
                    else
                    {
                        newObject.transform.Rotate(Vector3.up * (-90 * (spawnedObjectLocations[i].asDoor - 1)));
                    }

                    newObject.transform.parent = transform;
                    spawnedObjectLocations[i].spawnedObject = newObject;
                }
            }

        }
        gridManager.ResetGrid();



    }
    // Use this for initialization
    void Start()
    {

        if (generate_on_load)
        {
            ClearOldDungeon();
            Generate();

        }
    }
}
