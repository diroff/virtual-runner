using Abstractions.Interfaces;
using Cysharp.Threading.Tasks;
using Data;

public class SaveLoadService : IService
{
    private readonly IStorageService _storageService;
    private const string SaveKey = "game_save.json";

    public GameData Data { get; private set; } = new();

    public bool HasProgress => !string.IsNullOrEmpty(Data.Progress.LastReachedLevelName);

    public SaveLoadService(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public async UniTask Initialize()
    {
        await LoadAsync();
    }

    public async UniTask SaveAsync()
    {
        await _storageService.SaveAsync(SaveKey, Data);
    }

    public async UniTask LoadAsync()
    {
        var loadedData = await _storageService.LoadAsync<GameData>(SaveKey);
        Data = loadedData ?? new GameData();
    }
}