using MergeCubes.Config;
using MergeCubes.Game.Board;
using MergeCubes.Game.Level;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MergeCubes.Game.Bootstrapp
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameConfigSO _gameConfigSO;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_gameConfigSO).As<GameConfigSO>();
            builder.Register<LevelRepository>(Lifetime.Singleton);
            
            builder.RegisterComponentInHierarchy<Test.Test>();
            builder.Register<NormalizationController>(Lifetime.Singleton);
            builder.Register<BoardModel>(Lifetime.Singleton);
            builder.Register<GravityResolver>(Lifetime.Singleton);
            builder.Register<MatchFinder>(Lifetime.Singleton);

        }
    }
}
