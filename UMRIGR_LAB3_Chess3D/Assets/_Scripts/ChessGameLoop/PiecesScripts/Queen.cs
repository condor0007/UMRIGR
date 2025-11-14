using System.IO;

namespace ChessMainLoop
{
    public class Queen : Piece
    {    
        public override void CreatePath()
        {
            /*
             * Nadopunite kod za stvaranje objekata za odabir polja koji prati logiku figure kraljice.
             */
            PathManager.CreateVerticalPath(this);
            PathManager.CreateDiagonalPath(this);
        }

        public override bool IsAttackingKing(int row, int column)
        {
            /*
             * Zamijenite liniju return false; sa kodom za provjeru napada li kraljica kralja sa trenutnog polja.
            */
            if (CheckStateCalculator.IsAttackingKingVertical(row, column, PieceColor))
            {
                return true;
            }
            else if (CheckStateCalculator.IsAttackingKingDiagonal(row, column, PieceColor))
            {
                return true;
            }
            return false;
        }

        public override bool CanMove(int row, int column)
        {
            /*
             * Zamijenite liniju return false; sa kodom za provjeru ima li kraljica preostalih dopu�tenih poteza.
            */
            
            return false;
        }
    }
}