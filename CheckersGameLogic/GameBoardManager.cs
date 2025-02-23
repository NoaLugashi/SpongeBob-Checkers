using System;
using System.Collections.Generic;

namespace CheckersGameLogic
{
    public class GameBoardManager
    {
        private readonly eBoardSize r_BoardSize;
        private readonly Cell[,] r_Board;
        private readonly Player r_FirstPlayer;
        private readonly Player r_SecondPlayer;
        private readonly Player[] r_PlayersReferencesArray;
        private readonly AI r_AI = null;
        private Player m_WinnerPlayer;
        private Player m_LastPlayer;
        private Player m_PlayerTurn;
        private bool m_IsGameFinished;

        public GameBoardManager(eBoardSize i_BoardSize, String i_FirstPlayerName, String i_SecondPlayerName)
        {
            if (i_SecondPlayerName == "Computer")
                r_AI = new AI(this);

            this.r_BoardSize = i_BoardSize;
            this.r_Board = new Cell[(int)this.r_BoardSize, (int)this.r_BoardSize];
            this.r_FirstPlayer = new Player(i_FirstPlayerName, 'X', 'K');
            this.r_SecondPlayer = new Player(i_SecondPlayerName, 'O', 'U');
            this.r_PlayersReferencesArray = new Player[] { r_FirstPlayer, r_SecondPlayer };
            
            initializeGame();
            initializeBoard();
        }

        private void initializeGame()
        {
            this.m_PlayerTurn = r_FirstPlayer;
            this.m_LastPlayer = this.r_FirstPlayer;
            this.m_IsGameFinished = false;
            this.m_WinnerPlayer = null;
            this.r_FirstPlayer.InitializePlayer();
            this.r_SecondPlayer.InitializePlayer();
        }

        private void initializeBoard()
        {
            int firstSeparateRowIndex = ((int)this.r_BoardSize - 2) / 2;
            int secondSeparateRowIndex = firstSeparateRowIndex + 1;
            BoardLocation currentCellPosition;
            Piece currentCheckerPiece;

            for (int rowIndex = 0; rowIndex < (int)this.r_BoardSize; ++rowIndex)
            {
                for (int columnIndex = 0; columnIndex < (int)this.r_BoardSize; ++columnIndex)
                {
                    currentCellPosition = new BoardLocation(rowIndex, columnIndex);
                    currentCheckerPiece = null;

                    if (isAlternatingCell(rowIndex, columnIndex))
                    {
                        if (rowIndex < firstSeparateRowIndex)
                        {
                            currentCheckerPiece = new Piece(this.r_SecondPlayer, ePieceType.Regular, currentCellPosition);
                            r_SecondPlayer.AddPieceToPlayerListOfPieces(currentCheckerPiece);
                        }
                        else if (rowIndex > secondSeparateRowIndex)
                        {
                            currentCheckerPiece = new Piece(this.r_FirstPlayer, ePieceType.Regular, currentCellPosition);
                            r_FirstPlayer.AddPieceToPlayerListOfPieces(currentCheckerPiece);
                        }
                    }

                    this.r_Board[rowIndex, columnIndex] = new Cell(currentCellPosition, currentCheckerPiece);
                }
            }
        }

        private bool isAlternatingCell(int i_RowIndex, int i_ColumnIndex)
        {
            return (i_RowIndex % 2 == 0 && i_ColumnIndex % 2 != 0) || (i_RowIndex % 2 != 0 && i_ColumnIndex % 2 == 0);
        }

        public void EndGame()
        {
            if (this.OpponentPlayer.IsPiecesListEmpty() || isOpponentBlocked())
            {
                this.m_WinnerPlayer = this.CurrentPlayer;
                this.m_WinnerPlayer.Score += calculateScoresToAdd();
            }
            else if (!this.IsGameFinished)
            {
                this.m_LastPlayer = this.CurrentPlayer;
                this.m_WinnerPlayer = this.OpponentPlayer;
                this.OpponentPlayer.Score += calculateScoresToAdd();
                this.m_IsGameFinished = true;
            }
            else
            {
                this.m_WinnerPlayer = null;
            }
        }

        private bool isOpponentBlocked()
        {
            return !this.CurrentPlayer.IsPossibleMovesListEmpty() && this.OpponentPlayer.IsPossibleMovesListEmpty();
        }

        public void RestartGame()
        {
            initializeGame();
            initializeBoard();
        }

        private int calculateScoresToAdd()
        {
            return Math.Abs((this.CurrentPlayer.RegularPiecesCounter + (this.CurrentPlayer.KingsCounter * 4))
                          - (this.OpponentPlayer.RegularPiecesCounter + (this.OpponentPlayer.KingsCounter * 4))); //מלך שווה פי 4 מחייל פשוט
        }

