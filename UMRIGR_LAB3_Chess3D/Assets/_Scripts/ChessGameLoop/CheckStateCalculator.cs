namespace ChessMainLoop
{
    public static class CheckStateCalculator
    {
        public static SideColor CalculateCheck(Piece[,] grid)
        {
            bool whiteCheck = false;
            bool blackCheck = false;
            int gridSize = grid.GetLength(0);

            for (int i = 0; i < gridSize; i++)
            {
                for(int j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] == null)
                    {
                        continue;
                    }

                    if (grid[i, j].IsAttackingKing(i, j))
                    {
                        if (grid[i, j].PieceColor == SideColor.Black)
                        {
                            whiteCheck = true;
                        }
                        else
                        {
                            blackCheck = true;
                        }
                    }
                }            
            }

            return whiteCheck ? blackCheck ? SideColor.Both : SideColor.White : blackCheck ? SideColor.Black : SideColor.None;
        }

        #region Direction Lookup Tables
        private static readonly int[,] DiagonalLookup =
        {
           { 1, 1 },
           { 1, -1 },
           { -1, 1 },
           { -1, -1 }
        };

        private static readonly int[,] VerticalLookup =
        {
           { 1, 0 },
           { -1, 0 },
           { 0, 1 },
           { 0, -1 }
        };
        #endregion

        public static bool IsAttackingKingDiagonal(int row, int column, SideColor attackerColor)
        {
            return IsAttackingKingInDirection(row, column, DiagonalLookup, attackerColor);
        }

        public static bool IsAttackingKingVertical(int row, int column, SideColor attackerColor)
        {
            return IsAttackingKingInDirection(row, column, VerticalLookup, attackerColor);
        }

        private static bool IsAttackingKingInDirection(int row, int column, int[,] directionLookupTable, SideColor attackerColor)
        {
            /*
             * Potrebno je zamijeniti liniju return false; logikom za provjeru napadala li figura sa danog polja koordinatama row i column
             * neprijateljskog kralja ovisnom o danom smjeru napada figure koji je definiran directionLookupTable parametrom.
             */
            int rowDirection, columnDirection;
            for (int i = 0; i < directionLookupTable.GetLength(0); i++)
            {
                rowDirection = directionLookupTable[i, 0];
                columnDirection = directionLookupTable[i, 1];

                int currentRow = row + rowDirection;
                int currentColumn = column + columnDirection;

                while (BoardState.Instance.IsInBorders(currentRow, currentColumn))
                {
                    Piece piece = BoardState.Instance.GetField(currentRow, currentColumn);

                    if (piece != null)
                    {
                        if (piece is King && piece.PieceColor != attackerColor)
                        {
                            return true;
                        }
                        else
                        {
                            break;
                        }
                    }

                    currentRow += rowDirection;
                    currentColumn += columnDirection;
                }
            }
            
            return false;
        }

        public static bool IsEnemyKingAtLocation(int row, int column, int rowDirection, int columnDirection, SideColor attackerColor)
        {

            if (BoardState.Instance.IsInBorders(row + rowDirection, column + columnDirection))
            {
                Piece piece = BoardState.Instance.GetField(row + rowDirection, column + columnDirection);

                if (piece == null) return false;
                if (piece is King && piece.PieceColor != attackerColor) return true;
            }

            return false;
        }
    }
}