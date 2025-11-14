using System.Collections.Generic;
using UnityEngine;

namespace ChessMainLoop
{
    public class BoardState : Singleton<BoardState>
    {
        [SerializeField] private int _boardSize;
        [SerializeField] private List<Piece> _blackPieces;
        [SerializeField] private List<Piece> _whitePieces;
        [SerializeField] private Queue<Piece> _promotedPieces;
        private Piece[,] _gridState;

        public int BoardSize { get => _boardSize; }
        public static float Offset = 1.5f;


        private void Start()
        {
            _gridState = new Piece[_boardSize, _boardSize];
            InitializeGrid();
            _promotedPieces = new Queue<Piece>();
        }

        public void InitializeGrid()
        {
            for (int i = 0; i < _boardSize; i++)
            {
                for (int j = 0; j < _boardSize; j++)
                {
                    _gridState[i, j] = null;
                }
            }

            Vector3 position = new Vector3();

            for (int i = 0; i < _blackPieces.Count; i++)
            {
                Piece piece = _blackPieces[i];
                var location = piece.Location;
                _gridState[location.Row, location.Column] = piece;
                position.x = piece.Location.Row;
                position.z = piece.Location.Column;
                position *= Offset;
                position.y = piece.transform.localPosition.y;
                piece.transform.localPosition = position;
            }

            for (int i = 0; i < _whitePieces.Count; i++)
            {
                Piece piece = _whitePieces[i];
                var location = piece.Location;
                _gridState[location.Row, location.Column] = piece;
                position.x = piece.Location.Row;
                position.z = piece.Location.Column;
                position *= Offset;
                position.y = piece.transform.localPosition.y;
                piece.transform.localPosition = position;
            }
        }

        /// <summary>
        /// Retrieves current state of that fied on board
        /// </summary>
        /// <returns>Piece reference of the piece on the field, or null if not occupied</returns>
        public Piece GetField(int row, int column) => _gridState[row, column];

        public void SetField(Piece piece, int newRow, int newColumn)
        {
            if (piece == null)
            {
                _gridState[newRow, newColumn] = null;

            }
            else
            {
                _gridState[piece.Location.Row, piece.Location.Column] = null;
                _gridState[newRow, newColumn] = piece;
            }
        }

        public void ClearField(int row, int column)
        {
            _gridState[row, column] = null;
        }

        /// <summary>
        /// Checks if cooridantes are inside board borders
        /// </summary>
        public bool IsInBorders(int row, int column)
        {
            bool check = (row >= 0 && row < _boardSize && column >= 0 && column < _boardSize);
            return check;
        }

        /// <summary>
        /// Mocks the translation of the piece to the target position and check if it would result in check.
        /// </summary>
        /// <returns>Weather translation performed on the piece would result in a check state</returns>
        public SideColor SimulateCheckState(int rowOld, int columnOld, int rowNew, int columnNew)
        {
            /*
             * Potrebno je zamijeniti liniju return SideColor.None logikom koja provjerava čijim stanjem šaha bi 
             * završilo stanje ploče prilikom izvođenja tog poteza.
             */
            Piece movingPiece = _gridState[rowOld, columnOld];
            Piece targetPiece = _gridState[rowNew, columnNew];
            _gridState[rowNew, columnNew] = movingPiece;
            _gridState[rowOld, columnOld] = null;
            SideColor checkState = CheckStateCalculator.CalculateCheck(_gridState);
            _gridState[rowOld, columnOld] = movingPiece;
            _gridState[rowNew, columnNew] = targetPiece;
            return checkState;
        }

        public SideColor CheckIfGameOver()
        {
            return GameEndCalculator.CheckIfGameEnd(_gridState);
        }

        public void ResetPieces()
        {
            foreach (Piece piece in _blackPieces)
            {
                piece.ResetPiece();
            }
            foreach (Piece piece in _whitePieces)
            {
                piece.ResetPiece();
            }

            while (_promotedPieces.Count > 0)
            {
                Destroy(_promotedPieces.Dequeue());
            }

            InitializeGrid();
        }

        /// <summary>
        /// Replaces pawn being promoted with the selected piece.
        /// </summary>
        public void PromotePawn(Pawn promotingPawn, Piece piece, ChessPieceType pieceIndex)
        {
            MoveTracker.Instance.AddMove(promotingPawn.Location.Row, promotingPawn.Location.Column,
                (int)pieceIndex, (int)pieceIndex, GameManager.Instance.TurnCount - 1);
            _gridState[promotingPawn.Location.Row, promotingPawn.Location.Column] = piece;
            piece.PiecePromoted(promotingPawn);
            promotingPawn.gameObject.SetActive(false);
        }
    }
}
