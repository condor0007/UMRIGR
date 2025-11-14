using UnityEngine;

namespace ChessMainLoop
{
    public class Pawn : Piece
    {
        private static readonly int[,] LookupMovesBlack =
        {
           { 1, 0 },
           { 2, 0 },
           { 1, 1 },
           { 1, -1 }
        };
        private static readonly int[,] LookupMovesWhite =
        {
           { -1, 0 },
           { -2, 0 },
           { -1, 1 },
           { -1, -1 }
        };

        private static readonly int[,] attackMoves =
        {
           { 0, 1 },
           { 0, -1 }
        };
        private bool _hasntMoved = true;
        
        public override void CreatePath()
        {
            /*
             * Potrebno je nadponuti kod koji će stvoriti objekte za odabir polja za kretanje. Potrebno je po potrebi stvoriti 
             * polja za dijagonalni napad, En passant napad (https://en.wikipedia.org/wiki/En_passant), te polja za kretanje 
             * jedno i dva mjesta prema naprijed.
             */

            // print("Creating path for pawn at " + _row + ", " + _column + " with direction " + _direction);
            if (PieceColor == SideColor.Black)
            {
                if (BoardState.Instance.IsInBorders(_row + LookupMovesBlack[0, 0], _column + LookupMovesBlack[0, 1]) && BoardState.Instance.GetField(_row + LookupMovesBlack[0, 0], _column) == null)
                {
                    PathManager.CreatePathInSpotDirection(this, LookupMovesBlack[0, 0], LookupMovesBlack[0, 1]);
                    
                }
                if (_hasntMoved && BoardState.Instance.IsInBorders(_row, _column + LookupMovesBlack[1, 1]) && BoardState.Instance.GetField(_row + LookupMovesBlack[1, 0], _column) == null && BoardState.Instance.GetField(_row + LookupMovesBlack[0, 0], _column) == null)
                {
                    PathManager.CreatePathInSpotDirection(this, LookupMovesBlack[1, 0], LookupMovesBlack[1, 1]);

                }
                CreateAttackSpace(LookupMovesBlack[2, 0], LookupMovesBlack[2, 1]);
                CreateAttackSpace(LookupMovesBlack[3, 0], LookupMovesBlack[3, 1]);
            }
            else
            {
            
                if (BoardState.Instance.IsInBorders(_row + LookupMovesWhite[0, 0], _column + LookupMovesWhite[0, 1] ) && BoardState.Instance.GetField(_row + LookupMovesWhite[0, 0] , _column) == null)
                {
                    PathManager.CreatePathInSpotDirection(this, LookupMovesWhite[0, 0], LookupMovesWhite[0, 1] );
                    
                }
                if (_hasntMoved && BoardState.Instance.IsInBorders(_row, _column + LookupMovesWhite[1, 1]) && BoardState.Instance.GetField(_row + LookupMovesWhite[1, 0], _column) == null && BoardState.Instance.GetField(_row + LookupMovesWhite[0, 0], _column) == null)
                {
                    PathManager.CreatePathInSpotDirection(this, LookupMovesWhite[1, 0], LookupMovesWhite[1, 1]);

                }
                CreateAttackSpace(LookupMovesWhite[2, 0], LookupMovesWhite[2, 1]);
                CreateAttackSpace(LookupMovesWhite[3, 0], LookupMovesWhite[3, 1]);
            }


        }

        private void CreateAttackSpace(int rowDirection, int columnDirection)
        {
            if (!BoardState.Instance.IsInBorders(_row + rowDirection, _column + columnDirection) == true) return;
            Piece piece = BoardState.Instance.GetField(_row + rowDirection, _column + columnDirection);
            if (piece != null && piece.PieceColor != PieceColor)
            {
                PathManager.CreatePathInSpotDirection(this, rowDirection, columnDirection);
            }
        }

        private void CreatePassantSpace(int rowDirection, int columnDirection)
        {
            if (!BoardState.Instance.IsInBorders(_row, _column + columnDirection) == true) return;
            Piece piece = BoardState.Instance.GetField(_row, _column + columnDirection);
            if (piece != null && piece.PieceColor != PieceColor && piece == GameManager.Instance.Passantable)
            {
                PathManager.CreatePassantSpot(piece, _row + rowDirection, _column + columnDirection);
            }
        }

        /// <summary>
        /// Adds checks for making the piece passantable if it moved for two sapces and promoting the pawn if it reached the end of the board to Move method of base class.
        /// </summary>
        public override void Move(int newRow, int newColumn)
        {
            int oldRow = _row;
            _hasntMoved = false;

            base.Move(newRow, newColumn);

            if (Mathf.Abs(oldRow - newRow) == 2)
            {
                GameManager.Instance.Passantable = this;
            }

            if (newRow == 0 || newRow == BoardState.Instance.BoardSize - 1)
            {
                GameManager.Instance.PawnPromoting(this);
            }
        }

        public override bool IsAttackingKing(int row, int column)
        {
            int _direction = PieceColor == SideColor.Black ? 1 : -1;

            if (CheckStateCalculator.IsEnemyKingAtLocation(row, column, _direction, 1, PieceColor))
            {
                return true;
            }

            if (CheckStateCalculator.IsEnemyKingAtLocation(row, column, _direction, -1, PieceColor))
            {
                return true;
            }

            return false;
        }

        public override bool CanMove(int row, int column)
        {
            int _direction = PieceColor == SideColor.Black ? 1 : -1;

            //Following two sections perform checks if there are attackable units diagonally in looking direction of the pawn, and if moving to them would not resolve in a check for turn player
            if (BoardState.Instance.IsInBorders(row + _direction, column + 1))
            {
                Piece piece = BoardState.Instance.GetField(row + _direction, column + 1);
                if (piece != null && piece.PieceColor != PieceColor)
                {
                    if (GameEndCalculator.CanMoveToSpot(row, column, _direction, 1, PieceColor))
                    {
                        return true;
                    }
                }
            }

            if (BoardState.Instance.IsInBorders(row + _direction, column - 1))
            {
                Piece piece = BoardState.Instance.GetField(row + _direction, column - 1);
                if (piece != null && piece.PieceColor != PieceColor)
                {
                    if (GameEndCalculator.CanMoveToSpot(row, column, _direction, -1, PieceColor))
                    {
                        return true;
                    }
                }
            }

            //Following sections check if one in looking direction of the pawn is awailable for moving to
            if (!BoardState.Instance.IsInBorders(row + _direction, column)) return false;
            if (BoardState.Instance.GetField(row + _direction, column) != null) return false;

            if (GameEndCalculator.CanMoveToSpot(row, column, _direction, 0, PieceColor)) return true;

            return false;
        }
    }
}
