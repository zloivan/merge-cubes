namespace MergeCubes.Saving
{
    public interface ISaveService
    {
        SaveData Load();
        void Save(SaveData saveData);
        void Delete();
    }
}