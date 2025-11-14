using UnityEngine;

namespace ChessMainLoop
{
    public delegate void PathSelect(PathPiece piece);

    public class PathPiece : MonoBehaviour
    {
        [SerializeField] private PathPieceType _pathPieceType;
        private int _row;
        private int _column;
        private Color _startColor;
        private Renderer _renderer;
        private Piece _assignedPiece = null;
        private Piece _assignedCastle = null;

        public PathPieceType PathPieceType { get => _pathPieceType; }
        public Piece AssignedPiece { get => _assignedPiece; set => _assignedPiece = value; }
        public Piece AssignedCastle { get => _assignedCastle; set => _assignedCastle = value; }
        public (int Row, int Column) Location
        {
            get => (_row, _column);
            set
            {
                _row = value.Row;
                _column = value.Column;
            }
        }

        public static event PathSelect PathSelect;

        void OnEnable()
        {
            PieceController.PieceMoved += Disable;
        }
    
        void OnDisable()
        {
            PieceController.PieceMoved -= Disable;
        }

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            _startColor = _renderer.material.color;
        }

        private void OnMouseEnter()
        {
            _renderer.material.color = Color.white;
        }

        private void OnMouseExit()
        {
            _renderer.material.color = _startColor;
        }

        private void OnMouseDown()
        {
            Selected();
        }

        /// <summary>
        /// Disables the path gameobject. Also resets all refrences pieces have to it.
        /// </summary>
        private void Disable()
        {
            if (_assignedPiece != null)
            {
                _assignedPiece.AssignedAsEnemy = null;
                _assignedPiece = null;
            }
            else if (_assignedCastle != null)
            {
                _assignedCastle.AssignedAsCastle = null;
                _assignedCastle = null;
            }
            ObjectPool.Instance.RemoveHighlightPath(this);
        }

        public void AssignPiece(Piece piece)
        {
            _assignedPiece = piece;
            _assignedPiece.AssignedAsEnemy = this;
            print($"Assigned piece {piece.name} to path {_row}, {_column}");
        }

        public void AssignCastle(Piece piece)
        {
            _assignedCastle = piece;
            piece.AssignedAsCastle = this;
        }

        /// <summary>
        /// Sets the path as selected target for piece movement.
        /// </summary>
        public void Selected()
        {
            _renderer.material.color = _startColor;
            PathSelect?.Invoke(this);
        }
    }
}
