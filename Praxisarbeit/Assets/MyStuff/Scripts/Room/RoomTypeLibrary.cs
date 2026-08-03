using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomTypeLibrary", menuName = "DungeonGenerator/Room Type Library")]
public class RoomTypeLibrary : ScriptableObject 
{
    [System.Serializable]
    private struct Entry 
    {
        public RoomType type;
        public RoomData data;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();
    private Dictionary<RoomType, RoomData> _lookup;

    private void BuildLookupIfNeeded() 
    {
        _lookup = new Dictionary<RoomType, RoomData>();

        foreach (Entry entry in entries) 
        {
            if (entry.data == null) continue;
            _lookup[entry.type] = entry.data;
        }
    }

    public RoomData GetRoomData(RoomType type) 
    {
        BuildLookupIfNeeded();

        if (_lookup.TryGetValue(type, out RoomData data)) 
        {
            return data;
        }
        return null;
    }

    private void OnValidate() 
    {
        _lookup = null;

        //if (Application.isPlaying)
            //return;

        DungeonSpawner.UpdateDungeonFull();
    }
}