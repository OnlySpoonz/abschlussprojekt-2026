using System.Collections.Generic;
using UnityEngine;
using static RoomData;

public static class DungeonSpawner
{
    private struct RoomSpawnInfo
    {
        public RoomInstance room;
        public List<GameObject> floorObjects;
        public List<GameObject> wallObjects;
        public List<GameObject> contentObjects;
        public List<Vector2Int> trackedTiles;
    }

    private static Dictionary<RoomInstance, RoomSpawnInfo> RoomRegistry = new();
    private static HashSet<Vector2Int> GlobalWalkableTiles = new();
    private static Vector2Int[] Directions = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };

    private static DungeonData currentDungeonData;
    private static RoomTypeLibrary currentLibrary;
    private static Transform currentContainer;
    private static RoomData currentFallbackRoomData;

    public static void Spawn(DungeonData dungeonData, RoomTypeLibrary library, Transform container)
    {
        ClearDungeon(container);

        currentDungeonData = dungeonData;
        currentLibrary = library;
        currentContainer = container;
        currentFallbackRoomData = library.GetRoomData(RoomType.Standard);

        var roomDataLookup = new Dictionary<RoomInstance, (RoomData data, bool isFallback)>();

        foreach (RoomInstance room in dungeonData.Rooms)
        {
            RoomData roomData = library.GetRoomData(room.Type);
            bool isFallback = roomData == null;
            if (isFallback)
            {
                roomData = currentFallbackRoomData;
            }

            if (roomData == null) continue;

            room.WallHeight = roomData.wallHeight;
            roomDataLookup[room] = (roomData, isFallback);

            RoomSpawnInfo info = new RoomSpawnInfo
            {
                room = room,
                floorObjects = new List<GameObject>(),
                wallObjects = new List<GameObject>(),
                contentObjects = new List<GameObject>(),
                trackedTiles = new List<Vector2Int>()
            };

            SpawnRoomFloor(room, roomData, container, GlobalWalkableTiles, info);
            RoomRegistry[room] = info;

            room.OnTypeChanged -= HandleRoomTypeChanged;
            room.OnTypeChanged += HandleRoomTypeChanged;
        }

        SpawnCorridorFloor(dungeonData, currentFallbackRoomData, container, GlobalWalkableTiles);

        foreach (RoomInstance room in dungeonData.Rooms)
        {
            if (!roomDataLookup.TryGetValue(room, out var entry) || entry.isFallback)
                continue;

            RoomSpawnInfo info = RoomRegistry[room];
            SpawnRoomWalls(room, entry.data, container, GlobalWalkableTiles, info);
            RoomRegistry[room] = info;
        }

        SpawnCorridorWalls(dungeonData, currentFallbackRoomData, container, GlobalWalkableTiles);

        foreach (RoomInstance room in dungeonData.Rooms)
        {
            if (RoomRegistry.TryGetValue(room, out RoomSpawnInfo info))
            {
                RoomData roomData = library.GetRoomData(room.Type) ?? currentFallbackRoomData;
                if (roomData != null)
                {
                    SpawnRoomContent(room, roomData, info);
                }
            }
        }
    }

    private static void HandleRoomTypeChanged(RoomType oldType, RoomType newType)
    {
        if (currentDungeonData == null) return;

        foreach (RoomInstance room in currentDungeonData.Rooms)
        {
            if (room.Type == newType)
            {
                RespawnRoom(room);
                break;
            }
        }
    }

    public static void RespawnRoom(RoomInstance room)
    {
        if (!RoomRegistry.TryGetValue(room, out RoomSpawnInfo info)) return;

        ClearRoomObjects(info);

        foreach (Vector2Int tile in info.trackedTiles)
        {
            GlobalWalkableTiles.Remove(tile);
        }
        info.trackedTiles.Clear();

        room.ClearSpawnedContent();

        RoomData newRoomData = currentLibrary.GetRoomData(room.Type) ?? currentFallbackRoomData;
        if (newRoomData == null) return;

        room.WallHeight = newRoomData.wallHeight;

        SpawnRoomFloor(room, newRoomData, currentContainer, GlobalWalkableTiles, info);
        SpawnRoomWalls(room, newRoomData, currentContainer, GlobalWalkableTiles, info);
        SpawnRoomContent(room, newRoomData, info);

        RoomRegistry[room] = info;
    }

    private static void ClearRoomObjects(RoomSpawnInfo info)
    {
        DestroyObjectList(info.floorObjects);
        DestroyObjectList(info.wallObjects);
        DestroyObjectList(info.contentObjects);
    }

    private static void DestroyObjectList(List<GameObject> list)
    {
        foreach (GameObject obj in list)
        {
            if (obj == null) continue;
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }
        list.Clear();
    }

    public static void UpdateDungeonForAsset(RoomData changedAsset)
    {
        if (currentDungeonData == null || currentLibrary == null || currentContainer == null)
            return;

        foreach (RoomInstance room in currentDungeonData.Rooms)
        {
            if (currentLibrary.GetRoomData(room.Type) == changedAsset)
            {
                RespawnRoom(room);
            }
        }
    }

    public static void UpdateDungeonFull()
    {
        if (currentDungeonData == null || currentLibrary == null || currentContainer == null)
            return;

        RoomClassifier.Classify(currentDungeonData.Rooms, currentDungeonData.Connections, currentLibrary);
        foreach (RoomInstance room in currentDungeonData.Rooms)
        {
            RespawnRoom(room);
        }
    }

    private static void SpawnRoomFloor(RoomInstance room, RoomData roomData, Transform container, HashSet<Vector2Int> walkableTiles, RoomSpawnInfo info)
    {
        List<Vector2Int> roomTiles = room.GetTiles();

        foreach (Vector2Int tile in roomTiles)
        {
            if (!walkableTiles.Add(tile)) continue;

            info.trackedTiles.Add(tile);

            Vector3 floorPos = new Vector3(tile.x, 0f, tile.y);
            GameObject floorObj = Object.Instantiate(roomData.floorPrefab, floorPos, Quaternion.identity, container);
            info.floorObjects.Add(floorObj);
            room.RegisterSpawnedObject(floorObj);

            Vector3 ceilingPos = new Vector3(tile.x, room.WallHeight + 0.1f, tile.y);
            GameObject ceilingPrefab = roomData.ceilingPrefab != null ? roomData.ceilingPrefab : roomData.floorPrefab;
            GameObject ceilingObj = Object.Instantiate(ceilingPrefab, ceilingPos, Quaternion.identity, container);
            info.floorObjects.Add(ceilingObj);
            room.RegisterSpawnedObject(ceilingObj);
        }
    }

    private static void SpawnRoomWalls(RoomInstance room, RoomData roomData, Transform container, HashSet<Vector2Int> walkableTiles, RoomSpawnInfo info)
    {
        List<Vector2Int> roomTiles = room.GetTiles();
        HashSet<Vector2Int> roomTileSet = new HashSet<Vector2Int>(roomTiles);

        foreach (Vector2Int tile in roomTiles)
        {
            foreach (Vector2Int dir in Directions)
            {
                Vector2Int neighbor = tile + dir;

                if (!roomTileSet.Contains(neighbor))
                {
                    if (!walkableTiles.Contains(neighbor))
                    {
                        for (int y = 0; y < room.WallHeight; y++)
                        {
                            SpawnWallAtPosition(tile, dir, roomData.wallPrefab, container, info, y);
                        }
                    }
                    else if (currentDungeonData != null && currentDungeonData.CorridorTiles.Contains(neighbor))
                    {
                        GameObject doorPrefab = roomData.doorPrefab != null ? roomData.doorPrefab : currentFallbackRoomData.doorPrefab;
                        SpawnDoorAtPosition(tile, dir, doorPrefab, container, info);

                        for (int y = 3; y < room.WallHeight; y++)
                        {
                            SpawnWallAtPosition(tile, dir, roomData.wallPrefab, container, info, y);
                        }
                    }
                }
            }
        }
    }

    private static void SpawnWallAtPosition(Vector2Int tile, Vector2Int direction, GameObject wallPrefab, Transform container, RoomSpawnInfo info, int yOffset)
    {
        Vector3 wallPos;
        Quaternion rotation;

        if (direction.x != 0)
        {
            float offsetX = direction.x > 0 ? tile.x + 0.5f : tile.x - 0.5f;
            wallPos = new Vector3(offsetX, 0.55f + yOffset, tile.y);
            rotation = Quaternion.identity;
        }
        else
        {
            float offsetY = direction.y > 0 ? tile.y + 0.5f : tile.y - 0.5f;
            wallPos = new Vector3(tile.x, 0.55f + yOffset, offsetY);
            rotation = Quaternion.Euler(0, 90, 0);
        }

        GameObject wallObj = Object.Instantiate(wallPrefab, wallPos, rotation, container);
        info.wallObjects.Add(wallObj);
    }

    private static void SpawnDoorAtPosition(Vector2Int tile, Vector2Int direction, GameObject doorPrefab, Transform container, RoomSpawnInfo info)
    {
        if (doorPrefab == null) return;

        Vector3 doorPos;
        Quaternion rotation;

        if (direction.x != 0)
        {
            float offsetX = direction.x > 0 ? tile.x + 0.5f : tile.x - 0.5f;
            doorPos = new Vector3(offsetX, 0f, tile.y);
            rotation = Quaternion.identity;
        }
        else
        {
            float offsetY = direction.y > 0 ? tile.y + 0.5f : tile.y - 0.5f;
            doorPos = new Vector3(tile.x, 0f, offsetY);
            rotation = Quaternion.Euler(0, 90, 0);
        }

        GameObject doorObj = Object.Instantiate(doorPrefab, doorPos, rotation, container);
        info.wallObjects.Add(doorObj);
        info.room.RegisterSpawnedObject(doorObj);
    }

    private static void SpawnCorridorFloor(DungeonData dungeonData, RoomData corridorRoomData, Transform container, HashSet<Vector2Int> walkableTiles)
    {
        if (corridorRoomData == null || corridorRoomData.floorPrefab == null) return;

        foreach (Vector2Int tile in dungeonData.CorridorTiles)
        {
            if (!walkableTiles.Add(tile)) continue;

            Vector3 floorPos = new Vector3(tile.x, 0f, tile.y);
            Object.Instantiate(corridorRoomData.floorPrefab, floorPos, Quaternion.identity, container);

            Vector3 ceilingPos = new Vector3(tile.x, 3.1f, tile.y);
            Object.Instantiate(corridorRoomData.floorPrefab, ceilingPos, Quaternion.identity, container);
        }
    }

    private static void SpawnCorridorWalls(DungeonData dungeonData, RoomData corridorRoomData, Transform container, HashSet<Vector2Int> walkableTiles)
    {
        if (corridorRoomData == null || corridorRoomData.wallPrefab == null) return;

        HashSet<(Vector2Int tile, Vector2Int direction)> spawnedWalls = new();

        foreach (Vector2Int tile in dungeonData.CorridorTiles)
        {
            foreach (Vector2Int direction in Directions)
            {
                Vector2Int neighbor = tile + direction;
                if (walkableTiles.Contains(neighbor)) continue;

                if (!spawnedWalls.Add((tile, direction))) continue;

                for (int y = 0; y < 3; y++)
                {
                    SpawnCorridorWallSegment(tile, direction, corridorRoomData.wallPrefab, container, y);
                }
            }
        }
    }

    private static void SpawnCorridorWallSegment(Vector2Int tile, Vector2Int direction, GameObject wallPrefab, Transform container, int yOffset)
    {
        if (direction.x != 0)
        {
            float offsetX = direction.x > 0 ? tile.x + 0.5f : tile.x - 0.5f;
            Vector3 wallPos = new Vector3(offsetX, 0.55f + yOffset, tile.y);
            Object.Instantiate(wallPrefab, wallPos, Quaternion.identity, container);
        }
        else
        {
            float offsetY = direction.y > 0 ? tile.y + 0.5f : tile.y - 0.5f;
            Vector3 wallPos = new Vector3(tile.x, 0.55f + yOffset, offsetY);
            Object.Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0), container);
        }
    }

    private static void SpawnRoomContent(RoomInstance room, RoomData roomData, RoomSpawnInfo info)
    {
        if (roomData == null || roomData.possibleContents == null || roomData.possibleContents.Count == 0) return;

        List<Vector2Int> roomTiles = room.GetTiles();
        if (roomTiles == null || roomTiles.Count == 0) return;

        List<Vector2Int> freeTiles = new List<Vector2Int>();
        HashSet<Vector2Int> corridorSet = new HashSet<Vector2Int>(currentDungeonData.CorridorTiles);

        foreach (Vector2Int tile in roomTiles)
        {
            bool isNearDoor = false;
            foreach (Vector2Int dir in Directions)
            {
                if (corridorSet.Contains(tile + dir))
                {
                    isNearDoor = true;
                    break;
                }
            }

            if (!isNearDoor)
            {
                freeTiles.Add(tile);
            }
        }

        if (freeTiles.Count == 0) freeTiles.AddRange(roomTiles);

        ShuffleList(freeTiles);

        foreach (SpawnableObjectData spawnRule in roomData.possibleContents)
        {
            if (spawnRule.prefab == null) continue;

            int targetAmount = Random.Range(spawnRule.minAmount, spawnRule.maxAmount + 1);

            for (int i = 0; i < targetAmount; i++)
            {
                if (freeTiles.Count == 0) break;

                Vector2Int spawnTile = freeTiles[freeTiles.Count - 1];
                freeTiles.RemoveAt(freeTiles.Count - 1);

                Vector3 spawnPos = new Vector3(spawnTile.x, 0f, spawnTile.y);
                GameObject spawnedObj = Object.Instantiate(spawnRule.prefab, spawnPos, Quaternion.identity, currentContainer);

                info.contentObjects.Add(spawnedObj);
                room.RegisterSpawnedObject(spawnedObj);
            }
        }
    }

    private static void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public static void ClearDungeon(Transform container)
    {
        if (currentDungeonData != null)
        {
            foreach (RoomInstance room in currentDungeonData.Rooms)
            {
                room.OnTypeChanged -= HandleRoomTypeChanged;
            }
        }

        RoomRegistry.Clear();
        GlobalWalkableTiles.Clear();
        currentDungeonData = null;
        currentLibrary = null;
        currentContainer = null;

        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Object.Destroy(container.GetChild(i).gameObject);
            else
                Object.DestroyImmediate(container.GetChild(i).gameObject);
        }
    }
}