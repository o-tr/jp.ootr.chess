using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.chess
{
    public class ChessObjectPool : ChessCore
    {
        [SerializeField] protected GameObject blackKing;
        [SerializeField] protected GameObject whiteKing;
        [SerializeField] protected GameObject[] blackQueens;
        [SerializeField] protected GameObject[] whiteQueens;
        [SerializeField] protected GameObject[] blackRooks;
        [SerializeField] protected GameObject[] whiteRooks;
        [SerializeField] protected GameObject[] blackBishops;
        [SerializeField] protected GameObject[] whiteBishops;
        [SerializeField] protected GameObject[] blackKnights;
        [SerializeField] protected GameObject[] whiteKnights;
        [SerializeField] protected GameObject[] blackPawns;
        [SerializeField] protected GameObject[] whitePawns;
        [SerializeField] protected Transform[] cells;
        [SerializeField] protected Toggle[] toggles;
        
        [SerializeField] protected TextMeshProUGUI statusText;
        
        private int selectedIndex = -1;

        public override MoveResult MakeMove(string fromPos, string toPos, PromotionPiece promotionPiece)
        {
            var result = base.MakeMove(fromPos, toPos, promotionPiece);
            UpdateBoardCells();
            return result;
        }
        
        public void OnToggleSelected()
        {
            int index = GetSelectedToggleIndex();
            Debug.Log($"Toggle selected: {index}, selectedIndex: {selectedIndex}");
            if (index < 0 || index >= toggles.Length)
            {
                Debug.LogError("Invalid toggle index selected.");
                return;
            }

            if (selectedIndex < 0)
            {
                OnCellSelected(index);
                return;
            }
            if (selectedIndex == index)
            {
                ResetToggles();
                selectedIndex = -1;
                UpdateBoardSelection();
                return;
            }
            OnPieceMoved(index);
        }
        
        protected void OnCellSelected(int index)
        {
            if (index < 0 || index >= cells.Length)
            {
                Debug.LogError("Invalid cell index selected.");
                return;
            }

            ResetToggles();
            selectedIndex = index;
            UpdateBoardSelection();
        }

        protected void OnPieceMoved(int toIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= cells.Length || toIndex < 0 || toIndex >= cells.Length)
            {
                Debug.LogError("Invalid cell index for piece move.");
                return;
            }

            var fromPos = Position.New(selectedIndex / 8, selectedIndex % 8);
            var toPos = Position.New(toIndex / 8, toIndex % 8);
            ResetToggles();
            selectedIndex = -1;

            var result = MakeMove(fromPos.AsString(), toPos.AsString(), PromotionPiece.Queen);
            if (!result.Success())
            {
                Debug.LogError($"Move failed: {result.Message()}");
            }
            
            UpdateBoardCells();
            UpdateBoardSelection();
            if (gameOver)
            {
                Debug.Log($"Game over! Winner: {winner}");
            }
        }

        protected void UpdateBoardCells()
        {
            ResetPieces();
            var board = _board.GetBoardState();
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    var piece = board[r][c];
                    var index = r * 8 + c;
                    var cell = cells[index];
                    GameObject pieceObject = null;

                    switch (piece)
                    {
                        case Piece.WhiteKing:
                            pieceObject = whiteKing;
                            break;
                        case Piece.BlackKing:
                            pieceObject = blackKing;
                            break;
                        case Piece.WhiteQueen:
                            pieceObject = GetAvailablePiece(whiteQueens);
                            break;
                        case Piece.BlackQueen:
                            pieceObject = GetAvailablePiece(blackQueens);
                            break;
                        case Piece.WhiteRook:
                            pieceObject = GetAvailablePiece(whiteRooks);
                            break;
                        case Piece.BlackRook:
                            pieceObject = GetAvailablePiece(blackRooks);
                            break;
                        case Piece.WhiteBishop:
                            pieceObject = GetAvailablePiece(whiteBishops);
                            break;
                        case Piece.BlackBishop:
                            pieceObject = GetAvailablePiece(blackBishops);
                            break;
                        case Piece.WhiteKnight:
                            pieceObject = GetAvailablePiece(whiteKnights);
                            break;
                        case Piece.BlackKnight:
                            pieceObject = GetAvailablePiece(blackKnights);
                            break;
                        case Piece.WhitePawn:
                            pieceObject = GetAvailablePiece(whitePawns);
                            break;
                        case Piece.BlackPawn:
                            pieceObject = GetAvailablePiece(blackPawns);
                            break;
                        case Piece.None:
                            continue;
                    }
                    
                    if (pieceObject == null)
                    {
                        Debug.LogError($"No available piece for {piece} at ({r}, {c})");
                        continue;
                    }
                    pieceObject.transform.position = cell.position;
                    pieceObject.transform.rotation = cell.rotation;
                    pieceObject.transform.localScale = cell.localScale;
                    pieceObject.SetActive(true);
                }
            }
        }
        
        protected void UpdateBoardSelection()
        {
            ResetCells();
            ResetToggles();
            if (selectedIndex < 0)
            {
                var movable1 = GetMovablePieces();
                foreach (var pos in movable1)
                {
                    cells[pos].gameObject.SetActive(true);
                }

                return;
            }
            var selectedRow = selectedIndex / 8;
            var selectedCol = selectedIndex % 8;
            var movable2 = GetValidMovesForPiece(selectedRow, selectedCol);
            foreach (var pos in movable2)
            {
                cells[pos].gameObject.SetActive(true);
            }
            cells[selectedIndex].gameObject.SetActive(true);
        }

        protected void UpdateStatusText()
        {
            if (_stalemate)
            {
                statusText.text = "Game Over! Stalemate!";
                return;
            }
            if (_checkmate)
            {
                var winnerText = winner == Player.White ? "White" : "Black";
                statusText.text = $"Game Over! Winner: {winnerText}";
                return;
            }
            
            var currentPlayerText = currentPlayer == Player.White ? "White" : "Black";
            var checkText = _check ? " (Check)" : "";
            statusText.text = $"{currentPlayerText} Turn{checkText}";
        }

        private void ResetPieces()
        {
            foreach (var piece in blackQueens) piece.SetActive(false);
            foreach (var piece in whiteQueens) piece.SetActive(false);
            foreach (var piece in blackRooks) piece.SetActive(false);
            foreach (var piece in whiteRooks) piece.SetActive(false);
            foreach (var piece in blackBishops) piece.SetActive(false);
            foreach (var piece in whiteBishops) piece.SetActive(false);
            foreach (var piece in blackKnights) piece.SetActive(false);
            foreach (var piece in whiteKnights) piece.SetActive(false);
            foreach (var piece in blackPawns) piece.SetActive(false);
            foreach (var piece in whitePawns) piece.SetActive(false);
            blackKing.SetActive(false);
            whiteKing.SetActive(false);
        }
        
        private void ResetCells()
        {
            foreach (var cell in cells) cell.gameObject.SetActive(false);
        }
        
        private GameObject GetAvailablePiece(GameObject[] pool)
        {
            foreach (var piece in pool)
            {
                if (!piece.activeInHierarchy)
                {
                    piece.SetActive(true);
                    return piece;
                }
            }
            return null;
            
        }

        protected int GetSelectedToggleIndex()
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i].isOn)
                {
                    return i;
                }
            }

            return -1;
        }

        protected void ResetToggles()
        {
            foreach (var toggle in toggles)
            {
                toggle.isOn = false;
            }
        }
    }
}
