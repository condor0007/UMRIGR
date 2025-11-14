using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MainMenu
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("Menu elements")]
        [SerializeField] private GameObject _settingsMenu;
        [SerializeField] private GameObject _loadingScreen;
        [SerializeField] private SettingsLevels _settings;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private List<AudioSource> sounds;
        [SerializeField] private TextMeshProUGUI _loadPercent;
        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _settingsReturnButton;
        [SerializeField] private Slider _audioSlider;
        [SerializeField] private Slider _loadingSlider;

        private void Start()
        {
            foreach (AudioSource sound in sounds)
            {
                sound.volume = _settings.SoundLevels;
            }
            _volumeSlider.value = _settings.SoundLevels;

            _playButton.onClick.AddListener(Play);
            _replayButton.onClick.AddListener(Replay);
            _settingsButton.onClick.AddListener(Settings);
            _quitButton.onClick.AddListener(Quit);
            _settingsReturnButton.onClick.AddListener(ReturnFromSettings);
            _audioSlider.onValueChanged.AddListener(value => VolumeChanged());
        }

        public void Play()
        {
            StartCoroutine(AsyncLoading("ChessGameLoop"));
            _loadingScreen.SetActive(true);
        }

        public void Replay()
        {
            StartCoroutine(AsyncLoading("ChessReplay"));
            _loadingScreen.SetActive(true);
        }

        /// <summary>
        /// Loads scene given as parameter by name. Displays loading screen while waiting with filling progress bar.
        /// </summary>
        IEnumerator AsyncLoading(string scene)
        {
            AsyncOperation _loading = SceneManager.LoadSceneAsync(scene);

            while (_loading.isDone == false)
            {
                _loadingSlider.value = _loading.progress;
                _loadPercent.SetText((int)(_loading.progress * 100) + "%");
                yield return new WaitForSeconds(0.01f);
            }
        }

        public void Settings()
        {
            _settingsMenu.SetActive(true);
        }

        public void VolumeChanged()
        {
            foreach (AudioSource sound in sounds)
            {
                sound.volume = _volumeSlider.value;
            }
            _settings.SoundLevels = _volumeSlider.value;
        }

        public void ReturnFromSettings()
        {
            _settingsMenu.SetActive(false);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
