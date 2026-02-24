using MergeCubes.Config;
using MergeCubes.Game;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MergeCubes
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameConfigSO _gameConfigSO;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_gameConfigSO).As<GameConfigSO>();
            builder.Register<LevelRepository>(Lifetime.Singleton);
            
            builder.RegisterComponentInHierarchy<Test>();
        }
    }
}
