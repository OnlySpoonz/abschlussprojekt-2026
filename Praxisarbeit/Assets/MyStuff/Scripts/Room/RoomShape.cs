using UnityEngine;
using System.Collections.Generic;

public interface RoomShape 
{
    List<Vector2Int> GetTiles();
    RectInt GetBounds();
    Vector2Int GetCenter();
}