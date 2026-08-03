using UnityEngine;
using System.Collections.Generic;
public class BSPTree
{
    private readonly int width;
    private readonly int length;
    public BSPNode Root { get; private set; }

    public BSPTree(int width, int length)
    {
        this.width = width;
        this.length = length;
    }
    public List<BSPNode> Build(int minRoomSize, int maxDepth)
    {
        Root = new BSPNode(new RectInt(0, 0, width, length));

        List<BSPNode> leaves = new List<BSPNode>();
        Split(Root, minRoomSize, maxDepth, leaves);
        return leaves;
    }
    private void Split(BSPNode node, int minRoomSize, int remainingDepth, List<BSPNode> leaves)
    {
        RectInt bounds = node.Bounds;
        bool canSplitWidth = bounds.width >= minRoomSize * 2;
        bool canSplitHeight = bounds.height >= minRoomSize * 2;

        if (remainingDepth <= 0 || (!canSplitWidth && !canSplitHeight))
        {
            leaves.Add(node);
            return;
        }

        BSPNode left;
        BSPNode right;

        if (Random.value > 0.5f && canSplitWidth)
        {
            int splitX = Random.Range(minRoomSize, bounds.width - minRoomSize);
            left = new BSPNode(new RectInt(bounds.x, bounds.y, splitX, bounds.height));
            right = new BSPNode(new RectInt(bounds.x + splitX, bounds.y, bounds.width - splitX, bounds.height));
        }
        else if (canSplitHeight)
        {
            int splitY = Random.Range(minRoomSize, bounds.height - minRoomSize);
            left = new BSPNode(new RectInt(bounds.x, bounds.y, bounds.width, splitY));
            right = new BSPNode(new RectInt(bounds.x, bounds.y + splitY, bounds.width, bounds.height - splitY));
        }
        else
        {
            int splitX = Random.Range(minRoomSize, bounds.width - minRoomSize);
            left = new BSPNode(new RectInt(bounds.x, bounds.y, splitX, bounds.height));
            right = new BSPNode(new RectInt(bounds.x + splitX, bounds.y, bounds.width - splitX, bounds.height));
        }

        node.SetChildren(left, right);
        Split(left, minRoomSize, remainingDepth - 1, leaves);
        Split(right, minRoomSize, remainingDepth - 1, leaves);
    }
}
