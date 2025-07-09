using UdonSharp;
using UnityEngine;

namespace jp.ootr.chess.Tests
{
    public class ChessGameIntegrationTests : UdonSharpBehaviour
    {
        public Chess chessGame;
        private string[][] moves;
        private int round = 0;

        public void Start()
        {
            chessGame.Initialize();       // ゲームを初期化

            moves = new string[][] {
                // Move 1
                new string[] {"e2", "e4"}, new string[] {"e7", "e5"},
                // Move 2
                new string[] {"g1", "f3"}, new string[] {"d7", "d6"},
                // Move 3
                new string[] {"d2", "d4"}, new string[] {"c8", "g4"},
                // Move 4
                new string[] {"d4", "e5"}, new string[] {"g4", "f3"},
                // Move 5
                new string[] {"d1", "f3"}, new string[] {"d6", "e5"},
                // Move 6
                new string[] {"f1", "c4"}, new string[] {"g8", "f6"},
                // Move 7
                new string[] {"f3", "b3"}, new string[] {"d8", "e7"},
                // Move 8
                new string[] {"b1", "c3"}, new string[] {"c7", "c6"},
                // Move 9
                new string[] {"c1", "g5"}, new string[] {"b7", "b5"},
                // Move 10
                new string[] {"c3", "b5"}, new string[] {"c6", "b5"},
                // Move 11
                new string[] {"c4", "b5"}, new string[] {"b8", "d7"},
                // Move 12
                new string[] {"e1", "c1"}, new string[] {"a8", "d8"}, // White: O-O-O (Queenside castling)
                // Move 13
                new string[] {"d1", "d7"}, new string[] {"d8", "d7"},
                // Move 14
                new string[] {"h1", "d1"}, new string[] {"e7", "e6"},
                // Move 15
                new string[] {"b5", "d7"}, new string[] {"f6", "d7"},
                // Move 16
                new string[] {"b3", "b8"}, new string[] {"d7", "b8"},
                // Move 17
                new string[] {"d1", "d8"} // Final move, white wins (Rd8#)
            };
            SendCustomEventDelayedSeconds(nameof(UpdateRound), 1f);
        }

        public void UpdateRound()
        {
            var move = moves[round];
            string fromPos = move[0];
            string toPos = move[1];

            var result = chessGame.MakeMove(fromPos, toPos, PromotionPiece.Queen);
                
            // デバッグ用に指し手ごとの情報を出力する場合 (任意)
            // UnityEngine.Debug.Log($"Move {i + 1}: {fromPos}-{toPos}. Player: {chessGame.currentPlayer}. Success: {result.Success()}. Message: {result.Message()}");
            // chessGame.DisplayBoard();

            if (!result.Success())
            {
                chessGame.DisplayBoard();
                Debug.LogError($"Move {round + 1} ({fromPos}-{toPos}) by player {chessGame.currentPlayer} failed: {result.Message()}");
            }
            else
            {
                chessGame.DisplayBoard();
                Debug.Log(result.Message());
            }

            if (chessGame.gameOver)
            {
                chessGame.DisplayBoard();
                Debug.Log($"Game over! Winner: {chessGame.winner}");
                return;
            }
            
            round++;
            if (round < moves.Length)
            {
                SendCustomEventDelayedSeconds(nameof(UpdateRound), 0.5f);
            }
            else
            {
                Debug.Log("All moves completed.");
            }
        }
    }
}
