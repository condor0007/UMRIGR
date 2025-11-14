using System.Collections;
using UnityEngine;

namespace ChessMainLoop
{
    public class AnimationManager : Singleton<AnimationManager>
    {
        #region Private animator prefabs
        [SerializeField]
        private RuntimeAnimatorController _whiteAnimator;
        [SerializeField]
        private RuntimeAnimatorController _blackAnimator;
        [SerializeField]
        private RuntimeAnimatorController _whiteKnightAnimator;
        [SerializeField]
        private RuntimeAnimatorController _blackKnightAnimator;
        [SerializeField]
        private RuntimeAnimatorController _whiteBishopAnimator;
        [SerializeField]
        private RuntimeAnimatorController _blackBishopAnimator;
        #endregion

        [SerializeField] private float _moveSpeed = 20f;
        [SerializeField] private AudioSource _moveSound;
        private bool _isActive = false;
        public bool IsActive { get => _isActive; }

        /// <summary>
        /// Assigns animator controller to piece based on piece type.
        /// </summary>
        /// <returns>Animator controler for piece parameter</returns>
        public RuntimeAnimatorController Assign(Piece piece)
        {
            if (piece.PieceColor == SideColor.White)
            {
                if(piece is Knight || piece is King)
                {
                    return _whiteKnightAnimator;
                }

                if(piece is Bishop)
                {
                    return _whiteBishopAnimator;
                }

                return _whiteAnimator;
            }
            else
            {
                if (piece is Knight || piece is King)
                {
                    return _blackKnightAnimator;
                }

                if (piece is Bishop)
                {
                    return _blackBishopAnimator;
                }

                return _blackAnimator;
            }
        }

        public void MovePiece(Piece piece, Vector3 target, Piece killTarget)
        {
            _isActive = true;
            StartCoroutine(MoveAnimation(piece, target, killTarget));
        }

        /// <summary>
        /// Moves the piece to target location with root motion animations and translation.
        /// </summary>
        private IEnumerator MoveAnimation(Piece piece, Vector3 target, Piece killTarget)
        {
            Animator pieceAnimator = piece.Animator;

            //Performs animation to raise the piece and tilt it
            pieceAnimator.SetInteger("State", 1);
            while (pieceAnimator.GetCurrentAnimatorStateInfo(0).IsName("Travel") == false)
            {
                yield return new WaitForSeconds(0.001f);
            }

            //performs translation to target position
            target.y = piece.transform.localPosition.y;
            while (piece.transform.localPosition != target)
            {
                piece.transform.localPosition = Vector3.MoveTowards(piece.transform.localPosition, target, _moveSpeed * (Time.deltaTime));
                yield return new WaitForSeconds(0.001f);
            }

            _moveSound.Play();

            //Perfoms root motion animation that puts piece back down
            pieceAnimator.SetInteger("State", 2);
            if (killTarget != null)
            {
                killTarget.Die();
            }

            while (pieceAnimator.GetCurrentAnimatorStateInfo(0).IsName("StartState") == false)
            {
                yield return new WaitForSeconds(0.001f);
            }

            target.y = piece.transform.localPosition.y;
            piece.transform.localPosition = target;
            _isActive = false;
        }
    }
}
