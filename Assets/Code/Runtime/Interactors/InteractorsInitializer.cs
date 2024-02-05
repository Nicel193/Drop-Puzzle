﻿using Code.Runtime.Configs;
using Code.Runtime.Repositories;
using CodeBase.Services.StaticDataService;

namespace Code.Runtime.Interactors
{
    public static class InteractorsInitializer
    {
        public static void Initialize(PlayerProgress playerProgress, IInteractorContainer interactorContainer,
            IStaticDataService staticDataService)
        {
            interactorContainer.CreateInteractor<ShapeInteractor, ShapeRepository>(playerProgress.ShapeRepository);
            interactorContainer.CreateInteractor<MoneyInteractor, MoneyRepository>(playerProgress.MoneyRepository);
            interactorContainer.CreateInteractor<GameplayShapesInteractor, GameplayShapesRepository>(playerProgress
                .GameplayShapesRepository);

            RegisterPurchasesInteractor(playerProgress, interactorContainer, staticDataService);
            RegisterScoreInteractor(playerProgress, interactorContainer, staticDataService);
        }

        private static void RegisterScoreInteractor(PlayerProgress playerProgress,
            IInteractorContainer interactorContainer,
            IStaticDataService staticDataService)
        {
            interactorContainer.CreateInteractor<ScoreInteractor, ScoreRepository, ShapeScoreConfig>(
                playerProgress.ScoreRepository,
                staticDataService.ShapeScoreConfig);
        }

        private static void RegisterPurchasesInteractor(PlayerProgress playerProgress,
            IInteractorContainer interactorContainer,
            IStaticDataService staticDataService)
        {
            interactorContainer.CreateInteractor<PurchasesInteractor, PurchasesRepository, PurchasesInteractor.Payload>(
                playerProgress.PurchasesRepository,
                new PurchasesInteractor.Payload(
                    staticDataService.PurchasedBackgroundsConfig,
                    interactorContainer.Get<MoneyInteractor>()));
        }
    }
}