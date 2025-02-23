using System;
using System.Collections.Generic;

namespace CheckersGameLogic
{
    public class AI
    {
        private readonly GameBoardManager r_GameBoard;

        public AI(GameBoardManager i_GameBoard)
        {
            r_GameBoard = i_GameBoard;
        }

        public Move GetBestMoveFromLocation(BoardLocation i_Location)
        {
            Move selectedMove = null;

            List<Move> captureMoves = r_GameBoard.CurrentPlayer.PlayerCaptureMovesList
                .FindAll(move => move.Origin.Equals(i_Location));

            if (captureMoves.Count > 0)
            {
                selectedMove = getBestCaptureMove(captureMoves);
            }

            return selectedMove;
        }

        public Move GetBestMove()
        {
            List<Move> captureMoves = r_GameBoard.CurrentPlayer.PlayerCaptureMovesList;
            List<Move> possibleMoves = r_GameBoard.CurrentPlayer.PlayerPossibleMovesList;

            if (captureMoves.Count > 0)
            {
                return getBestCaptureMove(captureMoves);
            }

            return getBestRegularMove(possibleMoves);
        }

        private Move getBestCaptureMove(List<Move> i_CaptureMoves)
        {
            Move bestMove = null;
            int maxScore = int.MinValue;

            foreach (Move move in i_CaptureMoves)
            {
                int moveScore = evaluateMove(move);

                if (moveScore > maxScore)
                {
                    maxScore = moveScore;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private Move  getBestRegularMove(List<Move> i_PossibleMoves)
        {
            Move bestMove = null;
            int maxScore = int.MinValue;

            foreach (Move move in i_PossibleMoves)
            {
                int moveScore = evaluateMove(move);

                if (moveScore > maxScore)
                {
                    maxScore = moveScore;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private int evaluateMove(Move i_Move)
        {
            GameBoardManager simulatedBoard = cloneBoard(r_GameBoard);
            simulatedBoard.TryMove(i_Move.Origin.ToString(), i_Move.Destination.ToString());

            int score = simulatedBoard.CurrentPlayer.Score - simulatedBoard.OpponentPlayer.Score;

            if (isPromotionMove(i_Move))
            {
                score += 50;
            }

            if (i_Move.IsJumpMove)
            {
                score += 20;
            }

            if (isPieceInDangerAfterMove(simulatedBoard, i_Move))
            {
                score -= 30;
            }

            return score;
        }

        private bool isPromotionMove(Move i_Move)
        {
            return (i_Move.Destination.Row == 0 && r_GameBoard.CurrentPlayer == r_GameBoard.FirstPlayer) ||
                   (i_Move.Destination.Row == (int)r_GameBoard.BoardSize - 1 && r_GameBoard.CurrentPlayer == r_GameBoard.SecondPlayer);
        }

        private bool isPieceInDangerAfterMove(GameBoardManager i_SimulatedBoard, Move i_Move)
        {
            BoardLocation newPosition = i_Move.Destination;
            bool isInDanger = false;
            foreach (Move opponentMove in i_SimulatedBoard.OpponentPlayer.PlayerCaptureMovesList)
            {
                if (opponentMove.Destination.Equals(newPosition))
                {
                    isInDanger = true;
                }
            }

            return isInDanger;
        }

        private GameBoardManager cloneBoard(GameBoardManager i_Original)
        {
            return new GameBoardManager(i_Original.BoardSize, i_Original.FirstPlayer.PlayerName, i_Original.SecondPlayer.PlayerName);
        }
    }
}