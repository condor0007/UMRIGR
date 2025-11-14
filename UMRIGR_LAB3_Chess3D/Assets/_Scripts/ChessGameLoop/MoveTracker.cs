using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ChessMainLoop
{
    public class MoveTracker : Singleton<MoveTracker>
    {
        private List<List<Vector2>> _moves;
        private static readonly string[] _files =
        {
            "/save1.json",
            "/save2.json",
            "/save3.json",
            "/save4.json"
        };

        private void Start()
        {
            _moves = new List<List<Vector2>>();
        }

        public void AddMove(int oldRow, int oldColumn, int newRow, int newColumn, int moveIndex)
        {
            if (_moves.Count <= moveIndex)
            {
                _moves.Add(new List<Vector2>());
            }

            _moves[moveIndex].Add(new Vector2(oldRow, oldColumn));
            _moves[moveIndex].Add(new Vector2(newRow, newColumn));
        }

        public List<List<Vector2>> Moves()
        {
            return _moves;
        }

        public void ResetMoves()
        {
            _moves = new List<List<Vector2>>();
        }

        public void SaveGame(int fileIndex)
        {
            string json = "";
            for (int i = 0; i < _moves.Count; i++) 
            {
                var myclass=new MovesSerializator(_moves[i], i);
                json =json + "\n" + JsonUtility.ToJson(myclass);            
            }

            File.WriteAllText(Application.persistentDataPath + _files[fileIndex], json);


        }

        /// <summary>
        /// Class for json serialization of moves.
        /// </summary>
        public class MovesSerializator
        {
            public List<Vector2> MoveList;
            public int TurnOrder;

            public MovesSerializator(List<Vector2> moveList, int turnOrder)
            {
                MoveList = moveList;
                TurnOrder = turnOrder;
            }
        }
    }
}
