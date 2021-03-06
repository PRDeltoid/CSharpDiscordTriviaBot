﻿using Discord.WebSocket;
using System.Collections.Generic;

namespace TriviaBot.Services
{
    public class ScoreKeeperService : IScoreKeeperService
    {
        private readonly Dictionary<ulong, uint> scores = new Dictionary<ulong, uint>();
        private readonly DiscordSocketClient _discord;

        #region Properties
        public List<UserScoreModel> Scores
        {
            get
            {
                List<UserScoreModel> scoreList = new List<UserScoreModel>();
                foreach (var score in scores)
                {
                    scoreList.Add(new UserScoreModel { Id = score.Key, Score = score.Value, Username = _discord.GetUser(score.Key).Username });
                }
                return scoreList;
            }
        }

        public int Count { get => scores.Count; }
        #endregion

        public ScoreKeeperService(DiscordSocketClient discordSocket)
        {
            _discord = discordSocket;
        }

        #region Public Region
        public void ResetScores()
        {
            scores.Clear();
        }

        public void AddScore(SocketUser user, uint score)
        {
            if (HasScoreForUserId(user.Id))
            {
                scores[user.Id] += score;
            }
            else
            {
                scores.Add(user.Id, score);
            }
        }
        #endregion

        #region Private Methods
        private bool HasScoreForUserId(ulong Id)
        {
            return scores.ContainsKey(Id);
        }
        #endregion
    }
}
