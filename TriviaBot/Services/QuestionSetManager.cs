using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using TriviaBot.Services;

namespace TriviaBot
{
    public class QuestionSetManager : IQuestionSetManager
    {
        #region Members 
        int currentQuestionIndex = 0;
        private readonly TriviaDBQuestionGetterService _questionGetter;

        #endregion

        #region Properties
        public IQuestionSet QuestionSet { get; internal set; }
        public QuestionModel CurrentQuestion => QuestionSet.GetQuestion(currentQuestionIndex);

        #endregion

        #region Constructor
        public QuestionSetManager(TriviaDBQuestionGetterService questionGetter)
        {
            _questionGetter = questionGetter;

        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets a new questionset from OpenTDB
        /// </summary>
        /// <param name="questionCount">The number of questions to pull. Try to keep this from being too large</param>
        /// <param name="difficulty">a string representing difficulty. Easy, medium, hard on OpenTDB</param>
        /// <param name="callback">A function to call when the questionset is ready. Must accept an IQuestionSet.</param>
        public async void GetNewQuestionSet(uint questionCount, string difficulty, Action<IQuestionSet> callback)
        {
            // Wrap the questions in a QuestionSet
            List<QuestionModel> questions = await _questionGetter.GetQuestions(questionCount, difficulty);
            QuestionSet = new QuestionSet(questions);
            currentQuestionIndex = 0;
            callback(QuestionSet);
        }

        /// <summary>
        /// Gets the next question in the current trivia session
        /// </summary>
        /// <returns>A <c>QuestionModel</c> representing the question</returns>
        public QuestionModel GetNextQuestion()
        {
            // Check if we've run out of questions
            if(currentQuestionIndex+1 > QuestionSet.QuestionCount)
            {
                OutOfQuestions?.Invoke(this, null);
                return null;
            }

            // If we have questions, grab a new one
            currentQuestionIndex += 1;
            return QuestionSet.GetQuestion(currentQuestionIndex);
        }
        #endregion

        public event EventHandler OutOfQuestions;
    }
}
