using Cysharp.Threading.Tasks;

namespace MergeCubes.Saving
{
    public interface ISaveService
    {
        SaveData Load();
        UniTask SaveAsync(SaveData saveData);
        void Delete();
    }
}