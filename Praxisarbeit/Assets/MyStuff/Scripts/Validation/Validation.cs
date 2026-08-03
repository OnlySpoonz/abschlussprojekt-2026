using UnityEngine;

public static class Validation 
{
    private const string prefix = "[DungeonGenerator]";

    public static bool ValidateReferences(Transform container, RoomTypeLibrary library, out string errorMessage) 
    {
        errorMessage = null;

        if (container == null) 
        {
            errorMessage = $"{prefix} Error: 'Dungeon Parent' not assigned." + "Please choose a GameObject to spawn the dungeon into";
            return false;
        }
        if (library == null)
        {
            errorMessage = $"{prefix} Error: 'Room Type Library not assigned'." + " Either assign a existing one or create a new library: Richt-click -> Create -> DungeonGenerator -> Room Type Library";
                return false;
        }
        return true;
    }

    public static bool ValidateGenerationSettings(int width, int length, int minRoomSize, int maxDepth, out string errorMessage)
    {
        errorMessage = null;

        if (width <= 0 || length <= 0)
        {
            errorMessage = $"{prefix} Error: Dungeon Width and Length must be > than 0" + $"Currently: {width}x{length}";
            return false;
        }
        if (minRoomSize <= 0)
        {
            errorMessage = $"{prefix} Error: Min Room Size must be > than 0. Currently: {minRoomSize}";
            return false;
        }
        if (maxDepth <= 0)
        {
            errorMessage = $"{prefix} Error: maxDepth must be > than 0. Currently: {maxDepth}";
            return false;
        }

        int smallestDimension = Mathf.Min(width, length);
        if (minRoomSize * 2 > smallestDimension)
        {
            int maxAllowedMinRoomSize = smallestDimension / 2;
            errorMessage = $"{prefix} Error: MinRoomSize({minRoomSize}) is too big for dungeon size {smallestDimension}." + $"Maximum: {maxAllowedMinRoomSize}(must be less than half of the smallest dimension)";
            return false;
        }
        if (maxDepth >= 15)
        {
            errorMessage = $"{prefix} Warning: Max Split Depth ({maxDepth}) is very high." + $"This leads to an exponential number of spaces. Recommended: <= 10.";
            return false;
        }
        if (width * length >= 100000)
        {
            errorMessage = $"{prefix} Warning: Dungeon-Size ({width}*{length} = {width * length} tiles) is to big. " + $"This could lead to performance issues and will most likely lead to a crash";
            return false;
        }
        return true;
    }

    public static void LogValidationSUccess(int width, int lenght, int minRoomSize, int maxDepth, int corridorWidth, bool useRandomSizes, int seed)
    {
        Debug.Log($"{prefix} Vailidation successful. Starting generation with:\n" + $"Size: {width}x{lenght} Tiles\n" + $"Min Room Size: {minRoomSize}\n"+ $"Max Split Depth: {maxDepth}\n" + $"Corridor Width: {corridorWidth}\n" + $"Random Room Sizes: {(useRandomSizes ? "Yes": "No")}\n" + $"Seed: {seed}");
    }
}
