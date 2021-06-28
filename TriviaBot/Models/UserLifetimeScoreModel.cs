namespace TriviaBot
{
    public class UserLifetimeScoreModel
    {
        [KeyColumn]
        public ulong UserID { get; set; }
        public uint Score { get; set; }
        public uint Wins { get; set; }
    }
}
