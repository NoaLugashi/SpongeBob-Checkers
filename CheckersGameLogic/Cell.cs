namespace CheckersGameLogic
{
    public class Cell
    {
        private readonly BoardLocation r_CellPosition;
        private Piece m_Piece;

        public Cell(BoardLocation i_CellPosition, Piece i_Piece)
        {
            r_CellPosition = i_CellPosition;
            m_Piece = i_Piece;
        }

        public Piece Piece
        {
            get
            {
                return m_Piece;
            }
            set
            {
                if (value == null)
                {
                    m_Piece = null;
                }
                else
                {
                    m_Piece = value;
                    m_Piece.PiecePosition = r_CellPosition;
                }
            }
        }

        public bool IsEmpty()
        {
            return m_Piece == null;
        }

        public void Clear()
        {
            m_Piece = null;
        }
    }
}