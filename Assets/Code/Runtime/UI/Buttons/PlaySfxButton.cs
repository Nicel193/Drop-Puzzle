using Code.Runtime.Configs;
using Code.Runtime.Services.AudioService;
using Code.Runtime.Services.StaticDataService;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Runtime.UI.Buttons
{
    [RequireComponent(typeof(Button))]
    public class PlaySfxButton : MonoBehaviour
    {
        [SerializeField] private SfxType sfxType;

        private Button _button;
        private Vector3 _startScale;
        private Tween _currentTween;

        private IAudioService _audioService;
        private IStaticDataService _staticDataService;

        [Inject]
        public void Construct(IAudioService audioService, IStaticDataService staticDataService)
        {
            _staticDataService = staticDataService;
            _audioService = audioService;
        }

        private void Awake()
        {
            _button = this.GetComponent<Button>();
            _startScale = this.transform.localScale;
        }

        private void OnEnable() =>
            _button.onClick.AddListener(Play);

        private void OnDisable() =>
            _button.onClick.RemoveListener(Play);

        private void OnDestroy() =>
            _currentTween?.Kill();

        private void Play()
        {
            if (_currentTween == null || !_currentTween.IsActive())
            {
                _currentTween = this.transform.DOPunchScale(
                    _staticDataService.AnimationConfig.GetPunchAnimationScaleFactor(),
                    _staticDataService.AnimationConfig.PunchAnimationScaleDuration);
            }

            PlaySfx();
        }

        private void PlaySfx() =>
            _audioService.PlaySfx(sfxType);
    }
}