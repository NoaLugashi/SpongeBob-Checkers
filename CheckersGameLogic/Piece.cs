namespace CheckersGameLogic
{
    public class Piece
    {
        private readonly Player r_AssignedPlayer;
        private ePieceType m_PieceType;
        private BoardLocation m_PiecePosition;

        public Piece(Player i_AssignedPlayer, ePieceType i_TokenType, BoardLocation i_TokenPosition)
        {
            r_AssignedPlayer = i_AssignedPlayer;
            m_PieceType = i_TokenType;
            m_PiecePosition = i_TokenPosition;
        }

        public Player AssignedPlayer
        {
            get
            {
                return r_AssignedPlayer;
            }
        }

        public ePieceType PieceType
        {
            get
            {
                return m_PieceType;
            }
            set
            {
                m_PieceType = value;
            }
        }

        public BoardLocation PiecePosition
        {
            get
            {
                return m_PiecePosition;
            }
            set
            {
                m_PiecePosition = value;
            }
        }
    }
}