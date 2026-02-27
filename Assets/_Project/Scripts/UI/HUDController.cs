using System;
using DG.Tweening;
using IKhom.EventBusSystem.Runtime;
using MergeCubes.Events;
using UnityEngine;
using UnityEngine.UI;

namespace MergeCubes.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private CanvasGroup _nextButtonCG;

        private EventBinding<LevelWonEvent> _onLevelWon;
        private EventBinding<LevelLoadedEvent> _onLevelLoaded;

        private void Awake()
        {
            _restartButton.onClick.AddListener(HandleRestartPress);
            _nextButton.onClick.AddListener(HandleNextPress);

            _onLevelWon = new EventBinding<LevelWonEvent>(HandleLevelWon);
            _onLevelLoaded = new EventBinding<LevelLoadedEvent>(HandleLevelLoaded);

            EventBus<LevelWonEvent>.Register(_onLevelWon);
            EventBus<LevelLoadedEvent>.Register(_onLevelLoaded);
        }

        private void Start()
        {
            SetNextVisible(false, instant: true);
        }

        private void OnDestroy()
        {
            _restartButton.onClick.RemoveListener(HandleRestartPress);
            _nextButton.onClick.RemoveListener(HandleNextPress);
            EventBus<LevelWonEvent>.Deregister(_onLevelWon);
            EventBus<LevelLoadedEvent>.Deregister(_onLevelLoaded);
        }

        private void HandleLevelLoaded(LevelLoadedEvent obj) =>
            SetNextVisible(false, instant: true);

        private void HandleLevelWon(LevelWonEvent obj) =>
            SetNextVisible(true);

        private void HandleNextPress() =>
            EventBus<NextLevelRequestedEvent>.Raise(new NextLevelRequestedEvent());

        private void HandleRestartPress() =>
            EventBus<RestartRequestedEvent>.Raise(new RestartRequestedEvent());


        private void SetNextVisible(bool visible, bool instant = false)
        {
            var targetAlpha = visible ? 1f : 0f;

            _nextButtonCG.interactable = visible;
            _nextButtonCG.blocksRaycasts = visible;

            _nextButtonCG.DOKill();

            if (instant)
                _nextButtonCG.alpha = targetAlpha;
            else
            {
                _nextButtonCG.DOFade(targetAlpha, .4f).SetEase(Ease.OutBack);
            }
        }
    }
}