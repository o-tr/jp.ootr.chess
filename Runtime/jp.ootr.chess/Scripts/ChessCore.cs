using System;
using System.Text;
using jp.ootr.common;
using UdonSharp;
using UnityEngine;

namespace jp.ootr.chess
{
    public class ChessCore : BaseClass
    {
        public Player currentPlayer;
        public bool gameOver;
        public Player winner;

        protected BoardData _board;
        private Position _enPassantTarget;
        private GameState _castlingRights;
        protected bool _check;
        protected bool _checkmate;
        protected bool _stalemate;

        public void Initialize()
        {
            _board = BoardData.New();
            currentPlayer = Player.White;
            gameOver = false;
            winner = Player.None; 

            _enPassantTarget = Position.New(-1,-1);
            _castlingRights = GameState.New(
                CastlingRights.New(true, true),
                CastlingRights.New(true, true)
            );
            _check = false;
            _checkmate = false;
            _stalemate = false;
        }
        private string GetPieceSymbol(Piece piece) // Changed from Piece?
        {
            switch (piece)
            {
                case Piece.WhiteKing:
                    return "♔";
                case Piece.WhiteQueen:
                    return "♕";
                case Piece.WhiteRook:
                    return "♖";
                case Piece.WhiteBishop:
                    return "♗";
                case Piece.WhiteKnight:
                    return "♘";
                case Piece.WhitePawn:
                    return "♙";
                case Piece.BlackKing:
                    return "♚";
                case Piece.BlackQueen:
                    return "♛";
                case Piece.BlackRook:
                    return "♜";
                case Piece.BlackBishop:
                    return "♝";
                case Piece.BlackKnight:
                    return "♞";
                case Piece.BlackPawn:
                    return "♟";
                case Piece.None:
                    return "。";
                default:
                    return "？";
            }
            
        }

        public void DisplayBoard()
        {
            var sb = new StringBuilder();
            sb.AppendLine("   a b c d e f g h");
            for (int row = 0; row < 8; row++)
            {
                var line = $"{8 - row} ";
                for (int col = 0; col < 8; col++)
                {
                    var piece = _board.GetPieceAt(row, col); // Changed
                    var symbol = GetPieceSymbol(piece);
                    line += $" {symbol}";
                }
                line += $" {8 - row}";
                sb.AppendLine(line);
            }
            sb.AppendLine("   a b c d e f g h");
            sb.AppendLine($"Current Player: {currentPlayer}");
            sb.AppendLine($"Game Over: {gameOver}");
            sb.AppendLine($"Winner: {winner}");
            sb.AppendLine($"Check: {_check}");
            sb.AppendLine($"Checkmate: {_checkmate}");
            sb.AppendLine($"Stalemate: {_stalemate}");
            sb.AppendLine($"En Passant Target: {(_enPassantTarget.Row() != -1 ? PositionToString(_enPassantTarget.Row(), _enPassantTarget.Col()) : "None")}");
            sb.AppendLine($"Castling Rights: White - Kingside: {_castlingRights.White().Kingside()}, Queenside: {_castlingRights.White().Queenside()} | Black - Kingside: {_castlingRights.Black().Kingside()}, Queenside: {_castlingRights.Black().Queenside()}");
            Debug.Log(sb.ToString());
        }

        private bool IsValidPosition(int row, int col)
        {
            return _board.IsValidPosition(row, col); // Changed
        }

        public Position ParsePosition(string pos)
        {
            if (pos.Length != 2) return Position.New(-1, -1); // Return invalid position
            int col = pos[0] - 'a';
            int row = 8 - (pos[1] - '0');
            return IsValidPosition(row, col) ? Position.New(row, col) : Position.New(-1, -1); // Return invalid position
        }

        public string PositionToString(int row, int col)
        {
            return $"{(char)('a' + col)}{8 - row}";
        }

        public bool IsWhitePiece(Piece piece) // Changed from Piece?
        {
            if (piece == Piece.None) return false;
            return (int)piece >= 'A' && (int)piece <= 'Z';
        }

        public bool IsBlackPiece(Piece piece) // Changed from Piece?
        {
            if (piece == Piece.None) return false;
            return (int)piece >= 'a' && (int)piece <= 'z';
        }

        public bool IsPieceOwnedByPlayer(Piece piece, Player player) // Changed from Piece?
        {
            if (piece == Piece.None) return false;
            return player == Player.White ? IsWhitePiece(piece) : IsBlackPiece(piece);
        }

        private bool IsValidMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            if (!IsValidPosition(fromRow, fromCol) || !IsValidPosition(toRow, toCol))
                return false;

            var piece = _board.GetPieceAt(fromRow, fromCol); // Changed
            if (piece == Piece.None || !IsPieceOwnedByPlayer(piece, currentPlayer))
                return false;

            var targetPiece = _board.GetPieceAt(toRow, toCol); // Changed
            if (targetPiece != Piece.None && IsPieceOwnedByPlayer(targetPiece, currentPlayer))
                return false;

