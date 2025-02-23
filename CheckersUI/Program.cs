using System;
using System.Windows.Forms;

namespace CheckersUI
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            CheckersSettingsForm settingsForm = new CheckersSettingsForm();

            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                CheckersBoardForm gameForm = new CheckersBoardForm(CheckersSettingsForm.GameBoard);
                Application.Run(gameForm);
            }
        }
    }
}
