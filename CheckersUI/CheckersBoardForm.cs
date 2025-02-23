using System;
using System.Drawing;
using System.Windows.Forms;
using CheckersGameLogic;

namespace CheckersUI
{
    public class CheckersBoardForm : Form
    {
        private readonly GameBoardManager r_GameBoard;
        private readonly Button[,] r_BoardButtons;
        private Label m_CurrentPlayerLabel;
        private Label m_Player1ScoreLabel;
        private Label m_Player2ScoreLabel;
        private string m_StartPosition = null;

        public CheckersBoardForm(GameBoardManager i_GameBoard)
        {
            r_GameBoard = i_GameBoard;
            r_BoardButtons = new Button[(int)r_GameBoard.BoardSize, (int)r_GameBoard.BoardSize];
            this.Text = "Checkers Game";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = true;
            this.ClientSize = new Size(700, 500);
            this.Resize += checkersBoardForm_Resize;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackgroundImage = Properties.Resources.BackroundImage;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.Icon = Properties.Resources.Icon;

            initializeComponents();
            this.Size = new Size((int)r_GameBoard.BoardSize * 70 + 20, (int)r_GameBoard.BoardSize * 70 + 150);
            checkersBoardForm_Resize(this, EventArgs.Empty);
        }

        private void initializeComponents()
        {
            initializeLabels();
            initializeBoardGroupBox();

            updateBoard();
        }

        private void initializeLabels()
        {
            m_Player1ScoreLabel = new Label
            {
                Text = $"{r_GameBoard.FirstPlayer.PlayerName}: 0",
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            this.Controls.Add(m_Player1ScoreLabel);

            m_Player2ScoreLabel = new Label
            {
                Text = $"{r_GameBoard.SecondPlayer.PlayerName}: 0",
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            this.Controls.Add(m_Player2ScoreLabel);

            m_CurrentPlayerLabel = new Label
            {
                Text = $"Current Player: {r_GameBoard.CurrentPlayer.PlayerName}",
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Visible = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(m_CurrentPlayerLabel);
        }

        private void initializeBoardGroupBox()
        {
            GroupBox boardGroupBox = new GroupBox
            {
                Text = "SpongeBob Checkers Board",
                Location = new Point(20, 50),
                Size = new Size(60 * (int)r_GameBoard.BoardSize, 60 * (int)r_GameBoard.BoardSize),
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill
            };
            this.Controls.Add(boardGroupBox);

            int boardSize = (int)r_GameBoard.BoardSize;

            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    Button button = new Button
                    {
                        Size = new Size(50, 50),
                        Location = new Point(10 + (50 * col), 20 + (50 * row)),
                        BackColor = (row + col) % 2 == 0 ? Color.Gray : Color.White,
                        Tag = new BoardLocation(row, col),
                        Enabled = (row + col) % 2 != 0
                    };

                    button.Click += button_Click ;
                    boardGroupBox.Controls.Add(button);
                    r_BoardButtons[row, col] = button;
                }
            }
        }

        private void checkersBoardForm_Resize(object i_sender, EventArgs i_EventArgs)
        {
            int boardSize = (int)r_GameBoard.BoardSize;
            int buttonSize = Math.Min(this.ClientSize.Width / boardSize, (this.ClientSize.Height - 150) / boardSize);

            int boardOffsetX = (this.ClientSize.Width - (buttonSize * boardSize)) / 2;
            int boardOffsetY = 50;

            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    Button button = r_BoardButtons[row, col];
                    button.Size = new Size(buttonSize, buttonSize);
                    button.Location = new Point(boardOffsetX + buttonSize * col, boardOffsetY + buttonSize * row);
                }
            }

            m_Player1ScoreLabel.Location = new Point(boardOffsetX, 15);
            m_Player2ScoreLabel.Location = new Point(this.ClientSize.Width - boardOffsetX - 150, 15);
            m_CurrentPlayerLabel.Location = new Point(boardOffsetX, boardOffsetY + buttonSize * boardSize + 10);
        }

        private void button_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            BoardLocation clickedPosition = (BoardLocation)clickedButton.Tag;
            string clickedPositionAsString = convertPositionToString(clickedPosition);

