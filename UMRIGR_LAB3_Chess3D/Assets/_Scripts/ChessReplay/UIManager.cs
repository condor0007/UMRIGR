using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChessReplay
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("Menu elements")]
        [SerializeField] private GameObject _pauseMenu;
        [SerializeField] private GameObject _settingsMenu;
        [SerializeField] private List<AudioSource> _sounds;
        [SerializeField] private SettingsLevels _settings;
        [SerializeField] private ReplayController _replayController;
        [SerializeField] private List<TMP_Text> _saves;
        [SerializeField] private GameObject _filesMenu;
        [Header("Buttons")]
        [SerializeField] private TMP_InputField _autplaySpeed;
        [SerializeField] private Button _lastMovePlayButton;
        [SerializeField] private Button _nextMovePlayButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _resumePauseButton;
        [SerializeField] private Button _settingsPauseButton;
        [SerializeField] private Button _mainMenuPauseButton;
        [SerializeField] private Button _quitPauseButton;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Button _returnSettingsButton;
        [SerializeField] private Button _file1SaveButton;
        [SerializeField] private Button _file2SaveButton;
        [SerializeField] private Button _file3SaveButton;
        [SerializeField] private Button _file4SaveButton;
        [SerializeField] private Button _mainMenuSaveButton;
        [SerializeField] private Button _autoPlay;

        private void Start()
        {
            foreach (AudioSource sound in _sounds)
            {
                sound.volume = _settings.SoundLevels;
            }
            _volumeSlider.value = _settings.SoundLevels;

            for(int i = 0; i < _saves.Count; i++)
            {
                if (File.Exists(Application.persistentDataPath + "/save" + (i + 1) + ".json") == false){
                    _saves[i].SetText("Empty");
                }
            }

            _autplaySpeed.onValueChanged.AddListener(value => AutoplaySpeedChange());
            _lastMovePlayButton.onClick.AddListener(LastTurn);
            _nextMovePlayButton.onClick.AddListener(NextTurn);
            _pauseButton.onClick.AddListener(Pause);
            _resumePauseButton.onClick.AddListener(Resume);
            _settingsPauseButton.onClick.AddListener(Settings);
            _mainMenuPauseButton.onClick.AddListener(MainMenu);
            _quitPauseButton.onClick.AddListener(Quit);
            _volumeSlider.onValueChanged.AddListener(value => VolumeChanged());
            _returnSettingsButton.onClick.AddListener(ReturnFromSettings);
            _file1SaveButton.onClick.AddListener(() => Save(0));
            _file2SaveButton.onClick.AddListener(() => Save(1));
            _file3SaveButton.onClick.AddListener(() => Save(2));
            _file4SaveButton.onClick.AddListener(() => Save(3));
            _mainMenuSaveButton.onClick.AddListener(MainMenu);
            _autoPlay.onClick.AddListener(StartAutoplay);
        }

        public void Pause()
        {
            _pauseMenu.SetActive(true);
            _replayController.StopAutoplay();
        }

        public void Resume()
        {
            _pauseMenu.SetActive(false);
        }

        public void Settings()
        {
            _settingsMenu.SetActive(true);
        }

        public void ReturnFromSettings()
        {
            _settingsMenu.SetActive(false);
        }

        public void VolumeChanged()
        {
            foreach (AudioSource sound in _sounds)
            {
                sound.volume = _volumeSlider.value;
            }
            _settings.SoundLevels = _volumeSlider.value;
        }

        public void MainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void AutoplaySpeedChange()
        {
            if(float.TryParse(_autplaySpeed.text, out float speed))
            { 
                _replayController.TurnSpeed = speed;
            }
        }

        public void NextTurn()
        {
            _replayController.NextTurn();
        }

        public void LastTurn()
        {
            _replayController.LastTurn();
        }

        public void StartAutoplay()
        {
            _replayController.StartAutoPlay();
        }

        public void Save(int index)
        {
            if(string.Compare(_saves[index].text, "Empty") == 0)
            {
                return;
            }

            _replayController.Initialize(index);
            _filesMenu.SetActive(false);
        }
    }
}
