using IKhom.EventBusSystem.Runtime;
using MergeCubes.Events;
using MergeCubes.Game.Board;
using MergeCubes.Game.Level;
using MergeCubes.Saving;
using UnityEngine;
using VContainer;

namespace MergeCubes.Bootstrap
{
    /// <summary>
    ///     App entry point. Bootstraps game on Start. Handles restart. Saves on pause/quit.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        private BoardModel _boardModel;
        private LevelController _levelController;
        private NormalizationController _normalizationController;
        private EventBinding<NormalizationCompletedEvent> _onNormalizationCompleted;


        private EventBinding<RestartRequestedEvent> _onRestartRequested;
        private ISaveService _saveService;

        private void Awake()
        {
            _onRestartRequested = new EventBinding<RestartRequestedEvent>(HandleRestartRequested);
            EventBus<RestartRequestedEvent>.Register(_onRestartRequested);

            _onNormalizationCompleted = new EventBinding<NormalizationCompletedEvent>(HandleNormalizationCompleted);
            EventBus<NormalizationCompletedEvent>.Register(_onNormalizationCompleted);
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
            
            // Architecture note — GameFlowController
            // Current flow coordination lives in GameController + LevelController which creates
            // coupling between win detection, save timing, and level transitions.
            // For a production build, extract a GameFlowController (or FSM) that:
            //   - Owns game states: Playing → WinPending → (RewardScreen) → Loading
            //   - Decides WHEN to save progress (only after player confirms, not on board clear)
            //   - Handles interrupt scenarios (restart during win delay, quit before collecting rewards)
            // This would allow adding new flow steps (reward screens, interstitials) 
            // without touching LevelController or save logic.
            Bootstrap();
        }

        private void OnDestroy() =>
            EventBus<RestartRequestedEvent>.Deregister(_onRestartRequested);

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                Save();
        }

        private void OnApplicationQuit() =>
            Save();

        [Inject]
        public void Construct(LevelController levelController, ISaveService saveService,
            NormalizationController normalizationController, BoardModel boardModel)
        {
            _levelController = levelController;
            _saveService = saveService;
            _normalizationController = normalizationController;
            _boardModel = boardModel;
        }

        private void HandleNormalizationCompleted(NormalizationCompletedEvent obj) =>
            Save();

        private void HandleRestartRequested(RestartRequestedEvent e)
        {
            _normalizationController.Cancel();
            _saveService.Delete();
            _levelController.LoadLevel(_levelController.GetCurrentLevelIndex());
        }

        private void Bootstrap()
        {
            var saveData = _saveService.Load();

            if (saveData != null)
                _levelController.LoadFromSave(saveData);
            else
                _levelController.LoadLevel(0);
        }

        private void Save()
        {
            if (_boardModel.IsAllEmpty())
                return;

            _saveService.SaveAsync(SaveDataConverter.FromBoard(_boardModel, _levelController.GetCurrentLevelIndex()));
        }
    }
}