namespace CheckersGameLogic
{
    public class Move
    {
        private readonly BoardLocation r_Origin;
        private readonly BoardLocation r_Destination;
        private bool m_IsJumpMove;

        public Move(BoardLocation i_Origin, BoardLocation i_Destination)
        {
            r_Origin = i_Origin;
            r_Destination = i_Destination;
            m_IsJumpMove = false;
        }

        public BoardLocation Origin
        {
            get
            {
                return r_Origin;
            }
        }

        public BoardLocation Destination
        {
            get
            {
                return r_Destination;
            }
        }

        public bool IsJumpMove
        {
            get
            {
                return m_IsJumpMove;
            }
            set
            {
                m_IsJumpMove = value;
            }
        }
    }
}