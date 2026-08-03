using UnityEngine;

public class BSPNode
{
    public RectInt Bounds { get; } 
    public BSPNode Left { get; private set; } 
    public BSPNode Right { get; private set; } 
    public bool IsLeaf => Left == null && Right == null; 

    public BSPNode(RectInt bounds) 
    {
        Bounds = bounds;
    }
    public void SetChildren(BSPNode left, BSPNode right) 
    {
        Left = left;
        Right = right;
    }
}