        public Move ActivateComputerMove(BoardLocation i_SpecificPieceLocation = default)
        {
            Move selectedMove = null;

            if (!i_SpecificPieceLocation.Equals(default(BoardLocation)))
            {
                selectedMove = r_AI.GetBestMoveFromLocation(i_SpecificPieceLocation);
            }
            else
            {
                selectedMove = r_AI.GetBestMove();
            }

            if (selectedMove != null)
            {
                makeMove(selectedMove);
            }

            return selectedMove;
        }

        public bool TryMove(String i_StartPositionAsString, String i_EndPositionAsString)
        {
            bool isMoveSucceed = false;
            BoardLocation startPosition = convertStringToPosition(i_StartPositionAsString);
            BoardLocation endPosition = convertStringToPosition(i_EndPositionAsString);
            Move playerMove = new Move(startPosition, endPosition);

            if (isMoveValid(playerMove, this.CurrentPlayer))
            {
                if(!playerMove.IsJumpMove || (playerMove.IsJumpMove && isMoveInCaptureMovesList(playerMove, this.CurrentPlayer)))
                {
                    makeMove(playerMove);
                    isMoveSucceed = true;
                }
            }

            return isMoveSucceed;
        }

        private void makeMove(Move i_PlayerMove)
        {
            int startPositionRowIndex = i_PlayerMove.Origin.Row;
            int startPositionColumnIndex = i_PlayerMove.Origin.Column;
            int endPositionRowIndex = i_PlayerMove.Destination.Row;
            int endPositionColumnIndex = i_PlayerMove.Destination.Column;
            
            this.r_Board[endPositionRowIndex, endPositionColumnIndex].Piece = this.r_Board[startPositionRowIndex, startPositionColumnIndex].Piece;
            this.r_Board[startPositionRowIndex, startPositionColumnIndex].Clear();
            checkIfShouldChangePieceState(this.r_Board[endPositionRowIndex, endPositionColumnIndex].Piece);

            if (i_PlayerMove.IsJumpMove)
            {
                capturePiece(i_PlayerMove);
                updatePlayersPossibleAndCaptureMovesLists(i_PlayerMove);

                if (!this.CurrentPlayer.IsCaptureMovesListEmpty())
                {
                    if (this.CurrentPlayer.IsComputer)
                    {
                        ActivateComputerMove(i_PlayerMove.Destination);
                    }

                    return;
                }
            }

            checkIfGameEnds(i_PlayerMove);
            switchTurn();
        }

        private bool checkIfGameEnds(Move i_LastMove)
        {
            updatePlayersPossibleAndCaptureMovesLists(i_LastMove);
            this.m_IsGameFinished = this.OpponentPlayer.IsPiecesListEmpty() || 
                                    (this.OpponentPlayer.IsPossibleMovesListEmpty() && this.OpponentPlayer.IsCaptureMovesListEmpty());
            if (m_IsGameFinished)
            {
                this.EndGame();
            }

            return this.m_IsGameFinished;
        }

        private void updatePlayersPossibleAndCaptureMovesLists(Move i_LastMove)
        {
            foreach (Player player in this.r_PlayersReferencesArray)
            {
                player.ClearPossibleMovesList();
                player.ClearCaptureMovesList();

                if(player == this.CurrentPlayer && i_LastMove.IsJumpMove)
                {
                    updatePlayerCapturingMovesPerPiece(player, i_LastMove.Destination);
                }
                else
                {
                    updateAllPlayerCapturingMoves(player);
                }

                if (player.IsCaptureMovesListEmpty())
                {
                    updateAllPlayerPossibleMoves(player);
                }
            }
        }

        private void updateAllPlayerCapturingMoves(Player i_Player)
        {
            foreach (Piece playerPiece in i_Player.PlayerPiecesList)
            {
                BoardLocation startPosition = playerPiece.PiecePosition;
                List<BoardLocation> potentialCaptureMovesPositions = getPotentialPositions(playerPiece, startPosition, 1);

                foreach (BoardLocation endPosition in potentialCaptureMovesPositions)
                {
                    Move potentialCaptureMove = new Move(startPosition, endPosition);

                    if (isMoveValid(potentialCaptureMove, i_Player))
                    {
                        i_Player.AppendMoveToCaptureMovesList(potentialCaptureMove);
                    }
                }
            }
        }

        private void updatePlayerCapturingMovesPerPiece(Player i_Player, BoardLocation i_LastMoveEndPosition)
        {
            BoardLocation startPosition = i_LastMoveEndPosition;
            Piece currentCheckerPiece = this.r_Board[startPosition.Row, startPosition.Column].Piece;
            List<BoardLocation> potentialCaptureMovesPositions = getPotentialPositions(currentCheckerPiece, startPosition, 1);

            foreach (BoardLocation endPosition in potentialCaptureMovesPositions)
            {
                Move potentialCaptureMove = new Move(startPosition, endPosition);

                if (isMoveValid(potentialCaptureMove, i_Player))
                {
                    i_Player.AppendMoveToCaptureMovesList(potentialCaptureMove);
                }
            }
        }

