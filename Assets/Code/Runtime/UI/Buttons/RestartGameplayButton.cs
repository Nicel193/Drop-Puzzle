using Code.Runtime.Interactors;
using Code.Runtime.Services.Progress;
using Zenject;

namespace Code.Runtime.UI.Buttons
{
    public class RestartGameplayButton : ChangeSceneButton
    {
        private GameplayShapesInteractor _gameplayShapesInteractor;

        [Inject]
        public void Construct(IPersistentProgressService progressService)
        {
            _gameplayShapesInteractor = progressService.InteractorContainer.Get<GameplayShapesInteractor>();
        }

        protected override void ChangeState()
        {
            _gameplayShapesInteractor.ClearShapesData();
            
            base.ChangeState();
        }
    }
}