using UnityEngine;
using System.Collections;

using Steamworks;

namespace LapinerTools.Steam.Data
{
	/// <summary>
	/// This event arguments are used for all score upload related events.
	/// </summary>
	public class LeaderboardsUploadedScoreEventArgs : EventArgsBase
	{
		/// <summary>
		/// The LeaderboardsUploadedScoreEventArgs.SteamNativeData class contains Steam native data such as the SteamLeaderboard handle.
		/// You can use this data to make own calls to the Steamworks.NET API.
		/// </summary>
		public class SteamNativeData
		{
			public SteamLeaderboard_t m_hSteamLeaderboard { get; set; }

			public SteamNativeData()
			{
			}
			public SteamNativeData(SteamLeaderboard_t p_hSteamLeaderboard)
			{
				m_hSteamLeaderboard = p_hSteamLeaderboard;
			}
		}

		/// <summary>
		/// The leaderboard name that was attempted to set.
		/// </summary>
		public string LeaderboardName { get; set; }

		/// <summary>
		/// The score that was attempted to set.
		/// </summary>
		public int Score { get; set; }

		/// <summary>
		/// Score formatted with SteamLeaderboardsMain.ScoreFormatNumeric or SteamLeaderboardsMain.ScoreFormatSeconds or SteamLeaderboardsMain.ScoreFormatMilliSeconds taking LeaderboardsUploadedScoreEventArgs.ScoreType into account.
		/// </summary>
		public string ScoreString { get; set; }

		/// <summary>
		/// The display type of the leaderboard, which was used to upload the score.
		/// </summary>
		public ELeaderboardDisplayType ScoreType { get; set; }

		/// <summary>
		/// True if the score in the leaderboard changed, false if the existing score was better.
		/// </summary>
		public bool IsScoreChanged { get; set; }

		/// <summary>
		/// The new global rank of the user in this leaderboard.
		/// </summary>
		public int GlobalRankNew { get; set; }

		/// <summary>
		/// The previous global rank of the user in this leaderboard; 0 if the user had no existing entry in the leaderboard.
		/// </summary>
		public int GlobalRankPrevious { get; set; }

		/// <summary>
		/// Contains Steam native data such as the SteamLeaderboard handle.
		/// You can use this data to make own calls to the Steamworks.NET API.
		/// </summary>
		public SteamNativeData SteamNative { get; set; }

		public LeaderboardsUploadedScoreEventArgs() : base()
		{
			SteamNative = new SteamNativeData();
		}
		public LeaderboardsUploadedScoreEventArgs(EventArgsBase p_errorEventArgs) : base(p_errorEventArgs)
		{
			SteamNative = new SteamNativeData();
		}
	}
}
