using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

using static RoomData;

public static class JsonIO
{
    public static string filePath = Path.Combine(Application.dataPath, "MyStuff", "Saves", "RoomConfig.json");
    public static string layoutFilePath = Path.Combine(Application.dataPath, "MyStuff", "Saves", "DungeonLayout.json");

    public static void ExportAll(RoomTypeLibrary library, DungeonData dungeonData)
    {
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        ExportRoomConfig(library);
        ExportGeneratedLayout(dungeonData);

        Debug.Log("[DungeonEditor] Export succsessfully completed!");
    }

    public static DungeonData ImportAll(RoomTypeLibrary library)
    {
        ImportRoomConfig(library);
        DungeonData loadedLayout = ImportGeneratedLayout();

        Debug.Log("[DungeonEditor] Import succsessfully completed!");
        return loadedLayout;
    }

    private static void ExportRoomConfig(RoomTypeLibrary library)
    {
        if (library == null) return;

        DungeonConfigContainer container = new DungeonConfigContainer { rooms = new List<SerializableRoomData>() };
        foreach (RoomType type in System.Enum.GetValues(typeof(RoomType)))
        {
            RoomData data = library.GetRoomData(type);
            if (data == null) continue;

            SerializableRoomData sData = new SerializableRoomData
            {
                roomType = type.ToString(),
                roomName = data.roomName,
                floorPrefabPath = AssetDatabase.GetAssetPath(data.floorPrefab),
                wallPrefabPath = AssetDatabase.GetAssetPath(data.wallPrefab),
                ceilingPrefabPath = AssetDatabase.GetAssetPath(data.ceilingPrefab),
                doorPrefabPath = AssetDatabase.GetAssetPath(data.doorPrefab),
                spawnAmount = data.spawnAmount,
                spawnChance = data.spawnChance,
                wallHeight = data.wallHeight,
                possibleContents = new List<SpawnableObjectJsonData>()
            };

            if (data.possibleContents != null)
            {
                foreach (var content in data.possibleContents)
                {
                    if (content.prefab == null) continue;
                    sData.possibleContents.Add(new SpawnableObjectJsonData
                    {
                        prefabPath = AssetDatabase.GetAssetPath(content.prefab),
                        minAmount = content.minAmount,
                        maxAmount = content.maxAmount
                    });
                }
            }

            container.rooms.Add(sData);
        }
        File.WriteAllText(filePath, JsonUtility.ToJson(container, true));
        Debug.Log($"[DungeonEditor] Rooms exported to {filePath}");
    }