            switch (piece)
            {
                case Piece.WhitePawn:
                case Piece.BlackPawn:
                    return IsValidPawnMove(fromRow, fromCol, toRow, toCol);
                case Piece.WhiteRook:
                case Piece.BlackRook:
                    return IsValidRookMove(fromRow, fromCol, toRow, toCol);
                case Piece.WhiteKnight:
                case Piece.BlackKnight:
                    return IsValidKnightMove(fromRow, fromCol, toRow, toCol);
                case Piece.WhiteBishop:
                case Piece.BlackBishop:
                    return IsValidBishopMove(fromRow, fromCol, toRow, toCol);
                case Piece.WhiteQueen:
                case Piece.BlackQueen:
                    return IsValidQueenMove(fromRow, fromCol, toRow, toCol);
                case Piece.WhiteKing:
                case Piece.BlackKing:
                    return IsValidKingMove(fromRow, fromCol, toRow, toCol);
                default: return false;
            }
        }

        public bool IsValidPawnMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            var piece = _board.GetPieceAt(fromRow, fromCol); // Changed
            bool isWhite = IsWhitePiece(piece);
            int direction = isWhite ? -1 : 1;
            int startRow = isWhite ? 6 : 1;

            // 前進
            if (fromCol == toCol && _board.GetPieceAt(toRow, toCol) == Piece.None) // Changed
            {
                if (toRow == fromRow + direction) return true;
                // Make sure path is clear for 2-square move
                if (fromRow == startRow && toRow == fromRow + 2 * direction && _board.GetPieceAt(fromRow + direction, fromCol) == Piece.None) return true; // Changed
            }

            // 斜め攻撃（通常の取り）
            if (Math.Abs(fromCol - toCol) == 1 && toRow == fromRow + direction)
            {
                if (_board.GetPieceAt(toRow, toCol) != Piece.None) return true; // Changed

                // アンパサン
                if (_enPassantTarget.Row() != -1 &&
                    _enPassantTarget.Row() == toRow &&
                    _enPassantTarget.Col() == toCol)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsValidRookMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            if (fromRow != toRow && fromCol != toCol) return false;
            return _board.IsPathClear(fromRow, fromCol, toRow, toCol); // Changed
        }

