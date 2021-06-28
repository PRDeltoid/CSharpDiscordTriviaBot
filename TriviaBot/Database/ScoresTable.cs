namespace TriviaBot
{
    public class ScoresTable : DatabaseTable<UserLifetimeScoreModel, ulong>
    {
        public ScoresTable() : base("Scores") { }
    }
}