        private void updateAllPlayerPossibleMoves(Player i_Player)
        {
            foreach (Piece playerPiece in i_Player.PlayerPiecesList)
            {
                BoardLocation startPosition = playerPiece.PiecePosition;
                List<BoardLocation> potentialMovesPositions = getPotentialPositions(playerPiece, startPosition, 0);

                foreach (BoardLocation endPosition in potentialMovesPositions)
                {
                    Move potentialPossibleMove = new Move(startPosition, endPosition);

                    if (isMoveValid(potentialPossibleMove, i_Player))
                    {
                        i_Player.AppendMoveToPossibleMovesList(potentialPossibleMove);
                    }
                }
            }
        }

        private void switchTurn()
        {
            this.m_PlayerTurn = this.OpponentPlayer;
        }

        private void capturePiece(Move i_PlayerMove)
        {
            int middleRowIndex = (i_PlayerMove.Origin.Row + i_PlayerMove.Destination.Row) / 2;
            int middleColumnIndex = (i_PlayerMove.Origin.Column + i_PlayerMove.Destination.Column) / 2;
            Piece capturedPiece = this.r_Board[middleRowIndex, middleColumnIndex].Piece;

            if (capturedPiece != null)
            {
                this.OpponentPlayer.RemovePieceFromPlayerListOfPieces(capturedPiece);
                this.r_Board[middleRowIndex, middleColumnIndex].Clear();
            }
        }

        private List<BoardLocation> getPotentialPositions(Piece i_Piece, BoardLocation i_StartPosition, int i_Offset)
        {
            List<BoardLocation> potentialPositions = new List<BoardLocation>();

            if (i_Piece.PieceType == ePieceType.King)
            {
                potentialPositions.Add(getLeftUpDiagonal(i_StartPosition, i_Offset));
                potentialPositions.Add(getRightUpDiagonal(i_StartPosition, i_Offset));
                potentialPositions.Add(getLeftDownDiagonal(i_StartPosition, i_Offset));
                potentialPositions.Add(getRightDownDiagonal(i_StartPosition, i_Offset));
            }
            else if (i_Piece.AssignedPlayer == this.r_FirstPlayer)
            {
                potentialPositions.Add(getLeftUpDiagonal(i_StartPosition, i_Offset));
                potentialPositions.Add(getRightUpDiagonal(i_StartPosition, i_Offset));
            }
            else
            {
                potentialPositions.Add(getRightDownDiagonal(i_StartPosition, i_Offset));
                potentialPositions.Add(getLeftDownDiagonal(i_StartPosition, i_Offset));
            }

            return potentialPositions;
        }

        private BoardLocation getLeftUpDiagonal(BoardLocation i_StartPosition, int i_Offset)
        {
            return new BoardLocation(i_StartPosition.Row - 1 - i_Offset, i_StartPosition.Column - 1 - i_Offset);
        }

        private BoardLocation getRightUpDiagonal (BoardLocation i_StartPosition, int i_Offset)
        {
            return new BoardLocation(i_StartPosition.Row - 1 - i_Offset, i_StartPosition.Column + 1 + i_Offset);
        }

        private BoardLocation getLeftDownDiagonal(BoardLocation i_StartPosition, int i_Offset)
        {
            return new BoardLocation(i_StartPosition.Row + 1 + i_Offset, i_StartPosition.Column - 1 - i_Offset);
        }

        private BoardLocation getRightDownDiagonal(BoardLocation i_StartPosition, int i_Offset)
        {
            return new BoardLocation(i_StartPosition.Row + 1 + i_Offset, i_StartPosition.Column + 1 + i_Offset);
        }

        private void checkIfShouldChangePieceState(Piece i_CheckerPiece)
        {
            if (i_CheckerPiece.PieceType != ePieceType.King)
            {
                if (i_CheckerPiece.AssignedPlayer == this.r_FirstPlayer && i_CheckerPiece.PiecePosition.Row == 0)
                {
                    i_CheckerPiece.PieceType = ePieceType.King;
                    this.r_FirstPlayer.KingsCounter++;
                }
                else if (i_CheckerPiece.AssignedPlayer == this.r_SecondPlayer && i_CheckerPiece.PiecePosition.Row == (int)r_BoardSize - 1)
                {
                    i_CheckerPiece.PieceType = ePieceType.King;
                    this.r_SecondPlayer.KingsCounter++;
                }
            }
        }

