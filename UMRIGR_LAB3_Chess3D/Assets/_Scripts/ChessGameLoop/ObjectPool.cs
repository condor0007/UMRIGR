using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ChessMainLoop
{
    /// <summary>
    /// Contains pools for objects and methods to put objects and recieve objects to and from pools.
    /// </summary>
    public  class ObjectPool : Singleton<ObjectPool>
    {
        [SerializeField] private List<PathPiece> _prefabs;
        private Dictionary<PathPieceType, Queue<GameObject>> _poolDictionary;
        private Queue<Piece> _pieces;

        private void Start()
        {
            _poolDictionary = new Dictionary<PathPieceType, Queue<GameObject>>();
            _pieces = new Queue<Piece>();

            Queue<GameObject> queue;

            foreach (PathPiece prefab in _prefabs)
            {
                queue = new Queue<GameObject>();
                _poolDictionary.Add(prefab.PathPieceType, queue);
            }
        }

        /// <summary>
        /// Returns number of path objects indexed by name equal to quantity parameter. Gets objects from pool or instantiates new ones if quantity in pool isnt enough.
        /// </summary>
        /// <returns>List of path objects quantity long</returns>
        public GameObject[] GetHighlightPaths(int quantity, PathPieceType pathPieceType)
        {
            GameObject[] paths = new GameObject[quantity];

            for(int i = 0; i < quantity; i++)
            {
                if (_poolDictionary[pathPieceType].Count > 0)
                {
                    paths[i] = _poolDictionary[pathPieceType].Dequeue();
                    paths[i].SetActive(true);
                }
                else
                {
                    paths[i]= Instantiate(_prefabs.Where(piece => piece.PathPieceType == pathPieceType)
                        .SingleOrDefault().gameObject, transform.parent);
                }
            }

            return paths;
        }

        /// <summary>
        /// Returns a singular path object indexed by type
        /// </summary>
        /// <returns>Path object of type</returns>
        public GameObject GetHighlightPath(PathPieceType pathPieceType)
        {
            /*
             * Potrebno je zamijeniti liniju return null; logikom koja dohvaća HighlightPath objekt definiran parametrom tipa, 
             * ili ako ih nema više dostupnih stvara novi.
             */
            GameObject path = null;
            if (_poolDictionary[pathPieceType].Count > 0)
            {
                path = _poolDictionary[pathPieceType].Dequeue();
                path.SetActive(true);
            }
            else
            {
                path = Instantiate(_prefabs.Where(piece => piece.PathPieceType == pathPieceType)
                    .SingleOrDefault().gameObject, transform.parent);
                
            }
            return path;
        }

        /// <summary>
        /// Disables a path object and puts it back into pool
        /// </summary>
        public void RemoveHighlightPath(PathPiece path)
        {
            /*
             * Potrebno je nadopuniti metodu logikom koja onesposobljuje objekt za odabir polja, te sprema referencu na njegov game object.
             */
            path.gameObject.SetActive(false);
            _poolDictionary[path.PathPieceType].Enqueue(path.gameObject);
        }

        public void AddPiece(Piece piece)
        {
            _pieces.Enqueue(piece);
            piece.gameObject.SetActive(false);
            print($"piece {piece.name} added to pool");
            

        }

        public void ResetPieces()
        {
            while (_pieces.Count > 0)
            {
                _pieces.Dequeue().gameObject.SetActive(true);
            }
        }
    }
}
