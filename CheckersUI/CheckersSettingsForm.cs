using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CheckersGameLogic;

namespace CheckersUI
{
    public class CheckersSettingsForm : Form
    {
        private RadioButton m_Radio6x6;
        private RadioButton m_Radio8x8;
        private RadioButton m_Radio10x10;
        private TextBox m_Player1TextBox;
        private TextBox m_Player2TextBox;
        private CheckBox m_EnablePlayer2CheckBox;
        private Button m_DoneButton;

        public static GameBoardManager GameBoard { get; private set; }

        public CheckersSettingsForm()
        {
            this.Text = "Game Settings";
            this.ClientSize = new Size(350, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackgroundImage = Properties.Resources.CheckersIcon;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.Icon = Properties.Resources.Icon;

            initializeComponents();
        }

        private void initializeComponents()
        {
            initializeBoardSizeGroup();
            initializePlayersGroup();
            initializeDoneButton();
        }

        private void initializeBoardSizeGroup()
        {
            GroupBox boardSizeGroupBox = new GroupBox
            {
                Text = "Board Size",
                Location = new Point(20, 50),
                Size = new Size(280, 80),
                BackColor = Color.Transparent
            };
            boardSizeGroupBox.Paint += drawTextWithOutline;
            this.Controls.Add(boardSizeGroupBox);

            m_Radio6x6 = createRadioButton("6 x 6", new Point(20, 30), true);
            m_Radio8x8 = createRadioButton("8 x 8", new Point(100, 30), false);
            m_Radio10x10 = createRadioButton("10 x 10", new Point(180, 30), false);

            boardSizeGroupBox.Controls.Add(m_Radio6x6);
            boardSizeGroupBox.Controls.Add(m_Radio8x8);
            boardSizeGroupBox.Controls.Add(m_Radio10x10);
        }

        private void initializePlayersGroup()
        {
            GroupBox playersGroupBox = new GroupBox
            {
                Text = "Players",
                Location = new Point(20, 140),
                Size = new Size(280, 110),
                BackColor = Color.Transparent
            };
            playersGroupBox.Paint += drawTextWithOutline;
            this.Controls.Add(playersGroupBox);

            m_Player1TextBox = createTextBox(new Point(100, 30), 150);
            playersGroupBox.Controls.Add(m_Player1TextBox);

            Label player1Label = createLabel("Player 1:", new Point(20, 35));
            playersGroupBox.Controls.Add(player1Label);

            m_Player2TextBox = createTextBox(new Point(100, 65), 150);
            playersGroupBox.Controls.Add(m_Player2TextBox);

            m_EnablePlayer2CheckBox = createCheckBox("Player 2:", new Point(20, 70));
            m_Player2TextBox.Enabled = false;
            m_Player2TextBox.Text = "[Computer]";
            m_EnablePlayer2CheckBox.CheckedChanged += enablePlayer2CheckBox_CheckedChanged;
            playersGroupBox.Controls.Add(m_EnablePlayer2CheckBox);
        }

        private void initializeDoneButton()
        {
            m_DoneButton = new Button
            {
                Text = "Done",
                Location = new Point(120, 270),
                Width = 100
            };
            m_DoneButton.Click += doneButton_Click;
            this.Controls.Add(m_DoneButton);
        }

        private RadioButton createRadioButton(string i_Text, Point i_Location, bool i_IsChecked)
        {
            return new RadioButton
            {
                Text = i_Text,
                Location = i_Location,
                AutoSize = true,
                Checked = i_IsChecked,
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold)
            };
        }

        private Label createLabel(string i_Text, Point i_Location)
        {
            return new Label
            {
                Text = i_Text,
                Location = i_Location,
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
        }

        private CheckBox createCheckBox(string i_Text, Point i_Location)
        {
            return new CheckBox
            {
                Text = i_Text,
                Location = i_Location,
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
        }

        private TextBox createTextBox(Point i_Location, int i_Width)
        {
            return new TextBox
            {
                Location = i_Location,
                Width = i_Width,
                Enabled = true,
                ReadOnly = false,
                Text = string.Empty
            };
        }

        private void drawTextWithOutline(object sender, PaintEventArgs e)
        {
            GroupBox groupBox = sender as GroupBox;

            if (groupBox != null)
            {
                using (Graphics g = e.Graphics)
                {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddString(
                            groupBox.Text,
                            groupBox.Font.FontFamily,
                            (int)groupBox.Font.Style,
                            g.DpiY * groupBox.Font.SizeInPoints / 72,
                            new Point(10, -1),
                            StringFormat.GenericDefault
                        );

                        using (Pen pen = new Pen(Color.Black, 3))
                        {
                            g.DrawPath(pen, path);
                        }

                        using (Brush brush = new SolidBrush(Color.White))
                        {
                            g.FillPath(brush, path);
                        }
                    }
                }
            }
        }

        private void enablePlayer2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (m_EnablePlayer2CheckBox.Checked)
            {
                m_Player2TextBox.Enabled = true;
                m_Player2TextBox.Text = string.Empty;
            }
            else
            {
                m_Player2TextBox.Enabled = false;
                m_Player2TextBox.Text = "[Computer]";
            }
        }

        private void doneButton_Click(object sender, EventArgs e)
        {
            eBoardSize boardSize = m_Radio6x6.Checked ? eBoardSize.Small :
                                   m_Radio8x8.Checked ? eBoardSize.Medium :
                                   eBoardSize.Large;

            string player1Name = m_Player1TextBox.Text;
            string player2Name = m_EnablePlayer2CheckBox.Checked ? m_Player2TextBox.Text : "Computer";

            GameBoard = new GameBoardManager(boardSize, player1Name, player2Name);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}