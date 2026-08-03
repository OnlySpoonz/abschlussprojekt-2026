using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

[Serializable]
public class SerializableRoomData 
{
    public string roomType;
    public string roomName;
    public string floorPrefabPath;
    public string wallPrefabPath;
    public string ceilingPrefabPath;
    public string doorPrefabPath;
    public float spawnChance;
    public int spawnAmount;
    public int wallHeight;
    public List<SpawnableObjectJsonData> possibleContents = new List<SpawnableObjectJsonData>();
}

[Serializable]
public class DungeonConfigContainer 
{
    public List<SerializableRoomData> rooms;
}
[Serializable]
public class SpawnableObjectJsonData 
{
    public string prefabPath; 
    public int minAmount;
    public int maxAmount;
}