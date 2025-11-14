using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChessReplay
{ 
    public class ReplayController : MonoBehaviour
    {
        [SerializeField] private AudioSource _moveSound;
        [SerializeField] private float _turnSpeed = 1f;
        private List<List<Vector2>> _moveList;
        private int _turnCount;
        private IEnumerator _automaticTurns;
        private float _timeSinceLeft;
        private float _timeSinceRight;
        private IEnumerator _automaticTurnsCoroutine;

        public float TurnSpeed { get => _turnSpeed; set => _turnSpeed = value; }

        /// <summary>
        /// Loads moveset data from file selected by index parameter and starts autoplay.
        /// </summary>
        public void Initialize(int fileIndex)
        {
            _moveList = DataLoader.LoadData(fileIndex);
            _automaticTurns = AutomaticTurns();
            _turnCount = 0;
            StartAutoPlay();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    LastTurn();
                    _timeSinceLeft = Time.time;
                }
                else if (Time.time - _timeSinceLeft > _turnSpeed)
                {
                    LastTurn();
                    _timeSinceLeft = Time.time;
                }
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    NextTurn();
                    _timeSinceRight = Time.time;
                }
                else if (Time.time - _timeSinceRight > _turnSpeed)
                {
                    NextTurn();
                    _timeSinceRight = Time.time;
                }
            }
        }

        public void StartAutoPlay()
        {
            if (_automaticTurnsCoroutine == null)
            {
                _automaticTurnsCoroutine = _automaticTurns;
                StartCoroutine(_automaticTurnsCoroutine);
            }
        }

        /// <summary>
        /// Stops autoplay of turns and plays past turn if it exists.
        /// </summary>
        public void LastTurn()
        {
            if (_turnCount > 0)
            {
                /*
                 * Nadopuniti metodu logikom koja vraća unazad i izvodi prošli izvedeni potez.
                 */
                _turnCount--;
                List<Vector2> move = _moveList[_turnCount];
                Vector2 from = move[0];
                Vector2 to = move[1];
                BoardStateReplay.Instance.UndoMove(from, to, _turnCount);
            }
        }

        /// <summary>
        /// Stops autoplay of turns and plays folowing turn if it exists.
        /// </summary>
        public void NextTurn()
        {
            if (_turnCount < _moveList.Count)
            {
                /*
                 * Nadopuniti metodu logikom koja izvodi slijedeći spremljeni potez.
                 */
                List<Vector2> move = _moveList[_turnCount];
                Vector2 from = move[0];
                Vector2 to = move[1];
                BoardStateReplay.Instance.MovePiece(from, to, _turnCount);
                if (_moveSound != null)
                {
                    _moveSound.Play();
                }
                _turnCount++;

            }
        }

        private IEnumerator AutomaticTurns()
        {
            while (_turnCount < _moveList.Count)
            {
                NextTurn();

                yield return new WaitForSeconds(_turnSpeed);
            }
        }

        public void StopAutoplay()
        {
            if (_automaticTurnsCoroutine != null)
            {
                StopCoroutine(_automaticTurnsCoroutine);
                _automaticTurnsCoroutine = null;
            }
        }
    }
}