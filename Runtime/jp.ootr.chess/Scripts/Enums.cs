using UdonSharp;

namespace jp.ootr.chess
{
    // --- 既存の Enum 定義 (変更なし) ---
    public enum Player
    {
        White,
        Black,
        None
    }

    public enum Piece
    {
        WhitePawn = 'P',
        WhiteRook = 'R',
        WhiteKnight = 'N',
        WhiteBishop = 'B',
        WhiteQueen = 'Q',
        WhiteKing = 'K',
        BlackPawn = 'p',
        BlackRook = 'r',
        BlackKnight = 'n',
        BlackBishop = 'b',
        BlackQueen = 'q',
        BlackKing = 'k',
        None = ' '
    }
    
    public static class PieceExt
    {
        public static bool IsWhite(this Piece piece) => (int)piece >= (int)Piece.WhitePawn && (int)piece <= (int)Piece.WhiteKing;
        public static bool IsBlack(this Piece piece) => (int)piece >= (int)Piece.BlackPawn && (int)piece <= (int)Piece.BlackKing;
        public static bool IsNone(this Piece piece) => piece == Piece.None;
        public static bool IsPawn(this Piece piece) => piece == Piece.WhitePawn || piece == Piece.BlackPawn;
        public static bool IsRook(this Piece piece) => piece == Piece.WhiteRook || piece == Piece.BlackRook;
        public static bool IsKnight(this Piece piece) => piece == Piece.WhiteKnight || piece == Piece.BlackKnight;
        public static bool IsBishop(this Piece piece) => piece == Piece.WhiteBishop || piece == Piece.BlackBishop;
        public static bool IsQueen(this Piece piece) => piece == Piece.WhiteQueen || piece == Piece.BlackQueen;
        public static bool IsKing(this Piece piece) => piece == Piece.WhiteKing || piece == Piece.BlackKing;
    }

    public enum PromotionPiece
    {
        Queen = 'q',
        Rook = 'r',
        Bishop = 'b',
        Knight = 'n'
    }

    // --- Position 疑似クラス ---
    public class Position : UdonSharpBehaviour
    {
        public static Position New(int row, int col) 
        {
            var buff = new object[] { row, col };
            return (Position)(object)(buff);
        }
    }

    public static class PositionExt  
    {
        public static int Row(this Position p) { return (int)(((object[])(object)p)[0]); }
        public static void SetRow(this Position p, int val) { ((object[])(object)p)[0] = val; }
        public static int Col(this Position p) { return (int)(((object[])(object)p)[1]); }
        public static void SetCol(this Position p, int val) { ((object[])(object)p)[1] = val; }
        
        public static string AsString(this Position p)
        {
            return $"{(char)('a' + p.Col())}{8 - p.Row()}";
        }
    }

    // --- MoveResult 疑似クラス ---
    public class MoveResult  : UdonSharpBehaviour
    {
        public static MoveResult New(bool success, string message) 
        {
            var buff = new object[] { success, message };
            return (MoveResult)(object)(buff);
        }
    }

    public static class MoveResultExt
    {
        public static bool Success(this MoveResult mr) { return (bool)(((object[])(object)mr)[0]); }
        public static void SetSuccess(this MoveResult mr, bool val) { ((object[])(object)mr)[0] = val; }
        public static string Message(this MoveResult mr) { return (string)(((object[])(object)mr)[1]); }
        public static void SetMessage(this MoveResult mr, string val) { ((object[])(object)mr)[1] = val; }
    }

    // --- CastlingRights 疑似クラス ---
    public class CastlingRights  : UdonSharpBehaviour
    {
        public static CastlingRights New(bool kingside, bool queenside) 
        {
            var buff = new object[] { kingside, queenside };
            return (CastlingRights)(object)(buff);
        }
    }

    public static class CastlingRightsExt
    {
        public static bool Kingside(this CastlingRights cr) { return (bool)(((object[])(object)cr)[0]); }
        public static void SetKingside(this CastlingRights cr, bool val) { ((object[])(object)cr)[0] = val; }
        public static bool Queenside(this CastlingRights cr) { return (bool)(((object[])(object)cr)[1]); }
        public static void SetQueenside(this CastlingRights cr, bool val) { ((object[])(object)cr)[1] = val; }
    }

    // --- GameState 疑似クラス ---
    public class GameState  : UdonSharpBehaviour
    {
        // 注意: フィールドは CastlingRights 疑似クラスのインスタンスになります
        public static GameState New(CastlingRights white, CastlingRights black) 
        {
            var buff = new object[] { white, black };
            return (GameState)(object)(buff);
        }
    }

    public static class GameStateExt
    {
        public static CastlingRights White(this GameState gs) { return (CastlingRights)(((object[])(object)gs)[0]); }
        public static void SetWhite(this GameState gs, CastlingRights val) { ((object[])(object)gs)[0] = val; }
        public static CastlingRights Black(this GameState gs) { return (CastlingRights)(((object[])(object)gs)[1]); }
        public static void SetBlack(this GameState gs, CastlingRights val) { ((object[])(object)gs)[1] = val; }
    }


    // --- SerializedPosition 疑似クラス ---
    public class SerializedPosition : UdonSharpBehaviour
    {
        public static SerializedPosition New(ulong[] board, ulong metadata) 
        {
            var buff = new object[] { board, metadata };
            return (SerializedPosition)(object)(buff);
        }
    }

    public static class SerializedPositionExt
    {
        public static ulong[] Board(this SerializedPosition sp) { return (ulong[])(((object[])(object)sp)[0]); }
        public static void SetBoard(this SerializedPosition sp, ulong[] val) { ((object[])(object)sp)[0] = val; }

        public static ulong Metadata(this SerializedPosition sp) { return (ulong)(((object[])(object)sp)[1]); }
        public static void SetMetadata(this SerializedPosition sp, ulong val) { ((object[])(object)sp)[1] = val; }
    }
}

