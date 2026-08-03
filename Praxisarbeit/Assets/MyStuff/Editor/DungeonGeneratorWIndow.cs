using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class DungeonGeneratorWindow : EditorWindow
{
    private Transform dungeonTargetContainer;
    private RoomTypeLibrary roomTypeLibrary;
    private DungeonData dungeonData;

    private int dungeonWidth = 100;
    private int dungeonLength = 100;
    private int minRoomSize = 15;
    private int maxSplitDepth = 5;

    //private int roomPadding = 2;

    private bool useFixedSeed = true;
    private int seed = 0;
    private bool useRandomRoomSizes = true;

    private CorridorWidthPreset corridorWidthPreset = CorridorWidthPreset.medium;

    private Vector2 roomEditorScrollPosition = Vector2.zero;

    private Dictionary<RoomType, bool> roomFoldouts = new Dictionary<RoomType, bool>();

    [MenuItem("Praxisarbeit/DungeonGenerator")]
    public static void ShowWindow() => GetWindow<DungeonGeneratorWindow>("Dungeon Generator");

    private void OnEnable()
    {
        AutoAssignReferences();
    }

    private void AutoAssignReferences()
    {
        if (dungeonTargetContainer == null)
        {
            GameObject parentGo = GameObject.Find("Dungeon Parent");

            if (parentGo != null)
            {
                dungeonTargetContainer = parentGo.transform;
            }
        }
        if (roomTypeLibrary == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:RoomTypeLibrary");

            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                roomTypeLibrary = AssetDatabase.LoadAssetAtPath<RoomTypeLibrary>(path);
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Dungeon Settings", EditorStyles.toolbarButton))
            currentTab = Tab.Dungeon;
        if (GUILayout.Button("Room Settings", EditorStyles.toolbarButton))
            currentTab = Tab.Rooms;
        EditorGUILayout.EndHorizontal();

        if (currentTab == Tab.Dungeon)
            DrawDungeonSettings();
        else if (currentTab == Tab.Rooms)
            DrawRoomSettings();
    }

    private enum Tab { Dungeon, Rooms }
    private Tab currentTab = Tab.Dungeon;

    private void DrawDungeonSettings()
    {
        EditorGUILayout.Space(10);
        GUILayout.Label("Dungeon Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        GUIContent DungeonParentLabel = new GUIContent("Dungeon Parent", "Dungeon spawns as a Child Object in here");
        dungeonTargetContainer = (Transform)EditorGUILayout.ObjectField(DungeonParentLabel, dungeonTargetContainer, typeof(Transform), true);

        GUIContent roomTypeLibraryLabel = new GUIContent("Room Type Library", "Collection of RoomData that is to spawn in the Dungeon ");
        roomTypeLibrary = (RoomTypeLibrary)EditorGUILayout.ObjectField("Room Type Library", roomTypeLibrary, typeof(RoomTypeLibrary), false);

        EditorGUILayout.Space(10);

        GUILayout.Label("Size and Generation", EditorStyles.boldLabel);
        //EditorGUILayout.Space(10);

        GUIContent dungeonWidthLabel = new GUIContent("Dungeon Width", "Width of the Dungeon");
        dungeonWidth = EditorGUILayout.IntField(dungeonWidthLabel, dungeonWidth);

        GUIContent dungeonLengthLabel = new GUIContent("Dungeon Length", "Length of the Dungeon");
        dungeonLength = EditorGUILayout.IntField(dungeonLengthLabel, dungeonLength);

        GUIContent minRoomSizeLabel = new GUIContent("Min Room Size", "Minimum Size of a Room possible");
        minRoomSize = EditorGUILayout.IntField(minRoomSizeLabel, minRoomSize);

        GUIContent maxSplitDepthLabel = new GUIContent("Max Split Depth", "Maximum amount the Dungeon Splits itself");
        maxSplitDepth = EditorGUILayout.IntField(maxSplitDepthLabel, maxSplitDepth);

        //GUIContent roomPaddingLabel = new GUIContent("Room Padding", "Space between Rooms");
        //roomPadding = EditorGUILayout.IntField(roomPaddingLabel, roomPadding);

        EditorGUILayout.Space(10);

        GUILayout.Label("Corridors", EditorStyles.boldLabel);
        GUIContent corridorWidthPresetLabel = new GUIContent("Corridor Width", "Small = 1 Tile, Medium = 2 Tiles, Max = 3 Tiles");
        corridorWidthPreset = (CorridorWidthPreset)EditorGUILayout.EnumPopup(corridorWidthPresetLabel, corridorWidthPreset);

        EditorGUILayout.Space(10);

        GUILayout.Label("Seed", EditorStyles.boldLabel);
        GUIContent useFixedSeedlabel = new GUIContent("Fixed Seed", "Toggle on = Fixed seed, Toggle off = random Seed based on system Time");
        useFixedSeed = EditorGUILayout.Toggle(useFixedSeedlabel, useFixedSeed);
        using (new EditorGUI.DisabledScope(!useFixedSeed))
        {
            seed = EditorGUILayout.IntField("Seed", seed);
        }

        EditorGUILayout.Space(10);

        GUILayout.Label("Room Generation", EditorStyles.boldLabel);
        GUIContent roomGenerationLabel = new GUIContent("Random Room Sizes", "true = Random Sizes, false = Padding Mode");
        useRandomRoomSizes = EditorGUILayout.Toggle(roomGenerationLabel, useRandomRoomSizes);

        EditorGUILayout.Space(10);

        GUIContent generateDungeonLabel = new GUIContent("Generate Dungeon", "Generates the Dungeon");
        {
            if (GUILayout.Button(generateDungeonLabel, GUILayout.Height(40)))
            {
                TryGenerateDungeon();
            }
        }

        GUIContent deleteDungeonLabel = new GUIContent("Delete Dungeon", "Deletes the whole Dungeon");
        if (GUILayout.Button(deleteDungeonLabel, GUILayout.Height(40)))
        {
            if (dungeonTargetContainer != null)
            {
                DungeonSpawner.ClearDungeon(dungeonTargetContainer);
            }
        }

        EditorGUILayout.Space(10);

        GUILayout.Label("JSON Im- & Export", EditorStyles.boldLabel);
        //EditorGUILayout.Space(10);

        GUIContent exportAllLabel = new GUIContent("Export", "Exports all Dungeon and RoomData into Json file.");
        if (GUILayout.Button(exportAllLabel, GUILayout.Height(40)))
        {
            if (roomTypeLibrary == null)
            {
                Debug.LogError("[DungeonEditor] Export canceled: No Room Type Library assigned!");
            }
            else if (dungeonData == null)
            {
                Debug.LogWarning("[DungeonEditor] Layout export skipped: First generate a dungeon to save the layout. Only exported roomdata");
                JsonIO.ExportAll(roomTypeLibrary, null);
            }
            else
            {
                JsonIO.ExportAll(roomTypeLibrary, dungeonData);
            }
        }

        GUIContent importAllLabel = new GUIContent("Import", "Imports all Dungeon and RoomData from Json file.");
        if (GUILayout.Button(importAllLabel, GUILayout.Height(40)))
        {
            if (roomTypeLibrary == null)
            {
                Debug.LogError("[DungeonEditor] Import canceled: No Room Type Library assigned!");
                return;
            }

            if (dungeonTargetContainer != null)
            {
                DungeonSpawner.ClearDungeon(dungeonTargetContainer);
            }

            dungeonData = JsonIO.ImportAll(roomTypeLibrary);

            if (dungeonData != null && dungeonTargetContainer != null)
            {
                DungeonSpawner.Spawn(dungeonData, roomTypeLibrary, dungeonTargetContainer);
                Debug.Log("[DungeonEditor] Import Completed and built in Szene");
            }
            else
            {
                DungeonSpawner.UpdateDungeonFull();
                Debug.LogWarning("[DungeonEditor] Room data was loaded, but no valid dungeon layout file was found.");
            }
        }
    }

    private void DrawRoomSettings()
    {
        EditorGUILayout.Space(10);

        GUILayout.Label("Room Configuration", EditorStyles.boldLabel);

        EditorGUILayout.Space(10);

        roomEditorScrollPosition = EditorGUILayout.BeginScrollView(roomEditorScrollPosition);

        foreach (RoomType roomType in System.Enum.GetValues(typeof(RoomType)))
        {
            /*if (roomType == RoomType.Standard)
                continue;*/

            DrawRoomTypeEditor(roomType);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawRoomTypeEditor(RoomType roomType)
    {
        if (!roomFoldouts.ContainsKey(roomType))
            roomFoldouts[roomType] = false;

        RoomData roomData = roomTypeLibrary.GetRoomData(roomType);

        string headerLabel = $"{roomType}";
        if (roomData != null)
            headerLabel += $" ({roomData.roomName})";
        else
            headerLabel += " (not configured)";

        roomFoldouts[roomType] = EditorGUILayout.Foldout(roomFoldouts[roomType], headerLabel, EditorStyles.foldoutHeader);

        if (!roomFoldouts[roomType])
            return;

        EditorGUI.indentLevel++;
        {
            GUIContent roomDataLebel = new GUIContent("RoomData SO", "Scriptable Object thats Holds Data properties");
            RoomData newRoomData = (RoomData)EditorGUILayout.ObjectField(roomDataLebel, roomData, typeof(RoomData), false);

            if (roomData != null)
            {
                EditorGUILayout.Space(10);

                SerializedObject serializedRoomData = new SerializedObject(roomData);
                serializedRoomData.Update();

                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);
                    GUIContent FloorPrefabLabel = new GUIContent("Floor Prefab", "Prefabs to be used as Floor for this Room");
                    GameObject newFloorPrefab = (GameObject)EditorGUILayout.ObjectField(FloorPrefabLabel, roomData.floorPrefab, typeof(GameObject), false);

                    GUIContent WallPrefabLabel = new GUIContent("Wall Prefab", "Prefab to be used as Wall for this Room");
                    GameObject newWallPrefab = (GameObject)EditorGUILayout.ObjectField(WallPrefabLabel, roomData.wallPrefab, typeof(GameObject), false);

                    GUIContent CeilingPrefabLabel = new GUIContent("Ceiling Prefab", "Prefab to be used as Ceiling for this Room");
                    GameObject newCeilingPrefab = (GameObject)EditorGUILayout.ObjectField(CeilingPrefabLabel, roomData.ceilingPrefab, typeof(GameObject), false);

                    GUIContent DoorPrefabLabel = new GUIContent("Door Prefab", "Prefab to be used as Door in this Room");
                    GameObject newDoorPrefab = (GameObject)EditorGUILayout.ObjectField(DoorPrefabLabel, roomData.doorPrefab, typeof(GameObject), false);

                    GUIContent WallHeightLabel = new GUIContent("Wall Height", "Height of the Walls in this Room");
                    int newWallHeight = EditorGUILayout.IntField(WallHeightLabel, roomData.wallHeight);
                    newWallHeight = Mathf.Max(3, newWallHeight);

                    EditorGUILayout.Space(10);

                    SerializedProperty contentProperty = serializedRoomData.FindProperty("possibleContents");
                    if (contentProperty != null)
                    {
                        EditorGUILayout.PropertyField(contentProperty, new GUIContent("Spawnable Content Rules", "Prefabs with Min/Max Settings for this Room"), true);
                    }

                    GUILayout.Space(10);

                    EditorGUILayout.LabelField("Spawn Behaviour", EditorStyles.boldLabel);
                    GUIContent SpawnChanceLabel = new GUIContent("Spawn Chance", "Chance for this Roomtype to be generated");
                    float newChance = EditorGUILayout.Slider(SpawnChanceLabel, roomData.spawnChance, 0f, 1f);

                    GUIContent MaxSpawnAmountLabel = new GUIContent("Max Spawn Amount", "Maximum number of this Room type to be generated");
                    int newAmount = EditorGUILayout.IntField(MaxSpawnAmountLabel, roomData.spawnAmount);

                    if (EditorGUI.EndChangeCheck())
                    {
                        bool prefabsChanged = (roomData.floorPrefab != newFloorPrefab || roomData.wallPrefab != newWallPrefab || roomData.ceilingPrefab != newCeilingPrefab || roomData.doorPrefab != newDoorPrefab || roomData.wallHeight != newWallHeight);
                        bool spawnSettingsChanged = (roomData.spawnChance != newChance || roomData.spawnAmount != newAmount);

                        Undo.RecordObject(roomData, "Modify Room Data");

                        roomData.floorPrefab = newFloorPrefab;
                        roomData.wallPrefab = newWallPrefab;
                        roomData.ceilingPrefab = newCeilingPrefab;
                        roomData.doorPrefab = newDoorPrefab;
                        roomData.wallHeight = newWallHeight;
                        roomData.spawnChance = newChance;
                        roomData.spawnAmount = Mathf.Max(1, newAmount);

                        EditorUtility.SetDirty(roomData);

                        if (prefabsChanged)
                            DungeonSpawner.UpdateDungeonForAsset(roomData);
                        else if (spawnSettingsChanged)
                            DungeonSpawner.UpdateDungeonFull();
                    }
                }

                if (serializedRoomData.ApplyModifiedProperties())
                {
                    DungeonSpawner.UpdateDungeonForAsset(roomData);
                }

                EditorGUILayout.Space(10);
            }
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(10);
    }

    private void TryGenerateDungeon()
    {
        if (!Validation.ValidateReferences(dungeonTargetContainer, roomTypeLibrary, out string refError))
        {
            Debug.LogWarning(refError);
            return;
        }
        if (!Validation.ValidateGenerationSettings(dungeonWidth, dungeonLength, minRoomSize, maxSplitDepth, out string settingsError))
        {
            Debug.LogWarning(settingsError);
            return;
        }

        int corridorWidth = (int)corridorWidthPreset;
        int actualSeed = useFixedSeed ? seed : System.Environment.TickCount;
        Validation.LogValidationSUccess(dungeonWidth, dungeonLength, minRoomSize, maxSplitDepth, corridorWidth, useRandomRoomSizes, actualSeed);

        Random.InitState(actualSeed);

        //int corridorWidth = (int)corridorWidthPreset;

        dungeonData = GenerateDungeonData(corridorWidth);

        DungeonSpawner.ClearDungeon(dungeonTargetContainer);
        DungeonSpawner.Spawn(dungeonData, roomTypeLibrary, dungeonTargetContainer);
    }

    private DungeonData GenerateDungeonData(int corridorWidth)
    {
        BSPTree tree = new BSPTree(dungeonWidth, dungeonLength);
        List<BSPNode> leaves = tree.Build(minRoomSize, maxSplitDepth);
        List<RoomInstance> rooms = RoomFactory.CreateRooms(leaves, useRandomRoomSizes);
        CorridorResult corridorResult = CorridorGenerator.Connect(tree.Root, rooms, corridorWidth);
        RoomClassifier.Classify(rooms, corridorResult.Connections, roomTypeLibrary);

        return new DungeonData(rooms, corridorResult.Tiles, corridorResult.Connections);
    }
}