        private bool IsValidKnightMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            int rowDiff = Math.Abs(fromRow - toRow);
            int colDiff = Math.Abs(fromCol - toCol);
            return (rowDiff == 2 && colDiff == 1) || (rowDiff == 1 && colDiff == 2);
        }

        private bool IsValidBishopMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            if (Math.Abs(fromRow - toRow) != Math.Abs(fromCol - toCol)) return false;
            return _board.IsPathClear(fromRow, fromCol, toRow, toCol); // Changed
        }

        private bool IsValidQueenMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            return IsValidRookMove(fromRow, fromCol, toRow, toCol) ||
                   IsValidBishopMove(fromRow, fromCol, toRow, toCol);
        }

        private bool IsValidKingMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            int rowDiff = Math.Abs(fromRow - toRow);
            int colDiff = Math.Abs(fromCol - toCol);

            // 通常の王の移動
            if (rowDiff <= 1 && colDiff <= 1) return true;

            // キャスリング
            if (rowDiff == 0 && colDiff == 2)
                return IsValidCastling(fromRow, fromCol, toRow, toCol);

            return false;
        }
        public bool IsValidCastling(int fromRow, int fromCol, int toRow, int toCol)
        {
            var piece = _board.GetPieceAt(fromRow, fromCol); // Changed
            bool isWhite = IsWhitePiece(piece);
            var player = isWhite ? Player.White : Player.Black;
            int kingRow = isWhite ? 7 : 0;

            if (fromRow != kingRow || fromCol != 4)
            {
                Debug.LogError($"Invalid castling attempt from {PositionToString(fromRow, fromCol)} to {PositionToString(toRow, toCol)}");
                return false;
            }
            if (toRow != kingRow)
            {
                Debug.LogError($"Invalid castling attempt to a different row: {PositionToString(toRow, toCol)}");
                return false;
            }

            var playerCastlingRights = player == Player.White ? this._castlingRights.White() : this._castlingRights.Black();
            if (!playerCastlingRights.Kingside() && !playerCastlingRights.Queenside())
            {
                Debug.LogError($"Player {player} has no castling rights.");
                return false;
            }

            if (IsKingInCheck(player))
            {
                Debug.LogError($"Player {player} cannot castle while in check.");
                return false;
            }

            if (toCol == 6) // キングサイドキャスリング
            {
                if (!playerCastlingRights.Kingside())
                {
                    Debug.LogError($"Player {player} cannot castle kingside.");
                    return false;
                }

                var rook = _board.GetPieceAt(kingRow, 7); // Changed
                if (rook == Piece.None || !rook.IsRook() || IsWhitePiece(rook) != isWhite)
                {
                    Debug.LogError($"Invalid rook for kingside castling at {PositionToString(kingRow, 7)}");
                    Debug.LogError($"{rook}, {piece}, {isWhite}, {IsWhitePiece(rook)}");
                    return false;
                }

                if (_board.GetPieceAt(kingRow, 5) != Piece.None || _board.GetPieceAt(kingRow, 6) != Piece.None)
                {
                    Debug.LogError($"Path not clear for kingside castling from {PositionToString(kingRow, 4)} to {PositionToString(kingRow, 6)}");
                    // Changed
                    return false;
                }

                if (WouldBeInCheck(player, kingRow, 5) || WouldBeInCheck(player, kingRow, 6))
                {
                    Debug.LogError($"Castling would put player {player} in check.");
                    return false;
                }

                return true;
            }
            else if (toCol == 2) // クイーンサイドキャスリング
            {
                if (!playerCastlingRights.Queenside()) return false;

                var rook = _board.GetPieceAt(kingRow, 0); // Changed
                if (rook == Piece.None || !rook.IsRook() || IsWhitePiece(rook) != isWhite)
                    return false;

                if (_board.GetPieceAt(kingRow, 1) != Piece.None || _board.GetPieceAt(kingRow, 2) != Piece.None || _board.GetPieceAt(kingRow, 3) != Piece.None) // Changed
                    return false;

                if (WouldBeInCheck(player, kingRow, 2) || WouldBeInCheck(player, kingRow, 3))
                    return false;

                return true;
            }

            return false;
        }

        public Position FindKing(Player player)
        {
            return _board.FindKing(player); // Changed
        }

        public bool IsSquareAttacked(int row, int col, Player byPlayer)
        {
            for (int fromRow = 0; fromRow < 8; fromRow++)
            {
                for (int fromCol = 0; fromCol < 8; fromCol++)
                {
                    var piece = _board.GetPieceAt(fromRow, fromCol); // Changed
                    if (piece != Piece.None && IsPieceOwnedByPlayer(piece, byPlayer))
                    {
                        if (IsValidMoveForAttack(fromRow, fromCol, row, col))
                            return true;
                    }
                }
            }
            return false;
        }

        private bool IsValidMoveForAttack(int fromRow, int fromCol, int toRow, int toCol)
        {
            if (!IsValidPosition(fromRow, fromCol) || !IsValidPosition(toRow, toCol))
                return false;

            var piece = _board.GetPieceAt(fromRow, fromCol); // Changed
            if (piece == Piece.None) return false;

            switch (piece)
            {
                case Piece.WhitePawn:
                case Piece.BlackPawn:
                    return IsValidPawnAttack(fromRow, fromCol, toRow, toCol);
                case Piece.WhiteRook:
                case Piece.BlackRook:
                    return IsValidRookMove(fromRow, fromCol, toRow, toCol);
                case Piece.WhiteKnight:
                case Piece.BlackKnight:
                    return IsValidKnightMove(fromRow, fromCol, toRow, toCol);
                case Piece.WhiteBishop:
                case Piece.BlackBishop:
                    return IsValidBishopMove(fromRow, fromCol, toRow, toCol);
                case Piece.WhiteQueen:
                case Piece.BlackQueen:
                    return IsValidQueenMove(fromRow, fromCol, toRow, toCol);
                case Piece.WhiteKing: 
                case Piece.BlackKing:
                    return Math.Abs(fromRow - toRow) <= 1 && Math.Abs(fromCol - toCol) <= 1;
                default: return false;
            }
        }

        private bool IsValidPawnAttack(int fromRow, int fromCol, int toRow, int toCol)
        {
            var piece = _board.GetPieceAt(fromRow, fromCol); // Changed
            bool isWhite = IsWhitePiece(piece);
            int direction = isWhite ? -1 : 1;

            if (Math.Abs(fromCol - toCol) == 1 && toRow == fromRow + direction)
                return true;

            return false;
        }

        public bool IsKingInCheck(Player player)
        {
            var king = FindKing(player);
            if (king.Row() == -1) return false; // Check if king position is invalid

            var opponent = player == Player.White ? Player.Black : Player.White;
            return IsSquareAttacked(king.Row(), king.Col(), opponent);
        }

        private bool WouldBeInCheck(Player player, int kingRow, int kingCol)
        {
            var originalPiece = _board.GetPieceAt(kingRow, kingCol); // Changed
            var kingPieceToPlace = player == Player.White ? Piece.WhiteKing : Piece.BlackKing;

            _board.SetPieceAt(kingRow, kingCol, kingPieceToPlace); // Changed

            var opponent = player == Player.White ? Player.Black : Player.White;
            bool inCheck = IsSquareAttacked(kingRow, kingCol, opponent);

            _board.SetPieceAt(kingRow, kingCol, originalPiece); // Changed

            return inCheck;
        }

        private bool WouldMoveResultInCheck(int fromRow, int fromCol, int toRow, int toCol, Player player)
        {
            var originalFromPiece = _board.GetPieceAt(fromRow, fromCol); // Changed
            var originalToPiece = _board.GetPieceAt(toRow, toCol);     // Changed

            _board.SetPieceAt(toRow, toCol, originalFromPiece); // Changed
            _board.SetPieceAt(fromRow, fromCol, Piece.None); // Changed

            bool inCheck = IsKingInCheck(player);

            _board.SetPieceAt(fromRow, fromCol, originalFromPiece); // Changed
            _board.SetPieceAt(toRow, toCol, originalToPiece); // Changed

            return inCheck;
        }

        public bool HasValidMoves(Player player)
        {
            for (int fromRow = 0; fromRow < 8; fromRow++)
            {
                for (int fromCol = 0; fromCol < 8; fromCol++)
                {
                    var piece = _board.GetPieceAt(fromRow, fromCol);
                    if (piece != Piece.None && IsPieceOwnedByPlayer(piece, player))
                    {
                        // 駒の種類に応じた移動可能範囲の走査
                        switch (piece)
                        {
                            case Piece.WhitePawn:
                            case Piece.BlackPawn:
                                if (CheckPawnMoves(fromRow, fromCol, player)) return true;
                                break;
                            case Piece.WhiteRook:
                            case Piece.BlackRook:
                                if (CheckRookMoves(fromRow, fromCol, player)) return true;
                                break;
                            case Piece.WhiteKnight:
                            case Piece.BlackKnight:
                                if (CheckKnightMoves(fromRow, fromCol, player)) return true;
                                break;
                            case Piece.WhiteBishop:
                            case Piece.BlackBishop:
                                if (CheckBishopMoves(fromRow, fromCol, player)) return true;
                                break;
                            case Piece.WhiteQueen:
                            case Piece.BlackQueen:
                                if (CheckQueenMoves(fromRow, fromCol, player)) return true;
                                break;
                            case Piece.WhiteKing:
                            case Piece.BlackKing:
                                if (CheckKingMoves(fromRow, fromCol, player)) return true;
                                break;
                        }
                    }
                }
            }
            return false;
        }

        // 各駒の移動可能範囲をチェックするヘルパーメソッド群
        private bool CheckPawnMoves(int fromRow, int fromCol, Player player)
        {
            int direction = player == Player.White ? -1 : 1;
            int startRow = player == Player.White ? 6 : 1;

            // 1マス前進
            if (IsValidMove(fromRow, fromCol, fromRow + direction, fromCol) &&
                !WouldMoveResultInCheck(fromRow, fromCol, fromRow + direction, fromCol, player)) return true;
            
            // 初手の2マス前進
            if (fromRow == startRow)
            {
                if (IsValidMove(fromRow, fromCol, fromRow + 2 * direction, fromCol) &&
                    !WouldMoveResultInCheck(fromRow, fromCol, fromRow + 2 * direction, fromCol, player)) return true;
            }

            // 斜め攻撃 (通常の取りとアンパサン)
            if (IsValidMove(fromRow, fromCol, fromRow + direction, fromCol + 1) &&
                !WouldMoveResultInCheck(fromRow, fromCol, fromRow + direction, fromCol + 1, player)) return true;
            
            if (IsValidMove(fromRow, fromCol, fromRow + direction, fromCol - 1) &&
                !WouldMoveResultInCheck(fromRow, fromCol, fromRow + direction, fromCol - 1, player)) return true;
            
            return false;
        }

        private bool CheckRookMoves(int fromRow, int fromCol, Player player)
        {
            // 水平方向 (同じ行の異なる列)
            for (int toCol = 0; toCol < 8; toCol++)
            {
                if (toCol == fromCol) continue;
                if (IsValidMove(fromRow, fromCol, fromRow, toCol) &&
                    !WouldMoveResultInCheck(fromRow, fromCol, fromRow, toCol, player)) return true;
            }
            // 垂直方向 (同じ列の異なる行)
            for (int toRow = 0; toRow < 8; toRow++)
            {
                if (toRow == fromRow) continue;
                if (IsValidMove(fromRow, fromCol, toRow, fromCol) &&
                    !WouldMoveResultInCheck(fromRow, fromCol, toRow, fromCol, player)) return true;
            }
            return false;
        }

        private bool CheckKnightMoves(int fromRow, int fromCol, Player player)
        {
            int[] dRow = { -2, -2, -1, -1, 1, 1, 2, 2 };
            int[] dCol = { -1, 1, -2, 2, -2, 2, -1, 1 };

            for (int i = 0; i < 8; i++)
            {
                int toRow = fromRow + dRow[i];
                int toCol = fromCol + dCol[i];
                if (IsValidMove(fromRow, fromCol, toRow, toCol) &&
                    !WouldMoveResultInCheck(fromRow, fromCol, toRow, toCol, player))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckBishopMoves(int fromRow, int fromCol, Player player)
        {
            for (int i = 1; i < 8; i++) 
            {
                int[][] directions = {
                    new[] {i, i},   
                    new[] {i, -i},  
                    new[] {-i, i},  
                    new[] {-i, -i}  
                };
                foreach (var dir in directions)
                {
                    int toRow = fromRow + dir[0];
                    int toCol = fromCol + dir[1];
                    if (IsValidMove(fromRow, fromCol, toRow, toCol) &&
                        !WouldMoveResultInCheck(fromRow, fromCol, toRow, toCol, player)) return true;
                }
            }
            return false;
        }

        private bool CheckQueenMoves(int fromRow, int fromCol, Player player)
        {
            if (CheckRookMoves(fromRow, fromCol, player)) return true;
            if (CheckBishopMoves(fromRow, fromCol, player)) return true;
            return false;
        }

        private bool CheckKingMoves(int fromRow, int fromCol, Player player)
        {
            for (int dRow = -1; dRow <= 1; dRow++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    if (dRow == 0 && dCol == 0) continue; 
                    int toRow = fromRow + dRow;
                    int toCol = fromCol + dCol;
                    if (IsValidMove(fromRow, fromCol, toRow, toCol) &&
                        !WouldMoveResultInCheck(fromRow, fromCol, toRow, toCol, player))
                    {
                        return true;
                    }
                }
            }
            
            if (IsValidMove(fromRow, fromCol, fromRow, fromCol + 2) && // Kingside castling
                !WouldMoveResultInCheck(fromRow, fromCol, fromRow, fromCol + 2, player)) return true;
            
            if (IsValidMove(fromRow, fromCol, fromRow, fromCol - 2) && // Queenside castling
                !WouldMoveResultInCheck(fromRow, fromCol, fromRow, fromCol - 2, player)) return true;

            return false;
        }

        public int[] GetMovablePieces()
        {
            int[] movablePieces = new int[16]; // 最大で16個のコマ
            int count = 0;

            // すべての盤面を走査
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var piece = _board.GetPieceAt(row, col);
                    // 現在のプレイヤーのコマかチェック
                    if (piece != Piece.None && IsPieceOwnedByPlayer(piece, currentPlayer))
                    {
                        // このコマが移動可能かどうか確認
                        if (HasValidMove(row, col))
                        {
                            movablePieces[count] = (row * 8 + col);
                            count++;
                            if (count >= 16) break; // 最大数に達した場合
                        }
                    }
                }
                if (count >= 16) break;
            }

            return movablePieces.Resize(count);
        }

        public int[] GetValidMovesForPiece(int fromRow, int fromCol)
        {
            var piece = _board.GetPieceAt(fromRow, fromCol);
            if (piece == Piece.None || !IsPieceOwnedByPlayer(piece, currentPlayer))
            {
                return new int[0]; // コマがないか、自分のコマではない場合は空の配列を返す
            }

            var validMoves = new int[64]; // 最大で考えられる移動先の数 (実際はもっと少ない)
            int count = 0;

            // 各駒の移動パターンに基づいて候補を生成し、検証する
            switch (piece)
            {
                case Piece.WhitePawn:
                case Piece.BlackPawn:
                    AddPawnMoves(fromRow, fromCol, currentPlayer, validMoves, ref count);
                    break;
                case Piece.WhiteRook:
                case Piece.BlackRook:
                    AddRookMoves(fromRow, fromCol, currentPlayer, validMoves, ref count);
                    break;
                case Piece.WhiteKnight:
                case Piece.BlackKnight:
                    AddKnightMoves(fromRow, fromCol, currentPlayer, validMoves, ref count);
                    break;
                case Piece.WhiteBishop:
                case Piece.BlackBishop:
                    AddBishopMoves(fromRow, fromCol, currentPlayer, validMoves, ref count);
                    break;
                case Piece.WhiteQueen:
                case Piece.BlackQueen:
                    AddQueenMoves(fromRow, fromCol, currentPlayer, validMoves, ref count);
                    break;
                case Piece.WhiteKing:
                case Piece.BlackKing:
                    AddKingMoves(fromRow, fromCol, currentPlayer, validMoves, ref count);
                    break;
            }
            
            return validMoves.Resize(count);
        }

        // 各駒の移動先をリストに追加するヘルパーメソッド群
        private void AddPawnMoves(int fromRow, int fromCol, Player player, int[] moves, ref int count)
        {
            int direction = player == Player.White ? -1 : 1;
            int startRow = player == Player.White ? 6 : 1;

            // 1マス前進
            TryAddMove(fromRow, fromCol, fromRow + direction, fromCol, player, moves, ref count);
            // 初手の2マス前進
            if (fromRow == startRow)
            {
                // 1マス目が空で、かつ2マス目も空の場合のみ
                if (_board.GetPieceAt(fromRow + direction, fromCol) == Piece.None)
                {
                    TryAddMove(fromRow, fromCol, fromRow + 2 * direction, fromCol, player, moves, ref count);
                }
            }
            // 斜め攻撃 (通常の取りとアンパサン)
            TryAddMove(fromRow, fromCol, fromRow + direction, fromCol + 1, player, moves, ref count);
            TryAddMove(fromRow, fromCol, fromRow + direction, fromCol - 1, player, moves, ref count);
        }

        private void AddRookMoves(int fromRow, int fromCol, Player player, int[] moves, ref int count)
        {
            // 水平・垂直方向
            int[] dRows = { 0, 0, 1, -1 };
            int[] dCols = { 1, -1, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j < 8; j++)
                {
                    int toRow = fromRow + dRows[i] * j;
                    int toCol = fromCol + dCols[i] * j;
                    if (!IsValidPosition(toRow, toCol)) break; // 盤外に出たらその方向の探索を終了
                    if (TryAddMove(fromRow, fromCol, toRow, toCol, player, moves, ref count))
                    {
                        if (_board.GetPieceAt(toRow, toCol) != Piece.None) break; // 相手の駒を取ったらそこでストップ
                    }
                    else break; // 自分の駒などでブロックされている場合
                }
            }
        }

        private void AddKnightMoves(int fromRow, int fromCol, Player player, int[] moves, ref int count)
        {
            int[] dRow = { -2, -2, -1, -1, 1, 1, 2, 2 };
            int[] dCol = { -1, 1, -2, 2, -2, 2, -1, 1 };
            for (int i = 0; i < 8; i++)
            {
                TryAddMove(fromRow, fromCol, fromRow + dRow[i], fromCol + dCol[i], player, moves, ref count);
            }
        }

        private void AddBishopMoves(int fromRow, int fromCol, Player player, int[] moves, ref int count)
        {
            // 斜め方向
            int[] dRows = { 1, 1, -1, -1 };
            int[] dCols = { 1, -1, 1, -1 };
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j < 8; j++)
                {
                    int toRow = fromRow + dRows[i] * j;
                    int toCol = fromCol + dCols[i] * j;
                    if (!IsValidPosition(toRow, toCol)) break;
                    if (TryAddMove(fromRow, fromCol, toRow, toCol, player, moves, ref count))
                    {
                        if (_board.GetPieceAt(toRow, toCol) != Piece.None) break; 
                    }
                    else break;
                }
            }
        }

        private void AddQueenMoves(int fromRow, int fromCol, Player player, int[] moves, ref int count)
        {
            AddRookMoves(fromRow, fromCol, player, moves, ref count); // ルークの動き
            AddBishopMoves(fromRow, fromCol, player, moves, ref count); // ビショップの動き
        }

        private void AddKingMoves(int fromRow, int fromCol, Player player, int[] moves, ref int count)
        {
            // 通常の移動 (周囲8マス)
            for (int dRow = -1; dRow <= 1; dRow++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    if (dRow == 0 && dCol == 0) continue;
                    TryAddMove(fromRow, fromCol, fromRow + dRow, fromCol + dCol, player, moves, ref count);
                }
            }
            // キャスリング
            TryAddMove(fromRow, fromCol, fromRow, fromCol + 2, player, moves, ref count); // キングサイド
            TryAddMove(fromRow, fromCol, fromRow, fromCol - 2, player, moves, ref count); // クイーンサイド
        }

        // IsValidMove と WouldMoveResultInCheck を満たす場合に移動先をリストに追加するヘルパー
        private bool TryAddMove(int fromRow, int fromCol, int toRow, int toCol, Player player, int[] moves, ref int count)
        {
            if (IsValidMove(fromRow, fromCol, toRow, toCol) && 
                !WouldMoveResultInCheck(fromRow, fromCol, toRow, toCol, player))
            {
                Debug.Log($"toRow: {toRow}, toCol: {toCol}");
                moves[count] = (toRow * 8 + toCol); // 1次元配列に変換
                count++;
                return true;
            }
            return false;
        }

        private bool HasValidMove(int fromRow, int fromCol) 
        {
            var piece = _board.GetPieceAt(fromRow, fromCol);
            Player player = currentPlayer; 

            if (piece == Piece.None) return false; 

            switch (piece)
            {
                case Piece.WhitePawn:
                case Piece.BlackPawn:
                    return CheckPawnMoves(fromRow, fromCol, player);
                case Piece.WhiteRook:
                case Piece.BlackRook:
                    return CheckRookMoves(fromRow, fromCol, player);
                case Piece.WhiteKnight:
                case Piece.BlackKnight:
                    return CheckKnightMoves(fromRow, fromCol, player);
                case Piece.WhiteBishop:
                case Piece.BlackBishop:
                    return CheckBishopMoves(fromRow, fromCol, player);
                case Piece.WhiteQueen:
                case Piece.BlackQueen:
                    return CheckQueenMoves(fromRow, fromCol, player);
                case Piece.WhiteKing:
                case Piece.BlackKing:
                    return CheckKingMoves(fromRow, fromCol, player);
                default:
                    return false; 
            }
        }

        public void CheckGameStatus()
        {
            bool currentPlayerInCheck = IsKingInCheck(currentPlayer);
            bool hasValidMovesAvailable = HasValidMoves(currentPlayer);

            _check = currentPlayerInCheck;
            _checkmate = currentPlayerInCheck && !hasValidMovesAvailable;
            _stalemate = !currentPlayerInCheck && !hasValidMovesAvailable;
            gameOver = _checkmate || _stalemate;


            if (_checkmate)
            {
                winner = (currentPlayer == Player.White) ? Player.Black : Player.White; // Valid
            }
            else if (_stalemate)
            {
                winner = Player.None; // Valid
            }
        }

        public bool HandlePromotion(int toRow, int toCol, Player player, PromotionPiece promotionPiece)
        {
            bool isWhite = player == Player.White;
            int promoteToRow = isWhite ? 0 : 7;

            if (toRow == promoteToRow)
            {
                _board.SetPieceAt(toRow, toCol, GetPromotedPiece(promotionPiece, player));
                return true;
            }
            return false;
        }
        
        private Piece GetPromotedPiece(PromotionPiece promotionPiece, Player player)
        {
            switch (promotionPiece)
            {
                case PromotionPiece.Queen:
                    return player == Player.White ? Piece.WhiteQueen : Piece.BlackQueen;
                case PromotionPiece.Rook:
                    return player == Player.White ? Piece.WhiteRook : Piece.BlackRook;
                case PromotionPiece.Bishop:
                    return player == Player.White ? Piece.WhiteBishop : Piece.BlackBishop;
                case PromotionPiece.Knight:
                    return player == Player.White ? Piece.WhiteKnight : Piece.BlackKnight;
                default:
                    return Piece.None;
            }
        }
        
        public virtual MoveResult MakeMove(string fromPos, string toPos, PromotionPiece promotionPiece)
        {
            var from = ParsePosition(fromPos);
            var to = ParsePosition(toPos);

            if (from.Row() == -1 || to.Row() == -1)
                return MoveResult.New(false, "無効な位置です。例: a2 a4");

            if (!IsValidMove(from.Row(), from.Col(), to.Row(), to.Col()))
                return MoveResult.New(false, "無効な手です。");

            if (WouldMoveResultInCheck(from.Row(), from.Col(), to.Row(), to.Col(), currentPlayer))
                return MoveResult.New(false, "この手は王をチェック状態にするため無効です。");

            var piece = _board.GetPieceAt(from.Row(), from.Col()); // Changed
            var capturedPiece = _board.GetPieceAt(to.Row(), to.Col()); // Changed

            string moveDescription = "";
            bool isEnPassant = false;
            bool isCastling = false;
            bool isPromotion = false;

            if (piece.IsPawn() && _enPassantTarget.Row() != -1 &&
                _enPassantTarget.Row() == to.Row() && _enPassantTarget.Col() == to.Col())
            {
                isEnPassant = true;
                int captureRow = currentPlayer == Player.White ? to.Row() + 1 : to.Row() - 1;
                var capturedPawn = _board.GetPieceAt(captureRow, to.Col()); // Changed
                _board.SetPieceAt(captureRow, to.Col(), Piece.None); // Changed

                moveDescription = $"{PositionToString(from.Row(), from.Col())}から{PositionToString(to.Row(), to.Col())}へ移動（アンパサン）し、{GetPieceSymbol(capturedPawn)}を取りました";
            }

            if (piece.IsKing() && Math.Abs(from.Col() - to.Col()) == 2)
            {
                isCastling = true;
                bool isKingside = to.Col() == 6;
                int rookFromCol = isKingside ? 7 : 0;
                int rookToCol = isKingside ? 5 : 3;
                int kingRow = from.Row();

                var rook = _board.GetPieceAt(kingRow, rookFromCol); // Changed
                _board.SetPieceAt(kingRow, rookToCol, rook); // Changed
                _board.SetPieceAt(kingRow, rookFromCol, Piece.None); // Changed

                moveDescription = $"{(isKingside ? "キングサイド" : "クイーンサイド")}キャスリング";
            }

            _board.SetPieceAt(to.Row(), to.Col(), piece); // Changed
            _board.SetPieceAt(from.Row(), from.Col(), Piece.None); // Changed

            if (piece.IsPawn())
            {
                bool isWhite = IsWhitePiece(piece);
                int promoteRow = isWhite ? 0 : 7;
                if (to.Row() == promoteRow)
                {
                    isPromotion = true;
                    HandlePromotion(to.Row(), to.Col(), currentPlayer, promotionPiece);
                    var promotedPieceOnBoard = _board.GetPieceAt(to.Row(), to.Col()); // Changed
                    moveDescription = $"{PositionToString(from.Row(), from.Col())}から{PositionToString(to.Row(), to.Col())}へ移動し、{GetPieceSymbol(promotedPieceOnBoard)}に昇格";
                }
            }

            _enPassantTarget = Position.New(-1, -1);
            if (piece.IsPawn() && Math.Abs(from.Row() - to.Row()) == 2)
            {
                int targetRow = (from.Row() + to.Row()) / 2;
                _enPassantTarget = Position.New(targetRow, to.Col());
            }

            if (piece.IsKing())
            {
                if (currentPlayer == Player.White)
                {
                    this._castlingRights.SetWhite(CastlingRights.New(false, false)); // Re-added ChessGame.
                }
                else
                {
                    this._castlingRights.SetBlack(CastlingRights.New(false, false)); // Re-added ChessGame.
                }
            }
            else if (piece.IsRook())
            {
                if (currentPlayer == Player.White)
                {
                    var whiteRights = this._castlingRights.White(); // Changed
                    if (from.Col() == 0) // Changed
                        whiteRights.SetQueenside(false); // Changed
                    else if (from.Col() == 7) // Changed
                        whiteRights.SetKingside(false); // Changed
                }
                else
                {
                    var blackRights = this._castlingRights.Black(); // Changed
                    if (from.Col() == 0) // Changed
                        blackRights.SetQueenside(false); // Changed
                    else if (from.Col() == 7) // Changed
                        blackRights.SetKingside(false); // Changed
                }
            }

            // 相手の飛車が取られた場合のキャスリング権利更新
            if (capturedPiece != Piece.None && piece.IsRook()) // Changed
            {
                var opponent = currentPlayer == Player.White ? Player.Black : Player.White;
                if (opponent == Player.White)
                {
                    var whiteRights = this._castlingRights.White();
                    if (to.Col() == 0)
                        whiteRights.SetQueenside(false);
                    else if (to.Col() == 7)
                        whiteRights.SetKingside(false);
                }
                else
                {
                    var blackRights = this._castlingRights.Black();
                    if (to.Col() == 0)
                        blackRights.SetQueenside(false);
                    else if (to.Col() == 7)
                        blackRights.SetKingside(false);
                }
            }

            currentPlayer = currentPlayer == Player.White ? Player.Black : Player.White;
            CheckGameStatus();

            if (!isCastling && !isEnPassant && !isPromotion)
            {
                if (capturedPiece != Piece.None) // Changed
                {
                    moveDescription = $"{PositionToString(from.Row(), from.Col())}から{PositionToString(to.Row(), to.Col())}へ移動し、{GetPieceSymbol(capturedPiece)}を取りました";
                }
                else
                {
                    moveDescription = $"{PositionToString(from.Row(), from.Col())}から{PositionToString(to.Row(), to.Col())}へ移動";
                }
            }

            if (_checkmate)
            {
                moveDescription += " - チェックメイト！";
            }
            else if (_stalemate)
            {
                moveDescription += " - ステイルメイト（引き分け）";
            }
            else if (_check)
            {
                moveDescription += " - チェック！";
            }

            return MoveResult.New(true, moveDescription);
        }

        public string GetCurrentPlayerName()
        {
            return currentPlayer == Player.White ? "白" : "黒";
        }

        public bool IsGameOver()
        {
            return gameOver;
        }

        public Player GetWinner()
        {
            return winner;
        }

        public string GetGameStatus()
        {
            if (_checkmate) return "チェックメイト";
            if (_stalemate) return "ステイルメイト";
            if (_check) return "チェック";
            return "通常";
        }

        // ============ 盤面シリアライズ/デシリアライズ機能 ============

        /// <summary>
        /// チェス盤面の状態をシリアライズ
        /// 265ビット（盤面256bit + 手番1bit + キャスリング4bit + アンパッサン4bit）を
        /// 5つのulongに格納
        /// </summary>
        public SerializedPosition SerializePosition()
        {
            ulong[] boardData = _board.SerializeBoard(); // Changed: Use BoardDataExt.SerializeBoard

            var metadata = 0UL;

            // メタデータをエンコード
            // bit 0: 手番（0=白, 1=黒）
            if (currentPlayer == Player.Black)
            {
                metadata |= 1UL;
            }

            // bit 1-4: キャスリング権利
            int castlingBits = 0;
            if (this._castlingRights.White().Kingside()) castlingBits |= 1;
            if (this._castlingRights.White().Queenside()) castlingBits |= 2;
            if (this._castlingRights.Black().Kingside()) castlingBits |= 4;
            if (this._castlingRights.Black().Queenside()) castlingBits |= 8;
            metadata |= (ulong)castlingBits << 1;

            // bit 5-8: アンパッサンのターゲット列（0=なし, 1-8=a-h列）
            int enPassantValue = 0;
            if (_enPassantTarget.Row() != -1) 
            {
                enPassantValue = _enPassantTarget.Col() + 1;
            }
            metadata |= (ulong)enPassantValue << 5;

            // 新しいゲーム状態の変数をエンコード (9ビット目から開始)
            // bit 9: gameOver (0=false, 1=true)
            if (gameOver)
            {
                metadata |= 1UL << 9;
            }

            // bit 10-11: winner (00=None, 01=White, 10=Black)
            ulong winnerValue = 0;
            if (winner == Player.White) winnerValue = 1;
            else if (winner == Player.Black) winnerValue = 2;
            metadata |= winnerValue << 10;

            // bit 12: _check (0=false, 1=true)
            if (_check)
            {
                metadata |= 1UL << 12;
            }

            // bit 13: _checkmate (0=false, 1=true)
            if (_checkmate)
            {
                metadata |= 1UL << 13;
            }

            // bit 14: _stalemate (0=false, 1=true)
            if (_stalemate)
            {
                metadata |= 1UL << 14;
            }

            return SerializedPosition.New(boardData, metadata);
        }

        /// <summary>
        /// シリアライズされた盤面データから状態を復元
        /// </summary>
        public void DeserializePosition(SerializedPosition data)
        {
            _board.DeserializeBoard(data.Board());

            var meta = data.Metadata();

            currentPlayer = (meta & 1) != 0 ? Player.Black : Player.White;

            int castlingBits = (int)((meta >> 1) & 0xF);
            this._castlingRights = GameState.New(
                CastlingRights.New((castlingBits & 1) != 0, (castlingBits & 2) != 0),
                CastlingRights.New((castlingBits & 4) != 0, (castlingBits & 8) != 0)
            );

            int enPassantValue = (int)((meta >> 5) & 0xF);
            if (enPassantValue == 0)
            {
                _enPassantTarget = Position.New(-1, -1);
            }
            else
            {
                int col = enPassantValue - 1;
                int row = currentPlayer == Player.White ? 2 : 5; 
                _enPassantTarget = Position.New(row, col);
            }

            // 新しいゲーム状態の変数をデコード
            gameOver = (meta & (1UL << 9)) != 0;

            ulong winnerValue = (meta >> 10) & 0x3; // 2ビット取得
            if (winnerValue == 1) winner = Player.White;
            else if (winnerValue == 2) winner = Player.Black;
            else winner = Player.None;

            _check = (meta & (1UL << 12)) != 0;
            _checkmate = (meta & (1UL << 13)) != 0;
            _stalemate = (meta & (1UL << 14)) != 0;
        }
    }
}
