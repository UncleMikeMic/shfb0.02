using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;

namespace LapinerTools.Steam.UI
{
	/// <summary>
	/// This is the Leaderboard popup. It wraps the SteamLeaderboardsUI class, which can be accessed through SteamLeaderboardsPopup.LeaderboardUI.
	/// This class is attached to the popup_steam_leaderboard_root prefab.
	/// Trigger this popup like this:<br />
	/// <c>uMyGUI_PopupManager.Instance.ShowPopup("steam_leaderboard");</c>
	/// </summary>
	public class SteamLeaderboardsPopup : uMyGUI_Popup
	{
		[SerializeField]
		protected SteamLeaderboardsUI m_leaderboardUI;
		/// <summary>
		/// Use this property to access the SteamLeaderboardsUI class.
		/// </summary>
		public SteamLeaderboardsUI LeaderboardUI { get{ return m_leaderboardUI; } }

		public SteamLeaderboardsPopup()
		{
			DestroyOnHide = true;
		}
	}
}
