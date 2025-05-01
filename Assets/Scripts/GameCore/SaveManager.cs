using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private string savePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            savePath = Application.persistentDataPath + "/save.dat";
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame(Transform playerTransform)
    {
        SaveData data = new SaveData();

        Vector3 pos = playerTransform.position;
        data.playerPosition = new float[] { pos.x, pos.y, pos.z };

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Create);
        formatter.Serialize(stream, data);
        stream.Close();

        Debug.Log("Game Saved.");
    }

    public void LoadGame(Transform playerTransform)
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Save file not found!");
            return;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Open);
        SaveData data = formatter.Deserialize(stream) as SaveData;
        stream.Close();

        playerTransform.position = new Vector3(data.playerPosition[0], data.playerPosition[1], data.playerPosition[2]);

        Debug.Log("Game Loaded.");
    }
}