    private static void ImportRoomConfig(RoomTypeLibrary library)
    {
        if (library == null || !File.Exists(filePath)) return;

        string json = File.ReadAllText(filePath);
        DungeonConfigContainer container = JsonUtility.FromJson<DungeonConfigContainer>(json);

        foreach (var sData in container.rooms)
        {
            if (!System.Enum.TryParse(sData.roomType, out RoomType type)) continue;

            RoomData data = library.GetRoomData(type);
            if (data == null) continue;
            data.roomName = sData.roomName;
            data.spawnChance = sData.spawnChance;
            data.spawnAmount = sData.spawnAmount;
            data.wallHeight = sData.wallHeight;

            if (!string.IsNullOrEmpty(sData.floorPrefabPath)) data.floorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sData.floorPrefabPath);
            if (!string.IsNullOrEmpty(sData.wallPrefabPath)) data.wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sData.wallPrefabPath);
            if (!string.IsNullOrEmpty(sData.ceilingPrefabPath)) data.ceilingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sData.ceilingPrefabPath);
            if (!string.IsNullOrEmpty(sData.doorPrefabPath)) data.doorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sData.doorPrefabPath);

            if (data.possibleContents == null) data.possibleContents = new List<SpawnableObjectData>();
            data.possibleContents.Clear();

            if (sData.possibleContents != null)
            {
                foreach (var jsonContent in sData.possibleContents)
                {
                    if (string.IsNullOrEmpty(jsonContent.prefabPath)) continue;
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(jsonContent.prefabPath);
                    if (prefab != null)
                    {
                        data.possibleContents.Add(new SpawnableObjectData
                        {
                            prefab = prefab,
                            minAmount = jsonContent.minAmount,
                            maxAmount = jsonContent.maxAmount
                        });
                    }
                }
            }

            EditorUtility.SetDirty(data);
        }
        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[DungeonEditor] Rooms Imported");
    }

    private static void ExportGeneratedLayout(DungeonData dungeonData)
    {
        if (dungeonData == null) return;

        SavedDungeonLayout layout = new SavedDungeonLayout();
        HashSet<Vector2Int> remainingCorridors = new HashSet<Vector2Int>(dungeonData.CorridorTiles);
        List<SavedRoomInstance> tempRooms = new List<SavedRoomInstance>();

        foreach (RoomInstance room in dungeonData.Rooms)
        {
            List<Vector2Int> tiles = room.GetTiles();
            if (tiles == null || tiles.Count == 0) continue;

            Vector2Int min = tiles[0];
            Vector2Int max = tiles[0];
            foreach (var tile in tiles)
            {
                if (tile.x < min.x) min.x = tile.x;
                if (tile.y < min.y) min.y = tile.y;
                if (tile.x > max.x) max.x = tile.x;
                if (tile.y > max.y) max.y = tile.y;
            }

            SavedRoomInstance sRoom = new SavedRoomInstance
            {
                roomType = room.Type.ToString(),
                minCorner = min,
                maxCorner = max,
                connectedCorridors = new List<SavedCorridorSegment>()
            };
            tempRooms.Add(sRoom);
        }

        List<SavedCorridorSegment> allSegments = new List<SavedCorridorSegment>();

        foreach (var tile in dungeonData.CorridorTiles)
        {
            if (!remainingCorridors.Contains(tile)) continue;

            int hLength = 1;
            while (remainingCorridors.Contains(new Vector2Int(tile.x + hLength, tile.y))) hLength++;

            int vLength = 1;
            while (remainingCorridors.Contains(new Vector2Int(tile.x, tile.y + vLength))) vLength++;

            if (hLength >= vLength && hLength > 1)
            {
                allSegments.Add(new SavedCorridorSegment { start = tile, isHorizontal = true, length = hLength });
                for (int i = 0; i < hLength; i++) remainingCorridors.Remove(new Vector2Int(tile.x + i, tile.y));
            }
            else if (vLength > 1)
            {
                allSegments.Add(new SavedCorridorSegment { start = tile, isHorizontal = false, length = vLength });
                for (int i = 0; i < vLength; i++) remainingCorridors.Remove(new Vector2Int(tile.x, tile.y + i));
            }
            else
            {
                allSegments.Add(new SavedCorridorSegment { start = tile, isHorizontal = true, length = 1 });
                remainingCorridors.Remove(tile);
            }
        }

        foreach (var segment in allSegments)
        {
            SavedRoomInstance bestRoom = null;
            float minDistance = float.MaxValue;

            foreach (var sRoom in tempRooms)
            {
                Vector2 roomCenter = new Vector2((sRoom.minCorner.x + sRoom.maxCorner.x) / 2f, (sRoom.minCorner.y + sRoom.maxCorner.y) / 2f);
                float dist = Vector2.Distance(segment.start, roomCenter);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestRoom = sRoom;
                }
            }

            if (bestRoom != null)
            {
                bestRoom.connectedCorridors.Add(segment);
            }
        }

        layout.rooms = tempRooms;

        File.WriteAllText(layoutFilePath, JsonUtility.ToJson(layout, true));
        Debug.Log($"[DungeonEditor] Layout exported to {layoutFilePath}");
    }

    private static DungeonData ImportGeneratedLayout()
    {
        if (!File.Exists(layoutFilePath))
        {
            Debug.LogWarning("[DungeonEditor] Couldnt find layout data!");
            return null;
        }

        string json = File.ReadAllText(layoutFilePath);
        SavedDungeonLayout savedLayout = JsonUtility.FromJson<SavedDungeonLayout>(json);

        List<RoomInstance> loadedRooms = new List<RoomInstance>();
        HashSet<Vector2Int> corridorTilesSet = new HashSet<Vector2Int>();
        var loadedConnections = new List<(RoomInstance a, RoomInstance b)>();

        int currentId = 0;

        foreach (var sRoom in savedLayout.rooms)
        {
            if (!System.Enum.TryParse(sRoom.roomType, out RoomType type)) continue;

            List<Vector2Int> reconstructedTiles = new List<Vector2Int>();
            for (int x = sRoom.minCorner.x; x <= sRoom.maxCorner.x; x++)
            {
                for (int y = sRoom.minCorner.y; y <= sRoom.maxCorner.y; y++)
                {
                    reconstructedTiles.Add(new Vector2Int(x, y));
                }
            }

            loadedRooms.Add(new RoomInstance(currentId, type, reconstructedTiles));
            currentId++;

            if (sRoom.connectedCorridors != null)
            {
                foreach (var segment in sRoom.connectedCorridors)
                {
                    for (int i = 0; i < segment.length; i++)
                    {
                        if (segment.isHorizontal)
                            corridorTilesSet.Add(new Vector2Int(segment.start.x + i, segment.start.y));
                        else
                            corridorTilesSet.Add(new Vector2Int(segment.start.x, segment.start.y + i));
                    }
                }
            }
        }

        List<Vector2Int> loadedCorridors = new List<Vector2Int>(corridorTilesSet);
        return new DungeonData(loadedRooms, loadedCorridors, loadedConnections);
    }

    [System.Serializable]
    public class SavedDungeonLayout
    {
        public List<SavedRoomInstance> rooms = new List<SavedRoomInstance>();
    }

    [System.Serializable]
    public class SavedRoomInstance
    {
        public string roomType;
        public Vector2Int minCorner;
        public Vector2Int maxCorner;
        public List<SavedCorridorSegment> connectedCorridors = new List<SavedCorridorSegment>();
    }

    [System.Serializable]
    public class SavedCorridorSegment
    {
        public Vector2Int start;
        public bool isHorizontal;
        public int length;
    }
}