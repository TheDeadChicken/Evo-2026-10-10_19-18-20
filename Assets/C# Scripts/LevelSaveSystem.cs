using UnityEngine;

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class LevelData {
    public int version = 1;
    public List<Vector3> tiles = new List<Vector3>();
}

public class LevelSaveSystem : MonoBehaviour {

    [SerializeField] private string levelName = "level1";

    private string PathFor(string name) =>
        Path.Combine(Application.persistentDataPath, name + ".json");

    private void Update() {
        if (!Main.instance.inEditMode) return;

        if (Keyboard.current.f5Key.wasPressedThisFrame) Save(levelName);
        if (Keyboard.current.f9Key.wasPressedThisFrame) Load(levelName);
    }

    public void Save(string name) {
        LevelData data = new LevelData();
        data.tiles.AddRange(HexPlacer.instance.tileLocations.Keys);

        File.WriteAllText(PathFor(name), JsonUtility.ToJson(data, true));
        Debug.Log($"Saved {data.tiles.Count} tiles to {PathFor(name)}");
    }

    public void Load(string name) {
        string path = PathFor(name);
        if (!File.Exists(path)) {
            Debug.LogWarning("No save found at " + path);
            return;
        }

        LevelData data = JsonUtility.FromJson<LevelData>(File.ReadAllText(path));

        HexPlacer.instance.ClearTiles();
        foreach (Vector3 pos in data.tiles) {
            HexPlacer.instance.AddTile(pos);
        }
        Debug.Log($"Loaded {data.tiles.Count} tiles");
    }
}
