using UnityEngine;
using System.Collections.Generic;

public class RectangleShape : RoomShape 
{
    private List<Vector2Int> tiles;
    private RectInt bounds;
    private Vector2Int center;
    public RectangleShape(RectInt leafBounds, bool useRandomSize) 
    {
        if (useRandomSize) 
        {
            GenerateRandomSize(leafBounds);
        }
        else 
        {
            GeneratePaddingMode(leafBounds);
        }

        tiles = new List<Vector2Int>();
        for (int x = 0; x < bounds.width; x++) 
        {
            for (int y = 0; y < bounds.height; y++) 
            {
                tiles.Add(new Vector2Int(bounds.x + x, bounds.y + y));
            }
        }

        center = new Vector2Int(bounds.x + bounds.width / 2, bounds.y + bounds.height / 2);
    }

    private void GenerateRandomSize(RectInt leafBounds) 
    {
        int minMargin = 1;
        int maxWidth = leafBounds.width - minMargin * 2;
        int maxHeight = leafBounds.height - minMargin * 2;

        int roomWidth = Random.Range(Mathf.Max(3, maxWidth / 2), maxWidth + 1);
        int roomHeight = Random.Range(Mathf.Max(3, maxHeight / 2), maxHeight + 1);

        int roomX = leafBounds.x + Random.Range(minMargin, Mathf.Max(minMargin + 1, leafBounds.width - roomWidth - minMargin + 1));
        int roomY = leafBounds.y + Random.Range(minMargin, Mathf.Max(minMargin + 1, leafBounds.height - roomHeight - minMargin + 1));

        this.bounds = new RectInt(roomX, roomY, roomWidth, roomHeight);
    }

    private void GeneratePaddingMode(RectInt leafBounds) 
    {
        int padding = 2;
        int width = Mathf.Max(1, leafBounds.width - padding * 2);
        int height = Mathf.Max(1, leafBounds.height - padding * 2);
        int x = leafBounds.x + padding;
        int y = leafBounds.y + padding;

        this.bounds = new RectInt(x, y, width, height);
    }

    public List<Vector2Int> GetTiles() => tiles;
    public RectInt GetBounds() => bounds;
    public Vector2Int GetCenter() => center;
}