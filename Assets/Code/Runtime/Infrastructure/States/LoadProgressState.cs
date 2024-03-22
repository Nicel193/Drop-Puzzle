using System;
using Code.Runtime.Configs;
using Code.Runtime.Interactors;
using Code.Runtime.Repositories;
using Code.Runtime.Services.AudioService;
using Code.Runtime.Services.Progress;
using Code.Runtime.Services.SaveLoadService;
using Code.Runtime.Services.StaticDataService;
using Plugin.DocuFlow.Documentation;

namespace Code.Runtime.Infrastructure.States
{
    [Doc("The LoadProgressState class responsible for loading progress and initializing game-related data before transitioning to the main menu scene.")]
    public class LoadProgressState : IState
    {
        private readonly IPersistentProgressService _persistentProgressService;
        private readonly IStaticDataService _staticDataService;
        private readonly ISaveLoadService _saveLoadService;
        private readonly ISceneLoader _sceneLoader;
        private readonly IAudioService _audioService;

        LoadProgressState(IPersistentProgressService persistentProgressService,
            IStaticDataService staticDataService, ISaveLoadService saveLoadService, ISceneLoader sceneLoader, IAudioService audioService)
        {
            _persistentProgressService = persistentProgressService;
            _staticDataService = staticDataService;
            _saveLoadService = saveLoadService;
            _sceneLoader = sceneLoader;
            _audioService = audioService;
        }

        public void Enter()
        {
            PlayerProgress playerProgress = InitializePlayerProgress();

            InteractorsInitializer.Initialize(playerProgress, _persistentProgressService.InteractorContainer,
                _staticDataService, _saveLoadService);
            _audioService.Initialize();
            
            _sceneLoader.Load(SceneName.Menu.ToString());
        }

        private PlayerProgress InitializePlayerProgress()
        {
            if (!_saveLoadService.TryLoadProgress(out PlayerProgress playerProgress))
            {
                playerProgress = new PlayerProgress();

                FirstLoadGameData(playerProgress);
            }

            _saveLoadService.Initialize(playerProgress);
            return playerProgress;
        }

        private void FirstLoadGameData(PlayerProgress playerProgress)
        {
            playerProgress.GameplayData.MoneyRepository.Coins = 0;
            playerProgress.PurchasesRepository.PurchasedBackgrounds.Add(BackgroundType.Default);
            playerProgress.PurchasesRepository.SelectedBackground = BackgroundType.Default;

            playerProgress.SettingsRepository.IsEnableVibrate = true;
            playerProgress.SettingsRepository.IsEnableSFX = true;
            playerProgress.SettingsRepository.IsEnableAmbient = true;
        }

        public void Exit()
        {
        }
    }
}