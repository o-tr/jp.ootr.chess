using UdonSharp;
using VRC.SDKBase;

namespace jp.ootr.chess
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Chess : ChessObjectPool
    {
        [UdonSynced] private ulong board0;
        [UdonSynced] private ulong board1;
        [UdonSynced] private ulong board2;
        [UdonSynced] private ulong board3;

        [UdonSynced] private ulong metadata;
        
        public void Start()
        {
            Startup();
        }

        private void Startup()
        {
            if (board0 == 0 && board1 == 0 && board2 == 0 && board3 == 0)
            {
                ResetAndInitialize();
            }
            else
            {
                _OnDeserialization();
            }
        }

        public void ResetAndInitialize()
        {
            Initialize();
            UpdateBoardCells();
            UpdateBoardSelection();
            UpdateStatusText();
        }

        public override MoveResult MakeMove(string fromPos, string toPos, PromotionPiece promotionPiece)
        {
            var result =  base.MakeMove(fromPos, toPos, promotionPiece);
            var serialized = SerializePosition();
            var board = serialized.Board();
            board0 = board[0];
            board1 = board[1];
            board2 = board[2];
            board3 = board[3];

            metadata = serialized.Metadata();
            
            Sync();
            
            return result;
        }

        public override void _OnDeserialization()
        {
            base._OnDeserialization();
            if (!Utilities.IsValid(_board))
            {
                _board = BoardData.New();
            }
            var board = new ulong[] { board0, board1, board2, board3 };
            var serialized = SerializedPosition.New(board, metadata);
            DeserializePosition(serialized);
            UpdateBoardCells();
            UpdateBoardSelection();
            UpdateStatusText();
        }
    }
}
