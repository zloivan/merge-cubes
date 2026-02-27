using IKhom.EventBusSystem.Runtime;
using IKhom.SoundSystem.Runtime.components;
using IKhom.SoundSystem.Runtime.data;
using MergeCubes.Config;
using MergeCubes.Events;
using UnityEngine;
using VContainer;

namespace MergeCubes.Audio
{
    public class GameSoundController : MonoBehaviour
    {
        private AudioConfigSO _audioConfig;
        private EventBinding<BlockMovedEvent> _onBlockMoved;
        private EventBinding<BlocksDestroyedEvent> _onBlocksDestroyed;
        private EventBinding<BlocksFellEvent> _onBlocksFell;
        private EventBinding<UIButtonClickedEvent> _onButtonClicked;
        private EventBinding<LevelWonEvent> _onLevelWon;

        private EventBinding<SwapExecutedEvent> _onSwap;
        private SoundManager _soundManager;

        private void Start()
        {
            _onButtonClicked = new EventBinding<UIButtonClickedEvent>(_ => PlayRandom(_audioConfig.ButtonClickSounds));


            _onSwap = new EventBinding<SwapExecutedEvent>(_ => PlayRandom(_audioConfig.SwapSounds));
            _onBlockMoved = new EventBinding<BlockMovedEvent>(_ => PlayRandom(_audioConfig.MoveSounds));
            _onBlocksFell = new EventBinding<BlocksFellEvent>(_ => PlayRandom(_audioConfig.FallSounds));
            _onBlocksDestroyed = new EventBinding<BlocksDestroyedEvent>(_ => PlayRandom(_audioConfig.DestroySounds));
            _onLevelWon = new EventBinding<LevelWonEvent>(_ => PlayRandom(_audioConfig.WinSounds));

            EventBus<UIButtonClickedEvent>.Register(_onButtonClicked);
            EventBus<SwapExecutedEvent>.Register(_onSwap);
            EventBus<BlockMovedEvent>.Register(_onBlockMoved);
            EventBus<BlocksFellEvent>.Register(_onBlocksFell);
            EventBus<BlocksDestroyedEvent>.Register(_onBlocksDestroyed);
            EventBus<LevelWonEvent>.Register(_onLevelWon);

            PlayAmbient();
        }

        private void OnDestroy()
        {
            EventBus<SwapExecutedEvent>.Deregister(_onSwap);
            EventBus<BlockMovedEvent>.Deregister(_onBlockMoved);
            EventBus<BlocksFellEvent>.Deregister(_onBlocksFell);
            EventBus<BlocksDestroyedEvent>.Deregister(_onBlocksDestroyed);
            EventBus<LevelWonEvent>.Deregister(_onLevelWon);
            EventBus<UIButtonClickedEvent>.Deregister(_onButtonClicked);
        }


        [Inject]
        public void Construct(SoundManager soundManager, GameConfigSO gameConfig)
        {
            _soundManager = soundManager;
            _audioConfig = gameConfig.AudioConfig;
        }

        private void PlayAmbient() =>
            _soundManager.CreateSound().WithSoundData(_audioConfig.BackgroundMusic).Play();

        private void PlayRandom(SoundData[] sounds)
        {
            if (sounds == null || sounds.Length == 0) return;
            _soundManager.CreateSound()
                .WithSoundData(sounds[Random.Range(0, sounds.Length)])
                .WithRandomPitch()
                .Play();
        }
    }
}