using Microsoft.VisualStudio.TestTools.UnitTesting;
using TriviaBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot.Tests
{
    [TestClass()]
    public class TableTests
    {
        [TestMethod()]
        public void AddRowTest()
        {
            Database db = new Database();
            db.Scores.AddRow(new UserLifetimeScoreModel { GuildId = 0, PlayerId = 0, Score = 10 });
        }

        [TestMethod()]
        public void GetRowTest()
        {
            Database db = new Database();
            var row = db.Scores.GetRow(0);
            Assert.Equals(row.GuildId, 0);
            Assert.Equals(row.PlayerId, 0);
            Assert.Equals(row.Score, 10);
        }
    }
}