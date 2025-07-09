using UdonSharp;
using VRC.SDKBase;

namespace jp.ootr.chess
{
    public class BoardData : UdonSharpBehaviour
    {
        // コンストラクタの代わりに New メソッドを使用
        public static BoardData New()
        {
            Piece[][] board = new Piece[8][];
            for (int r = 0; r < 8; r++)
            {
                board[r] = new Piece[8];
                for (int c = 0; c < 8; c++)
                {
                    board[r][c] = Piece.None;
                }
            }

            // 白の駒を配置
            board[7][0] = Piece.WhiteRook; board[7][1] = Piece.WhiteKnight; board[7][2] = Piece.WhiteBishop;
            board[7][3] = Piece.WhiteQueen; board[7][4] = Piece.WhiteKing; board[7][5] = Piece.WhiteBishop;
            board[7][6] = Piece.WhiteKnight; board[7][7] = Piece.WhiteRook;
            for (int col = 0; col < 8; col++)
                board[6][col] = Piece.WhitePawn;

            // 黒の駒を配置
            board[0][0] = Piece.BlackRook; board[0][1] = Piece.BlackKnight; board[0][2] = Piece.BlackBishop;
            board[0][3] = Piece.BlackQueen; board[0][4] = Piece.BlackKing; board[0][5] = Piece.BlackBishop;
            board[0][6] = Piece.BlackKnight; board[0][7] = Piece.BlackRook;
            for (int col = 0; col < 8; col++)
                board[1][col] = Piece.BlackPawn;
            
            var buff = new object[] { board };
            return (BoardData)(object)buff;
        }
    }

    public static class BoardDataExt
    {
        private static Piece[][] GetInternalBoard(this BoardData bd)
        {
            return (Piece[][])(((object[])(object)bd)[0]);
        }

        // InitializeBoard は New() の中で実行されるため不要

        public static Piece GetPieceAt(this BoardData bd, int row, int col)
        {
            if (!bd.IsValidPosition(row, col)) // 拡張メソッドを呼び出す
            {
                return Piece.None; 
            }
            return bd.GetInternalBoard()[row][col];
        }

        public static void SetPieceAt(this BoardData bd, int row, int col, Piece piece)
        {
            if (!bd.IsValidPosition(row, col)) // 拡張メソッドを呼び出す
            {
                return;
            }
            bd.GetInternalBoard()[row][col] = piece;
        }

        public static bool IsValidPosition(this BoardData bd, int row, int col)
        {
            // このメソッドは BoardData 自体には依存しないため、bd を使わないが、拡張メソッドのシグネチャは維持
            return row >= 0 && row < 8 && col >= 0 && col < 8;
        }

        public static bool IsPathClear(this BoardData bd, int fromRow, int fromCol, int toRow, int toCol)
        {
            int rowStep = toRow == fromRow ? 0 : (toRow > fromRow ? 1 : -1);
            int colStep = toCol == fromCol ? 0 : (toCol > fromCol ? 1 : -1);

            int currentRow = fromRow + rowStep;
            int currentCol = fromCol + colStep;

            while (currentRow != toRow || currentCol != toCol)
            {
                // GetPieceAt を拡張メソッド経由で呼び出す
                if (!bd.IsValidPosition(currentRow, currentCol) || bd.GetPieceAt(currentRow, currentCol) != Piece.None)
                    return false;
                currentRow += rowStep;
                currentCol += colStep;
            }
            return true;
        }

        public static Position FindKing(this BoardData bd, Player player)
        {
            var kingPiece = player == Player.White ? Piece.WhiteKing : Piece.BlackKing;
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    // GetPieceAt を拡張メソッド経由で呼び出す
                    if (bd.GetPieceAt(r,c) == kingPiece)
                        return Position.New(r, c); // Position.New は Enums.cs で定義された疑似クラスのメソッド
                }
            }
            return Position.New(-1, -1); // King not found
        }

        public static Piece[][] GetBoardState(this BoardData bd)
        {
            return bd.GetInternalBoard();
        }

        public static void SetBoardState(this BoardData bd, Piece[][] newBoard)
        {
            // バリデーションなどが必要な場合もある
            // 内部配列を直接置き換える
            ((object[])(object)bd)[0] = newBoard;
        }

        // --- Serialization Helpers ---
        private static int PieceToValue(Piece piece)
        {
            switch (piece)
            {
                case Piece.WhitePawn: return 1;
                case Piece.WhiteKnight: return 2;
                case Piece.WhiteBishop: return 3;
                case Piece.WhiteRook: return 4;
                case Piece.WhiteQueen: return 5;
                case Piece.WhiteKing: return 6;
                case Piece.BlackPawn: return 7;
                case Piece.BlackKnight: return 8;
                case Piece.BlackBishop: return 9;
                case Piece.BlackRook: return 10;
                case Piece.BlackQueen: return 11;
                case Piece.BlackKing: return 12;
                case Piece.None: return 0;
                default: return 0; 
            }
        }

        private static Piece ValueToPiece(int value)
        {
            switch (value)
            {
                case 0: return Piece.None;
                case 1: return Piece.WhitePawn;
                case 2: return Piece.WhiteKnight;
                case 3: return Piece.WhiteBishop;
                case 4: return Piece.WhiteRook;
                case 5: return Piece.WhiteQueen;
                case 6: return Piece.WhiteKing;
                case 7: return Piece.BlackPawn;
                case 8: return Piece.BlackKnight;
                case 9: return Piece.BlackBishop;
                case 10: return Piece.BlackRook;
                case 11: return Piece.BlackQueen;
                case 12: return Piece.BlackKing;
                default: return Piece.None;
            }
        }

        public static ulong[] SerializeBoard(this BoardData bd)
        {
            var serializedBoard = new ulong[4];
            Piece[][] currentBoardState = bd.GetBoardState();

            for (int square = 0; square < 64; square++)
            {
                int row = square / 8;
                int col = square % 8;
                var piece = currentBoardState[row][col];
                int pieceValue = PieceToValue(piece);

                int ulongIndex = square / 16; 
                int bitOffset = (square % 16) * 4;

                serializedBoard[ulongIndex] |= (ulong)pieceValue << bitOffset;
            }
            return serializedBoard;
        }

        public static void DeserializeBoard(this BoardData bd, ulong[] serializedBoardData)
        {
            var newBoardState = new Piece[8][];
            for (int r = 0; r < 8; r++)
            {
                newBoardState[r] = new Piece[8];
                 for (int c = 0; c < 8; c++) // Initialize all cells
                {
                    newBoardState[r][c] = Piece.None;
                }
            }

            for (int square = 0; square < 64; square++)
            {
                int row = square / 8;
                int col = square % 8;

                int ulongIndex = square / 16;
                int bitOffset = (square % 16) * 4;

                int pieceValue = (int)((serializedBoardData[ulongIndex] >> bitOffset) & 0xF);
                newBoardState[row][col] = ValueToPiece(pieceValue);
            }
            if (!Utilities.IsValid(bd))
            {
                bd = BoardData.New();
            }
            bd.SetBoardState(newBoardState);
        }
    }
}
