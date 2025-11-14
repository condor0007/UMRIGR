using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChessMainLoop
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("Menu refreces")]
        [SerializeField] private GameObject _pauseMenu;
        [SerializeField] private GameObject _settingsMenu;
        [SerializeField] private GameObject _gameOverMenu;
        [SerializeField] private GameObject _pawnPromotionMenu;
        [SerializeField] private List<AudioSource> _sounds;
        [SerializeField] private TextMeshProUGUI _winnerText;
        [SerializeField] private SettingsLevels _settings;
        [Header("Buttons")]
        [SerializeField] private Button _queenSelectionButton;
        [SerializeField] private Button _rookSelectionButton;
        [SerializeField] private Button _bishopSelectionButton;
        [SerializeField] private Button _knightSelectionButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _playAgainEndGameButton;
        [SerializeField] private Button _saveGameEndGameButton;
        [SerializeField] private Button _settingsEndGameButton;
        [SerializeField] private Button _mainMenuEndGameButton;
        [SerializeField] private Button _quitEndGameButton;
        [SerializeField] private Button _resumePauseButton;
        [SerializeField] private Button _settingsPauseButton;
        [SerializeField] private Button _mainMenuPauseButton;
        [SerializeField] private Button _quitPauseButton;
        [SerializeField] private Button _returnSettingsButton;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Button _file1SaveButton;
        [SerializeField] private Button _file2SaveButton;
        [SerializeField] private Button _file3SaveButton;
        [SerializeField] private Button _file4SaveButton;
        [SerializeField] private Button _returnSaveButton;
        [Header("Piece prefab references")]
        [SerializeField] private Queen _whiteQueen;
        [SerializeField] private Queen _blackQueen;
        [SerializeField] private Bishop _whiteBishop;
        [SerializeField] private Bishop _blackBishop;
        [SerializeField] private Rook _whiteRook;
        [SerializeField] private Rook _blackRook;
        [SerializeField] private Knight _whiteKnight;
        [SerializeField] private Knight _blackKnight;
        [Header("Replay save elements")]
        [SerializeField] private List<TMP_Text> _saves;
        [SerializeField] private GameObject _filesMenu;

        private SideColor _pawnColor = SideColor.None;

        private void Start()
        {
            foreach (AudioSource sound in _sounds)
            {
                sound.volume = _settings.SoundLevels;
            }
            _volumeSlider.value = _settings.SoundLevels;

            for (int i = 0; i < _saves.Count; i++)
            {
                if (File.Exists(Application.persistentDataPath + "/save" + (i + 1) + ".json") == false)
                {
                    _saves[i].SetText("Empty");
                }
            }

            _queenSelectionButton.onClick.AddListener(() =>
            {
                if (_pawnColor == SideColor.White) PieceSelected(ChessPieceType.WhiteQueen);
                else PieceSelected(ChessPieceType.BlackQueen);
            });
            _rookSelectionButton.onClick.AddListener(() =>
            {
                if (_pawnColor == SideColor.White) PieceSelected(ChessPieceType.WhiteRook);
                else PieceSelected(ChessPieceType.BlackRook);
            });
            _bishopSelectionButton.onClick.AddListener(() =>
            {
                if (_pawnColor == SideColor.White) PieceSelected(ChessPieceType.WhiteBishop);
                else PieceSelected(ChessPieceType.BlackBishop);
            });
            _knightSelectionButton.onClick.AddListener(() =>
            {
                if (_pawnColor == SideColor.White) PieceSelected(ChessPieceType.WhiteKnight);
                else PieceSelected(ChessPieceType.BlackKnight);
            });
            _pauseButton.onClick.AddListener(Pause);
            _playAgainEndGameButton.onClick.AddListener(PlayAgain);
            _saveGameEndGameButton.onClick.AddListener(SaveGame);
            _settingsEndGameButton.onClick.AddListener(Settings);
            _mainMenuEndGameButton.onClick.AddListener(MainMenu);
            _quitEndGameButton.onClick.AddListener(Quit);
            _resumePauseButton.onClick.AddListener(Resume);
            _settingsPauseButton.onClick.AddListener(Settings);
            _mainMenuPauseButton.onClick.AddListener(MainMenu);
            _quitPauseButton.onClick.AddListener(Quit);
            _returnSettingsButton.onClick.AddListener(ReturnFromSettings);
            _volumeSlider.onValueChanged.AddListener(value => VolumeChanged());
            _file1SaveButton.onClick.AddListener(() => Save(0));
            _file2SaveButton.onClick.AddListener(() => Save(1));
            _file3SaveButton.onClick.AddListener(() => Save(2));
            _file4SaveButton.onClick.AddListener(() => Save(3));
            _returnSaveButton.onClick.AddListener(ReturnFromSave);
        }

        public void Pause()
        {
            _pauseMenu.SetActive(true);
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

        public void ReturnFromSave()
        {
            _filesMenu.SetActive(false);
            _gameOverMenu.SetActive(true);
        }

        public void VolumeChanged()
        {
            foreach(AudioSource sound in _sounds)
            {
                sound.volume = _volumeSlider.value;
            }
            _settings.SoundLevels = _volumeSlider.value;
        }

        public void PlayAgain()
        {
            _pauseButton.gameObject.SetActive(true);
            _gameOverMenu.SetActive(false);
            GameManager.Instance.Restart();
        }

        public void GameOver(SideColor winner)
        {
            _pauseButton.gameObject.SetActive(false);
            _gameOverMenu.SetActive(true);
            if(winner == SideColor.Both)
            {
                _winnerText.SetText("DRAW");
            }
            else
            {
                _winnerText.SetText(winner+" WINS");
            }
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

        public void PawnPromotionMenu(SideColor color)
        {
            _pawnColor = color;
            _pawnPromotionMenu.SetActive(true);
        }

        public void PieceSelected(ChessPieceType pieceIndex)
        {
            _pawnColor = SideColor.None;
            _pawnPromotionMenu.SetActive(false);

            switch (pieceIndex)
            {
                case ChessPieceType.BlackQueen:
                    GameManager.Instance.SelectedPromotion(Instantiate(_blackQueen), pieceIndex);
                    break;
                case ChessPieceType.WhiteQueen:
                    GameManager.Instance.SelectedPromotion(Instantiate(_whiteQueen), pieceIndex);
                    break;
                case ChessPieceType.BlackRook:
                    GameManager.Instance.SelectedPromotion(Instantiate(_blackRook), pieceIndex);
                    break;
                case ChessPieceType.WhiteRook:
                    GameManager.Instance.SelectedPromotion(Instantiate(_whiteRook), pieceIndex);
                    break;
                case ChessPieceType.BlackBishop:
                    GameManager.Instance.SelectedPromotion(Instantiate(_blackBishop), pieceIndex);
                    break;
                case ChessPieceType.WhiteBishop:
                    GameManager.Instance.SelectedPromotion(Instantiate(_whiteBishop), pieceIndex);
                    break;
                case ChessPieceType.BlackKnight:
                    GameManager.Instance.SelectedPromotion(Instantiate(_blackKnight), pieceIndex);
                    break;
                case ChessPieceType.WhiteKnight:
                    GameManager.Instance.SelectedPromotion(Instantiate(_whiteKnight), pieceIndex);
                    break;
            }
        }

        public void SaveGame()
        {
            _filesMenu.SetActive(true);
            _gameOverMenu.SetActive(false);
        }

        public void Save(int fileIndex)
        {
            MoveTracker.Instance.SaveGame(fileIndex);
            _filesMenu.SetActive(false);
            _gameOverMenu.SetActive(true);
            _saves[0].SetText($"SAVE {fileIndex}");
        }
    }
}
