using Cinemachine;
using IKhom.SoundSystem.Runtime.components;
using MergeCubes.Audio;
using MergeCubes.Config;
using MergeCubes.Game.Balloons;
using MergeCubes.Game.Board;
using MergeCubes.Game.Level;
using MergeCubes.GameInput;
using MergeCubes.Saving;
using MergeCubes.Utilities;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using MergeCubes.UI;

namespace MergeCubes.Bootstrap
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameConfigSO _gameConfig;

        [SerializeField] private InputService _inputService;
        [SerializeField] private BoardView _boardView;
        [SerializeField] private CameraFitter _cameraFitter;
        [SerializeField] private BalloonsSpawner _balloonSpawner;
        [SerializeField] private HUDController _hudController;
        [SerializeField] private GameController _gameController;
        
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private CameraShakeController _shakeController;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private SoundManager _soundManager;
        [SerializeField] private GameSoundController _gameSoundController;
        [SerializeField] private BackgroundFitter _backgroundFitter;
        

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameConfig);
            builder.RegisterInstance(_mainCamera);

            builder.Register<BoardModel>(Lifetime.Singleton);
            builder.Register<SwipeValidator>(Lifetime.Singleton);
            builder.Register<GravityResolver>(Lifetime.Singleton);
            builder.Register<MatchFinder>(Lifetime.Singleton);

            builder.Register<LevelRepository>(Lifetime.Singleton).As<ILevelRepository>();
            builder.Register<NormalizationController>(Lifetime.Singleton);
            builder.Register<LevelController>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<BoardController>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<SaveService>(Lifetime.Singleton).As<ISaveService>();

            
            builder.RegisterComponent(_inputService);
            builder.RegisterComponent(_shakeController);
            builder.RegisterComponent(_boardView);
            builder.RegisterComponent(_cameraFitter);
            builder.RegisterComponent(_balloonSpawner);
            builder.RegisterComponent(_hudController);
            builder.RegisterComponent(_gameController);
            builder.RegisterComponent(_soundManager);
            builder.RegisterComponent(_gameSoundController);
            builder.RegisterComponent(_backgroundFitter);
        }
    }
}