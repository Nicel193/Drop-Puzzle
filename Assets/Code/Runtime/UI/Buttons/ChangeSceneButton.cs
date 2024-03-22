using Code.Runtime.Infrastructure.StateMachines;
using Code.Runtime.Infrastructure.States;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Runtime.UI.Buttons
{
    public class ChangeSceneButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private SceneName nextScene;

        private GameStateMachine _gameStateMachine;

        [Inject]
        void Construct(GameStateMachine gameStateMachine) =>
            _gameStateMachine = gameStateMachine;

        private void OnEnable() =>
            button.onClick.AddListener(ChangeState);

        private void OnDisable() =>
            button.onClick.RemoveListener(ChangeState);

        protected virtual void ChangeState() =>
            _gameStateMachine.Enter<LoadSceneState, string>(nextScene.ToString());
    }
}