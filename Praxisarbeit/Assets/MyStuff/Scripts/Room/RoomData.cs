using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomData", menuName = "DungeonGenerator/Room Data")]
public class RoomData : ScriptableObject
{
    [Header("General")]
    public string roomName = "New Room";

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject ceilingPrefab;
    public GameObject doorPrefab;

    public List<SpawnableObjectData> possibleContents = new List<SpawnableObjectData>();

    [System.Serializable]
    public struct SpawnableObjectData
    {
        public GameObject prefab;
        [Range(0, 10)] public int minAmount;
        [Range(1, 20)] public int maxAmount;
    }

    [Header("Spawn Behaviour")]
    [Range(0f, 1f)]
    public float spawnChance = 1f;
    [Min(1)]
    public int spawnAmount = 1;
    [Header("Room Height")]
    [Min(3)]
    public int wallHeight = 4;

     private void OnValidate() 
     {
        //if (Application.isPlaying) return;

        //DungeonSpawner.UpdateDungeonForAsset(this);
     }
}