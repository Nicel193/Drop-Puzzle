using System.Collections;
using Code.Runtime.Configs;
using Code.Runtime.Interactors;
using Code.Runtime.Logic.Gameplay;
using Code.Runtime.Services.AudioService;
using Code.Runtime.Services.Progress;
using Code.Runtime.UI;
using UnityEngine;

namespace Code.Runtime.Infrastructure.States.Gameplay
{
    public class LoseState : IState
    {
        private readonly PrefabFactory<ShapeSpawner> _spawnerFactory;
        private readonly PrefabFactory<HUD> _hudFactory;
        private readonly IPersistentProgressService _progressService;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IAudioService _audioService;

        private Camera mainCamera;
        private float initialSize;
        private float targetSize = 6f;
        private float changeDuration = 1.5f;

        public LoseState(PrefabFactory<ShapeSpawner> spawnerFactory, PrefabFactory<HUD> hudFactory,
            IPersistentProgressService progressService, ICoroutineRunner coroutineRunner, IAudioService audioService)
        {
            _audioService = audioService;
            _spawnerFactory = spawnerFactory;
            _hudFactory = hudFactory;
            _progressService = progressService;
            _coroutineRunner = coroutineRunner;
        }

        public void Enter()
        {
            mainCamera = Camera.main;
            initialSize = mainCamera.orthographicSize;

            _spawnerFactory.InstantiatedPrefab.gameObject.SetActive(false);

            _hudFactory.InstantiatedPrefab.ChangeToLose();

            ScoreInteractor scoreInteractor = _progressService.InteractorContainer.Get<ScoreInteractor>();
            
            _progressService.InteractorContainer.Get<RankingInteractor>().AddRecord(scoreInteractor.GetCurrentScore());

            scoreInteractor.ResetCurrentScore();
            _coroutineRunner.StartCoroutine(ChangeCameraSize());
        }

        public void Exit()
        {
        }

        IEnumerator ChangeCameraSize()
        {
            _hudFactory.InstantiatedPrefab.gameObject.SetActive(false);

            float timeElapsed = 0f;
            while (timeElapsed < changeDuration)
            {
                float newSize = Mathf.Lerp(initialSize, targetSize, timeElapsed / changeDuration);
                mainCamera.orthographicSize = newSize;

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            mainCamera.orthographicSize = targetSize;

            _hudFactory.InstantiatedPrefab.gameObject.SetActive(true);
            _audioService.PlaySfx(SfxType.Lose);
        }
    }
}