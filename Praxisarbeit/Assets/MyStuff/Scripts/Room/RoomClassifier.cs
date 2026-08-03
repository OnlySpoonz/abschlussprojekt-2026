using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class RoomClassifier
{

    public static void Classify(List<RoomInstance> rooms, List<(RoomInstance a, RoomInstance b)> connections, RoomTypeLibrary library)
    {
        if (rooms == null || rooms.Count == 0 || library == null)
            return;

        foreach (RoomInstance room in rooms)
            room.SetType(RoomType.Standard);

        List<RoomInstance> unassignedRooms = new List<RoomInstance>(rooms);

        RoomInstance spawnRoom = unassignedRooms[Random.Range(0, unassignedRooms.Count)];
        spawnRoom.SetType(RoomType.Spawn);
        unassignedRooms.Remove(spawnRoom);

        if (unassignedRooms.Count > 0)
        {
            RoomInstance bossRoom = FindFarthestRoom(spawnRoom, rooms, connections);

            if (bossRoom != null && bossRoom.Type == RoomType.Standard)
            {
                bossRoom.SetType(RoomType.Boss);
                unassignedRooms.Remove(bossRoom);
            }

            RoomData bossData = library.GetRoomData(RoomType.Boss);

            if (bossData != null && bossData.spawnAmount > 1)
            {
                int additionalBossCount = bossData.spawnAmount - 1;
                for (int i = 0; i < additionalBossCount && unassignedRooms.Count > 0; i++)
                {
                    int randomIndex = Random.Range(0, unassignedRooms.Count);
                    unassignedRooms[randomIndex].SetType(RoomType.Boss);
                    unassignedRooms.RemoveAt(randomIndex);
                }
            }
        }
        AssignRoomType(RoomType.Treasure, unassignedRooms, library);
        AssignRoomType(RoomType.Puzzle, unassignedRooms, library);
        AssignRoomType(RoomType.Shop, unassignedRooms, library);
        AssignRoomType(RoomType.NPC, unassignedRooms, library);
    }

    private static void AssignRoomType(RoomType typeToAssign, List<RoomInstance> unassignedRooms, RoomTypeLibrary library)
    {
        RoomData typeData = library.GetRoomData(typeToAssign);

        if (typeData == null || typeData.spawnChance <= 0f)
            return;

        int maxCount = typeData.spawnAmount;
        int assigned = 0;

        List<RoomInstance> shuffledRooms = new List<RoomInstance>(unassignedRooms);
        ShuffleList(shuffledRooms);

        for (int i = 0; i < shuffledRooms.Count; i++)
        {
            if (Random.value > typeData.spawnChance)
                continue;

            if (assigned >= maxCount)
                break;

            RoomInstance room = shuffledRooms[i];
            room.SetType(typeToAssign);
            unassignedRooms.Remove(room);
            assigned++;
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
    private static RoomInstance FindFarthestRoom(RoomInstance start, List<RoomInstance> rooms, List<(RoomInstance a, RoomInstance b)> connections)
    {
        Dictionary<int, List<RoomInstance>> adjacency = new Dictionary<int, List<RoomInstance>>();

        foreach (RoomInstance room in rooms)
        {
            adjacency[room.Id] = new List<RoomInstance>();
        }
        foreach (var (a, b) in connections)
        {
            adjacency[a.Id].Add(b); adjacency[b.Id].Add(a);
        }

        Dictionary<int, int> distances = new Dictionary<int, int> { [start.Id] = 0 };
        Queue<RoomInstance> queue = new Queue<RoomInstance>();
        queue.Enqueue(start);

        RoomInstance farthestRoom = start;
        int farthestDistance = 0;

        while (queue.Count > 0)
        {
            RoomInstance current = queue.Dequeue();
            int currentDistance = distances[current.Id];

            foreach (RoomInstance neighbor in adjacency[current.Id])
            {
                if (distances.ContainsKey(neighbor.Id)) continue;

                int neighborDistance = currentDistance + 1;
                distances[neighbor.Id] = neighborDistance;
                queue.Enqueue(neighbor);

                if (neighborDistance > farthestDistance)
                {
                    farthestDistance = neighborDistance;
                    farthestRoom = neighbor;
                }
            }
        }
        return farthestRoom;
    }
}