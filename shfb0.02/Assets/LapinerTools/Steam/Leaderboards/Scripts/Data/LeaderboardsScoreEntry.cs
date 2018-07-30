using UnityEngine;
using System.Collections;

using Steamworks;

namespace LapinerTools.Steam.Data
{
	/// <summary>
	/// The LeaderboardsScoreEntry class stores all available data about a Steam leaderboard score entry.
	/// You can get the native Steam data (e.g. SteamLeaderboard) from the LeaderboardsScoreEntry.SteamNative property.
	/// Most properties are self-explanatory and not documented in more detail.
	/// </summary>
	public class LeaderboardsScoreEntry
	{
		/// <summary>
		/// The LeaderboardsScoreEntry.SteamNativeData class contains Steam native data such as SteamIDUser, UGCHandle or the SteamLeaderboard handle.
		/// You can use this data to make own calls to the Steamworks.NET API.
		/// </summary>
		public class SteamNativeData
		{
			public SteamLeaderboard_t m_hSteamLeaderboard { get; set; }
			public UGCHandle_t m_hUGC { get; set; }
			/// <summary>
			/// Entrie's user - use SteamFriends()->GetFriendPersonaName() & SteamFriends()->GetFriendAvatar() to get more info
			/// </summary>
			public CSteamID m_steamIDUser { get; set; }

			public SteamNativeData()
			{
			}
			public SteamNativeData(SteamLeaderboard_t p_hSteamLeaderboard, UGCHandle_t p_hUGC, CSteamID p_steamIDUser)
			{
				m_hSteamLeaderboard = p_hSteamLeaderboard;
				m_hUGC = p_hUGC;
				m_steamIDUser = p_steamIDUser;
			}
		}

		public string LeaderboardName { get; set; }
		public string UserName { get; set; }
		public int GlobalRank { get; set; }
		public int Score { get; set; }

		/// <summary>
		/// Score formatted with SteamLeaderboardsMain.ScoreFormatNumeric or SteamLeaderboardsMain.ScoreFormatSeconds or SteamLeaderboardsMain.ScoreFormatMilliSeconds taking LeaderboardsScoreEntry.ScoreType into account.
		/// </summary>
		public string ScoreString { get; set; }
		
		/// <summary>
		/// The display type of the leaderboard, which was used to upload the score.
		/// </summary>
		public ELeaderboardDisplayType ScoreType { get; set; }

		/// <summary>
		/// The length of the details array for this score entry available on Steam.
		/// Use the SteamLeaderboardsMain.ScoreDownloadDetailsLength property to download this data.
		/// </summary>
		public int DetailsAvailableLength { get; set; }

		/// <summary>
		/// The downloaded details array for this score entry.
		/// Use the SteamLeaderboardsMain.ScoreDownloadDetailsLength property to download this data.
		/// </summary>
		public int[] DetailsDownloaded { get; set; }

		/// <summary>
		/// Converts LeaderboardsScoreEntry.DetailsDownloaded from an integer array to a string.
		/// Same as SteamLeaderboardsMain.ConvertIntArrayToStr.
		/// </summary>
		public string DetailsDownloadedAsString { get { return SteamLeaderboardsMain.ConvertIntArrayToStr(DetailsDownloaded); } }

		/// <summary>
		/// True if this score entry belongs to the current user.
		/// </summary>
		public bool IsCurrentUserScore { get; set; }
		
		/// <summary>
		/// Contains Steam native data such as CSteamID, UGCHandle or the SteamLeaderboard handle.
		/// You can use this data to make own calls to the Steamworks.NET API.
		/// </summary>
		public SteamNativeData SteamNative { get; set; }
		
		public LeaderboardsScoreEntry()
		{
			SteamNative = new SteamNativeData();
		}

	}
}
