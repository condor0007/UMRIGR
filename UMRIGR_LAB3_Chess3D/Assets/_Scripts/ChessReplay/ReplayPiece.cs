using UnityEngine;

namespace ChessReplay
{
    public class ReplayPiece : MonoBehaviour
    {
        [SerializeField] protected int _row;
        [SerializeField] protected int _column;

        public (int Row, int Column) Location => (_row, _column);
    }
}
