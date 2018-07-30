using UnityEngine;
using System.Collections;

using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.UI;
using LapinerTools.uMyGUI;

public class SteamLeaderboardsExamplePopup : MonoBehaviour
{
	private string m_leaderboardName = "Leaderboard";
	private int m_uploadScore = 123;
	
	public string LeaderboardName { get{ return m_leaderboardName; } }
	public int UploadScore { get{ return m_uploadScore; } }

	private void Start()
	{
		// enable debug log
		SteamLeaderboardsMain.Instance.IsDebugLogEnabled = true;
	}

	private void OnGUI()
	{
		// dowload scores
		m_leaderboardName = GUILayout.TextField(m_leaderboardName);
		if (GUILayout.Button("Load\nScores"))
		{
			// show the Steam Leaderboard popup
			((SteamLeaderboardsPopup)uMyGUI_PopupManager.Instance.ShowPopup("steam_leaderboard")).LeaderboardUI.DownloadScores(m_leaderboardName);
		}

		// upload scores
		m_uploadScore = (int)GUILayout.HorizontalSlider(m_uploadScore, 1, 5000);
		if (GUILayout.Button("Upload\nScore\n" + m_uploadScore))
		{
			SteamLeaderboardsUI.UploadScore(m_leaderboardName, m_uploadScore, (LeaderboardsUploadedScoreEventArgs p_leaderboardArgs) =>
			{
				// show top 10 scores around player when score is uploaded
				if (SteamLeaderboardsUI.Instance != null) // could have been closed
				{
					SteamLeaderboardsUI.Instance.DownloadScoresAroundUser(m_leaderboardName, 9);
				}
			});
		}
	}
}