        private bool isInBounds(Move i_PlayerMove)
        {
            return i_PlayerMove.Origin.Row >= 0 && i_PlayerMove.Origin.Row < (int)this.r_BoardSize
                && i_PlayerMove.Origin.Column >= 0 && i_PlayerMove.Origin.Column < (int)this.r_BoardSize
                && i_PlayerMove.Destination.Row >= 0 && i_PlayerMove.Destination.Row < (int)this.r_BoardSize
                && i_PlayerMove.Destination.Column >= 0 && i_PlayerMove.Destination.Column < (int)this.r_BoardSize;
        }

        private bool isMoveValid(Move i_PlayerMove, Player i_CurrentPlayer)
        {
            bool isValid = false;

            if (isInBounds(i_PlayerMove))
            {
                BoardLocation moveStartPosition = i_PlayerMove.Origin;
                BoardLocation moveEndPosition = i_PlayerMove.Destination;
                Cell moveStartCell = this.r_Board[moveStartPosition.Row, moveStartPosition.Column];
                Cell moveEndCell = this.r_Board[moveEndPosition.Row, moveEndPosition.Column];
                int columnDifference = Math.Abs(moveStartPosition.Column - moveEndPosition.Column);
                int rowDifference;

                if (!moveStartCell.IsEmpty() && moveEndCell.IsEmpty() && moveStartCell.Piece.AssignedPlayer == i_CurrentPlayer)
                {
                    if (moveStartCell.Piece.PieceType == ePieceType.King)
                    {
                        rowDifference = Math.Abs(moveStartPosition.Row - moveEndPosition.Row);
                    }
                    else
                    {
                        if (i_CurrentPlayer == this.r_FirstPlayer)
                        {
                            rowDifference = moveStartPosition.Row - moveEndPosition.Row;
                        }
                        else
                        {
                            rowDifference = moveEndPosition.Row - moveStartPosition.Row;
                        }
                    }

                    isValid = rowDifference == columnDifference &&
                        ((rowDifference == 1 && i_CurrentPlayer.IsCaptureMovesListEmpty()) ||
                         (rowDifference == 2 && shouldCapture(i_PlayerMove, i_CurrentPlayer)));
                }
            }

            return isValid;
        }

        private bool shouldCapture(Move i_PlayerMove, Player i_CurrentPlayer)
        {
            BoardLocation moveStartPosition = i_PlayerMove.Origin;
            BoardLocation moveEndPosition = i_PlayerMove.Destination;
            int middleRowIndex = (moveStartPosition.Row + moveEndPosition.Row) / 2;
            int middleColumnIndex = (moveStartPosition.Column + moveEndPosition.Column) / 2;
            Cell middleCell = this.r_Board[middleRowIndex, middleColumnIndex];

            i_PlayerMove.IsJumpMove = !middleCell.IsEmpty() && middleCell.Piece.AssignedPlayer != i_CurrentPlayer;

            return i_PlayerMove.IsJumpMove;
        }

        private bool isMoveInCaptureMovesList(Move i_PlayerMove, Player i_CurrentPlayer)
        {
            bool isMoveInList = false;

            foreach(Move playerCaptureMove in i_CurrentPlayer.PlayerCaptureMovesList)
            {
                isMoveInList = i_PlayerMove.Origin.Row == playerCaptureMove.Origin.Row &&  
                               i_PlayerMove.Origin.Column == playerCaptureMove.Origin.Column;

                if (isMoveInList)
                {
                    break;
                }
            }

            return isMoveInList;
        }

        private BoardLocation convertStringToPosition(String i_PositionAsString)
        {
            int cellRowIndex = i_PositionAsString[0] - 'A';
            int cellColumnIndex = i_PositionAsString[1] - 'a';

            return new BoardLocation(cellRowIndex, cellColumnIndex);
        }

        public Cell[,] Board
        {
            get
            {
                return this.r_Board;
            }
        }

        public eBoardSize BoardSize
        {
            get
            {
                return this.r_BoardSize;
            }
        }

        public Player FirstPlayer
        {
            get
            {
                return this.r_FirstPlayer;
            }
        }

        public Player SecondPlayer
        {
            get
            {
                return this.r_SecondPlayer;
            }
        }

        public Player CurrentPlayer
        {
            get
            {
                return this.m_PlayerTurn;
            }
        }

        public Player OpponentPlayer
        {
            get
            {
                return this.m_PlayerTurn == this.r_FirstPlayer ? this.r_SecondPlayer : this.r_FirstPlayer;
            }
        }

        public Player WinnerPlayer
        {
            get
            {
                return this.m_WinnerPlayer;
            }
            set
            {
                this.m_WinnerPlayer = value;
            }
        }

        public bool IsGameFinished
        {
            get
            {
                return this.m_IsGameFinished;
            }
            set
            {
                this.m_IsGameFinished = value;
            }
        }
    }
}