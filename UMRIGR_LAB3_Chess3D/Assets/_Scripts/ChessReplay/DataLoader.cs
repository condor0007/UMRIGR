using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ChessReplay
{
    public static class DataLoader
    {
        private static readonly string[] _files =
        {
            "/save1.json",
            "/save2.json",
            "/save3.json",
            "/save4.json"
        };

        /// <summary>
        /// Loads moveset data from file selected by index paramter
        /// </summary>
        /// <returns>Moveset data</returns>
        public static List<List<Vector2>> LoadData(int fileIndex)
        {
            List<List<Vector2>> moves = new List<List<Vector2>>();

            if (File.Exists(Application.persistentDataPath + _files[fileIndex]))
            {
                string[] json = File.ReadAllText(Application.persistentDataPath 
                    + _files[fileIndex]).Split("\n");
                MovesSerializator moveInstance;

                foreach (string turn in json)
                {
                    if (turn.Length == 0)
                    {
                        continue;
                    }
                    moveInstance = JsonUtility.FromJson<MovesSerializator>(turn);
                    moves.Add(moveInstance.MoveList);
                }
            }
        
            return moves;
        }

        /// <summary>
        /// Class for json data serialization of moves.
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
