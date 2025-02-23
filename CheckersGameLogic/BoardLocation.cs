namespace CheckersGameLogic
{
    public struct BoardLocation
    {
        private int m_RowIndex;
        private int m_ColumnIndex;

        public BoardLocation(int i_Row, int i_Column)
        {
            m_RowIndex = i_Row;
            m_ColumnIndex = i_Column;
        }

        public int Row
        {
            get
            {
                return m_RowIndex;
            }
            set
            {
                m_RowIndex = value;
            }
        }

        public int Column
        {
            get
            {
                return m_ColumnIndex;
            }
            set
            {
                m_ColumnIndex = value;
            }
        }

        public override string ToString()
        {
            return $"[{Row}, {Column}]";
        }
    }
}