            if (m_StartPosition == clickedPositionAsString)
            {
                clickedButton.BackColor = (clickedPosition.Row + clickedPosition.Column) % 2 == 0 ? Color.Gray : Color.White;
                m_StartPosition = null;
                return;
            }
            if (m_StartPosition == null)
            {
                m_StartPosition = clickedPositionAsString;
                clickedButton.BackColor = Color.LightBlue;
            }
            else
            {
                string endPosition = clickedPositionAsString;

                if (r_GameBoard.TryMove(m_StartPosition, endPosition))
                {
                    if (r_GameBoard.IsGameFinished)
                    {
                        string winner = r_GameBoard.WinnerPlayer == null
                            ? "Tie!"
                            : $"{r_GameBoard.WinnerPlayer.PlayerName} Won!";
                        DialogResult result = MessageBox.Show($"{winner}\nAnother Round?", "Damka", MessageBoxButtons.YesNo);

                        if (result == DialogResult.Yes)
                        {
                            r_GameBoard.RestartGame();
                            updateBoard();
                        }
                        else
                        {
                            this.Close();
                        }
                    }
                    updateBoard();

                    if (r_GameBoard.IsGameFinished)
                    {
                        string winner = r_GameBoard.WinnerPlayer == null
                            ? "It's a draw!"
                            : $"The winner is {r_GameBoard.WinnerPlayer.PlayerName}!";
                        MessageBox.Show(winner, "Game Over");
                        this.Close();
                    }
                    else
                    {
                        m_CurrentPlayerLabel.Text = $"Current Player: {r_GameBoard.CurrentPlayer.PlayerName}";

                        if (r_GameBoard.CurrentPlayer.IsComputer)
                        {
                            Application.DoEvents();
                            performComputerMove();
                            updateBoard();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Invalid move. Try again.", "Error");
                }

                m_StartPosition = null;
                resetButtonColors();
            }
        }

        private void performComputerMove()
        {
            updateBoard();
            m_CurrentPlayerLabel.Text = $"Current Player: {r_GameBoard.CurrentPlayer.PlayerName}";
            r_GameBoard.ActivateComputerMove();
            updateBoard();
        }

        private string convertPositionToString(BoardLocation i_Position)
        {
            char rowChar = (char)('A' + i_Position.Row);
            char colChar = (char)('a' + i_Position.Column);

            return $"{rowChar}{colChar}";
        }

        private void resetButtonColors()
        {
            foreach (Button button in r_BoardButtons)
            {
                BoardLocation position = (BoardLocation)button.Tag;
                button.BackColor = (position.Row + position.Column) % 2 == 0 ? Color.Gray : Color.White;
            }
        }

        private void updateBoard()
        {
            for (int row = 0; row < r_BoardButtons.GetLength(0); row++)
            {
                for (int col = 0; col < r_BoardButtons.GetLength(1); col++)
                {
                    Piece piece = r_GameBoard.Board[row, col].Piece;
                    Button button = r_BoardButtons[row, col];

                    button.BackgroundImage = null;
                    button.Image = null;
                    button.BackColor = (row + col) % 2 == 0 ? Color.Gray : Color.White;

                    if (piece != null)
                    {
                        button.FlatStyle = FlatStyle.Flat;
                        button.BackgroundImageLayout = ImageLayout.Stretch;

                        button.BackgroundImage = piece.PieceType == ePieceType.King
                            ? piece.AssignedPlayer == r_GameBoard.FirstPlayer ? Properties.Resources.CrabIcon : Properties.Resources.planktonIcon
                            : piece.AssignedPlayer == r_GameBoard.FirstPlayer ? Properties.Resources.SpongebobIcon : Properties.Resources.patrickIcon;
                    }
                }
            }

            updateLabels();
        }

        private void updateLabels()
        {
            m_Player1ScoreLabel.Text = $"{r_GameBoard.FirstPlayer.PlayerName}: {r_GameBoard.FirstPlayer.Score}";
            m_Player2ScoreLabel.Text = $"{r_GameBoard.SecondPlayer.PlayerName}: {r_GameBoard.SecondPlayer.Score}";
            m_CurrentPlayerLabel.Text = $"Current Player: {r_GameBoard.CurrentPlayer.PlayerName}";
        }
    }
}