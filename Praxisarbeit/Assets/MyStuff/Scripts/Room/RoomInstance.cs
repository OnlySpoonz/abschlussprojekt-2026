using UnityEngine;
using System;
using System.Collections.Generic;

public class RoomInstance {
    public int Id { get; private set; } 
    public RoomShape Shape { get; }
    private List<Vector2Int> tiles;
    public RoomType Type { get; private set; } = RoomType.Standard;
    public Vector2Int Center => Shape != null ? Shape.GetCenter() : Vector2Int.zero;

    public int WallHeight { get; set; } = 4;

    public event Action<RoomType, RoomType> OnTypeChanged;

    private List<GameObject> spawnedContent = new List<GameObject>();

    public RoomInstance(int id, RoomShape shape) 
    {
        Id = id;
        Shape = shape;
        this.tiles = shape.GetTiles();
    }


    public RoomInstance(int id, RoomType type, List<Vector2Int> savedTiles) 
    {
        Id = id;
        Type = type;
        this.tiles = new List<Vector2Int>(savedTiles);
        Shape = null; 
    }

    public List<Vector2Int> GetTiles() => tiles;

    public void SetTiles(List<Vector2Int> newTiles) 
    {
        this.tiles = new List<Vector2Int>(newTiles);
    }

    public void ForceType(RoomType newType)
    {
        Type = newType;
    }

    public void SetType(RoomType newType) 
    {
        if (Type == newType)
            return;

        RoomType oldType = Type;
        Type = newType;
        OnTypeChanged?.Invoke(oldType, newType);
    }

    public void RegisterSpawnedObject(GameObject obj) 
    {
        if (obj != null && !spawnedContent.Contains(obj))
            spawnedContent.Add(obj);
    }

    public List<GameObject> GetSpawnedContent() 
    {
        return new List<GameObject>(spawnedContent);
    }

    public void ClearSpawnedContent() 
    {
        spawnedContent.Clear();
    }
}