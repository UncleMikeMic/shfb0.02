using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Steamworks;

namespace LapinerTools.Steam.Data
{
	/// <summary>
	/// This event arguments are used for all leaderboard download related events.
	/// </summary>
	public class LeaderboardsDownloadedScoresEventArgs : EventArgsBase
	{
		/// <summary>
		/// Leaderboard name used in this download event.
		/// </summary>
		public string LeaderboardName { get; set; }

		/// <summary>
		/// List of scores downloaded for the LeaderboardsDownloadedScoresEventArgs.LeaderboardName leadeboard.
		/// </summary>
		public List<LeaderboardsScoreEntry> Scores { get; set; }

		public LeaderboardsDownloadedScoresEventArgs() : base()
		{
			LeaderboardName = "";
			Scores = new List<LeaderboardsScoreEntry>();
		}
		public LeaderboardsDownloadedScoresEventArgs(string p_leaderboardName, List<LeaderboardsScoreEntry> p_scores) : base()
		{
			LeaderboardName = p_leaderboardName;
			Scores = p_scores;
		}
		public LeaderboardsDownloadedScoresEventArgs(EventArgsBase p_errorEventArgs) : base(p_errorEventArgs)
		{
			LeaderboardName = "";
			Scores = new List<LeaderboardsScoreEntry>();
		}
	}
}
