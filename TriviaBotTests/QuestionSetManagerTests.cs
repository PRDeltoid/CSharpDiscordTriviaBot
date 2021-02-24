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
    public class QuestionSetManagerTests
    {
        [TestMethod()]
        public void GetNewQuestionSetTest()
        {
            var qsm = new QuestionSetManager();
            //qsm.GetNewQuestionSet(10, null);
        }
    }
}