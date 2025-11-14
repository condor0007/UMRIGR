using UnityEngine;

namespace ChessMainLoop
{
    public delegate void Selected(Piece self);
    /// <summary>
    /// Represents side colors of players.
    /// </summary>
    public enum SideColor
    {
        White,
        Black,
        None,
        Both
    }

    public abstract class Piece : MonoBehaviour
    {
        [SerializeField] private SideColor _pieceColor;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Animator _animator;
        [SerializeField] protected int _row;
        [SerializeField] protected int _column;
        private (int Row, int Column) _startLocation;
        private Vector3 _starPosition;
        private bool _isActive = false;
        private Color _startColor;
        private bool _hasMoved = false;
        private PathPiece _assignedAsEnemy = null;
        private PathPiece _assignedAsCastle = null;
        private Pawn _wasPawn = null;

        public SideColor PieceColor  => _pieceColor; 
        public Animator Animator => _animator;
        public (int Row, int Column) Location => (_row, _column);
        public bool IsActive { get => _isActive; set { _isActive = false; _renderer.material.color = _startColor; } }
        public bool HasMoved { get => _hasMoved; set => _hasMoved = value; }
        public PathPiece AssignedAsEnemy { get => _assignedAsEnemy; set => _assignedAsEnemy = value; }
        public PathPiece AssignedAsCastle { get => _assignedAsCastle; set { _assignedAsCastle = value; _renderer.material.color = _startColor; } }
        public Pawn WasPawn { get => _wasPawn; set => _wasPawn = value; }

        public static event Selected Selected;

        public abstract void CreatePath();
        public abstract bool IsAttackingKing(int _xPosition, int _yPosition);
        public abstract bool CanMove(int _xPosition, int _yPosition);

        private void Start()
        {
            _animator.runtimeAnimatorController = AnimationManager.Instance.Assign(this);
            _startLocation = (_row, _column);
            _starPosition = transform.position;
            _startColor = _renderer.material.color;
        }

        //If its turn players piece sets it as selected and sets path pieces for it. If the piece is target of enemy or castle calls select method of path object this piece is assing to.
        private void OnMouseDown()
        {
            if (_isActive == false && GameManager.Instance.TurnPlayer == _pieceColor && _assignedAsCastle == false && GameManager.Instance.IsPieceMoving == false) 
            {
                _isActive = true;
                Selected?.Invoke(this);
                CreatePath();
                _renderer.material.color = Color.yellow;
            }
            else if (_assignedAsEnemy)
            {
                _assignedAsEnemy.Selected();
            }
            else if (_assignedAsCastle)
            {
                _assignedAsCastle.Selected();
            }
        }

        private void OnMouseEnter()
        {
            if (PieceController.Instance.AnyActive == false && GameManager.Instance.TurnPlayer == _pieceColor)
            {
                _renderer.material.color = Color.yellow;
            }
            else if (_assignedAsEnemy)
            {
                _renderer.material.color = Color.red;
            }
            else if (_assignedAsCastle)
            {
                _renderer.material.color = Color.yellow;
            }
        }

        private void OnMouseExit()
        {
            if ((_isActive == false) || _assignedAsEnemy || _assignedAsCastle)
            {
                _renderer.material.color = _startColor;
            }

        }

        public void Die()
        {
            /*
             * Nadopunite kod sa logikom koja se treba izvršiti nakon što je figura pojedena. Potrebno je maknuti Figuru iz logičke 
             * matrice ploče pozivom pripadne metode klase BoardState, te je također Figuru potrebno dodati u object pool figura.
             */
            // print($"{this.GetType().Name} at {_row},{_column} has been captured.");
            
            if(AssignedAsEnemy != null)
            {
                AssignedAsEnemy.AssignedPiece = null;
                AssignedAsEnemy = null;
            }
            
            BoardState.Instance.ClearField(_row, _column);
            ObjectPool.Instance.AddPiece(this);
            print("death");
            
        }

        public void ResetPiece()
        {
            transform.position = _starPosition;
            _row = _startLocation.Row;
            _column = _startLocation.Column;
            _renderer.material.color = _startColor;
            _wasPawn = null;
            _hasMoved = false;
        }

        public virtual void Move(int newRow, int newColumn)
        {
            MoveTracker.Instance.AddMove(_row, _column, newRow, newColumn, GameManager.Instance.TurnCount);

            if(this is Pawn && GameManager.Instance.Passantable)
            {
                int direction = PieceColor == SideColor.Black ? 1 : -1;

                if (newColumn == GameManager.Instance.Passantable.Location.Column 
                    && _row == GameManager.Instance.Passantable.Location.Row && _column != newColumn)
                {
                    MoveTracker.Instance.AddMove(newRow - direction, newColumn, -1, -1, GameManager.Instance.TurnCount);
                }
            }

            BoardState.Instance.SetField(this, newRow, newColumn);
            _row = newRow;
            _column = newColumn;

            _hasMoved = true;
            GameManager.Instance.Passantable = null;
        }

        public void PiecePromoted(Pawn promotingPawn)
        {
            WasPawn = promotingPawn;
            HasMoved = true;
            _row = promotingPawn.Location.Row;
            _column = promotingPawn.Location.Column;
            transform.localPosition = promotingPawn.transform.localPosition;
            transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        }
    }
}
