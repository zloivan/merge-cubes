using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace MergeCubes.Saving
{
    [UsedImplicitly]
    
    public class SaveService : ISaveService
    {
        private const string SAVE_KEY = "SaveData.json";


        public SaveData Load()
        {
            try
            {
                var saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(GetPath()));

                if (saveData.SaveVersion <= SaveDataConverter.CURRENT_VERSION
                    && saveData.Width > 0
                    && saveData.Height > 0
                    && saveData.Blocks != null
                    && saveData.Blocks.Length == saveData.Width * saveData.Height)

                    return saveData;


                //TODO: 
                //Can be improved by implementing migration system, that will store list of migrations
                //assigned to app verion, and will apply interativly migrations one after another till
                //the last version is reached - overkill right now

                //corrupted
                Delete();
                return null;
            }
            catch
            {
                Delete();
                return null;
            }
        }

        public void Save(SaveData saveData)
        {
            File.WriteAllText(GetPath(), JsonUtility.ToJson(saveData));
        }

        public void Delete()
        {
            if (File.Exists(GetPath()))
                File.Delete(GetPath());
        }

        private string GetPath() =>
            Path.Combine(Application.persistentDataPath, SAVE_KEY);
    }
}