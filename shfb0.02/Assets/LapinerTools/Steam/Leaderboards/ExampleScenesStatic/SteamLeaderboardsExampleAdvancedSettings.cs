using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using Steamworks;
using LapinerTools.Steam;
using LapinerTools.Steam.UI;
using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;

public class SteamLeaderboardsExampleAdvancedSettings : MonoBehaviour
{
	private bool m_isShown = false;
	private Vector2 m_scrollPos = Vector2.zero;
	private string m_detailsToUpload = "";
	private LeaderboardsScoreEntry m_userScore = null;

	private void Start()
	{
		SteamLeaderboardsMain.Instance.OnDownloadedScores += OnDownloadedScores;
	}

	private void OnDestroy()
	{
		if (SteamLeaderboardsMain.IsInstanceSet)
		{
			SteamLeaderboardsMain.Instance.OnDownloadedScores -= OnDownloadedScores;
		}
	}

	private void OnDownloadedScores(LapinerTools.Steam.Data.LeaderboardsDownloadedScoresEventArgs p_leaderboardArgs)
	{
		foreach (LeaderboardsScoreEntry scoreEntry in p_leaderboardArgs.Scores)
		{
			if (scoreEntry.IsCurrentUserScore)
			{
				// save users score entry (will be used to see additional entry data)
				m_userScore = scoreEntry;
				break;
			}
		}

	}

	private void OnGUI()
	{
		// dowload scores
		m_isShown = GUI.Toggle(new Rect(0, 140, 80, 20), m_isShown, "Advanced");
		if (m_isShown)
		{
			m_scrollPos = GUI.BeginScrollView(new Rect(0, 166, 140, Screen.height - 166), m_scrollPos, new Rect(0, 0, 120, 600));

			// ScoreDownloadSource
			GUI.Box(new Rect(2, 0, 120, 110), "Source:\n" + SteamLeaderboardsMain.Instance.ScoreDownloadSource.ToString().Replace("k_ELeaderboardDataRequest", ""));
			if (GUI.Button(new Rect(6, 36, 112, 22), new GUIContent("Global", "Set to load global highscores")))
			{
				SteamLeaderboardsMain.Instance.ScoreDownloadSource = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal;
			}
			if (GUI.Button(new Rect(6, 60, 112, 22), new GUIContent("AroundUser", "Set to load scores around user's score\nAdapt range! e.g. start=-4, end=5")))
			{
				SteamLeaderboardsMain.Instance.ScoreDownloadSource = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser;
			}
			if (GUI.Button(new Rect(6, 84, 112, 22), new GUIContent("Friends", "Set to load scores for friends only")))
			{
				SteamLeaderboardsMain.Instance.ScoreDownloadSource = ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends;
			}

			// ScoreDownloadRangeStart and ScoreDownloadRangeEnd
			int rangeStart = SteamLeaderboardsMain.Instance.ScoreDownloadRangeStart;
			int rangeEnd = SteamLeaderboardsMain.Instance.ScoreDownloadRangeEnd;
			GUI.Box(new Rect(2, 116, 120, 30), "");
			GUI.Label(new Rect(4, 120, 112, 22), new GUIContent("Range:         -", "Score entries range to load. For example,\n[0,10] for Global, [-4,5] for AroundUser"));
			if (int.TryParse(GUI.TextField(new Rect(new Rect(48, 120, 30, 22)), rangeStart.ToString()), out rangeStart))
			{
				SteamLeaderboardsMain.Instance.ScoreDownloadRangeStart = rangeStart;
			}
			if (int.TryParse(GUI.TextField(new Rect(new Rect(88, 120, 30, 22)), rangeEnd.ToString()), out rangeEnd))
			{
				SteamLeaderboardsMain.Instance.ScoreDownloadRangeEnd = rangeEnd;
			}

			// ScoreSortMethod
			GUI.Box(new Rect(2, 152, 120, 86), "Sort:\n" + SteamLeaderboardsMain.Instance.ScoreSortMethod.ToString().Replace("k_ELeaderboardSortMethod", ""));
			if (GUI.Button(new Rect(6, 188, 112, 22), new GUIContent("Ascending", "If you use UploadScore to create\nleaderboards, then set the sorting mode.")))
			{
				SteamLeaderboardsMain.Instance.ScoreSortMethod = ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending;
			}
			if (GUI.Button(new Rect(6, 212, 112, 22), new GUIContent("Descending", "If you use UploadScore to create\nleaderboards, then set the sorting mode.")))
			{
				SteamLeaderboardsMain.Instance.ScoreSortMethod = ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending;
			}

			// ScoreType
			GUI.Box(new Rect(2, 242, 120, 110), "Type:\n" + SteamLeaderboardsMain.Instance.ScoreType.ToString().Replace("k_ELeaderboardDisplayType", ""));
			if (GUI.Button(new Rect(6, 278, 112, 22), new GUIContent("Numeric", "Affects visualization of the score entries\nfor leaderboards created with UploadScore.")))
			{
				SteamLeaderboardsMain.Instance.ScoreType = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric;
			}
			if (GUI.Button(new Rect(6, 302, 112, 22), new GUIContent("Seconds", "Affects visualization of the score entries\nfor leaderboards created with UploadScore.")))
			{
				SteamLeaderboardsMain.Instance.ScoreType = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeSeconds;
			}
			if (GUI.Button(new Rect(6, 326, 112, 22), new GUIContent("MilliSeconds", "Affects visualization of the score entries\nfor leaderboards created with UploadScore.")))
			{
				SteamLeaderboardsMain.Instance.ScoreType = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeMilliSeconds;
			}

			// ScoreUploadMethod
			GUI.Box(new Rect(2, 356, 120, 88), "Sort:\n" + SteamLeaderboardsMain.Instance.ScoreUploadMethod.ToString().Replace("k_ELeaderboardUploadScoreMethod", ""));
			if (GUI.Button(new Rect(6, 394, 112, 22), new GUIContent("KeepBest", "Overwrite existing scores\nonly with new records.")))
			{
				SteamLeaderboardsMain.Instance.ScoreUploadMethod = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest;
			}
			if (GUI.Button(new Rect(6, 418, 112, 22), new GUIContent("ForceUpdate", "Always overwrite existing scores.")))
			{
				SteamLeaderboardsMain.Instance.ScoreUploadMethod = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate;
			}

			// ScoreDownloadDetailsLength
			int detailsLength = SteamLeaderboardsMain.Instance.ScoreDownloadDetailsLength;
			GUI.Box(new Rect(2, 448, 120, 150), "Entry Details:");
			GUI.Label(new Rect(4, 472, 112, 22), new GUIContent("Length:", "Integers count of additional data for the\nscore entry. Could be used to save replays."));
			if (int.TryParse(GUI.TextField(new Rect(new Rect(55, 472, 60, 22)), detailsLength.ToString()), out detailsLength))
			{
				SteamLeaderboardsMain.Instance.ScoreDownloadDetailsLength = detailsLength;
			}
			GUI.Label(new Rect(4, 496, 112, 22), new GUIContent("Data:", "This text will be uploaded\nwhen the button below is used."));
			m_detailsToUpload = GUI.TextField(new Rect(new Rect(55, 496, 60, 22)), m_detailsToUpload);
			if (GUI.Button(new Rect(6, 520, 112, 50), new GUIContent("Upload Score\nWith Data", "Will upload the score\nwith the text entered above.")))
			{
				// get leaderboard name and score from SteamLeaderboardsExampleStatic or SteamLeaderboardsExamplePopup
				string leaderboardName;
				int uploadScore;
				if (GetLeaderboardNameAndScoreFromSimpleExampleScript(out leaderboardName, out uploadScore))
				{
					// show loading popup while score is uploading
					uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
					SteamLeaderboardsMain.Instance.UploadScore(
						leaderboardName,
						uploadScore,
						SteamLeaderboardsMain.Instance.ScoreSortMethod,
						SteamLeaderboardsMain.Instance.ScoreType,
						m_detailsToUpload,
						(LeaderboardsUploadedScoreEventArgs p_leaderboardArgs) =>
					{
						// score is uploaded -> hide loading popup
						uMyGUI_PopupManager.Instance.HidePopup(uMyGUI_PopupManager.POPUP_LOADING);
						// show top 10 scores around player when score is uploaded
						if (SteamLeaderboardsUI.Instance != null) // could have been closed if popup UI
						{
							SteamLeaderboardsUI.Instance.DownloadScoresAroundUser(leaderboardName, 9);
						}
					});
				}
			}
			if (GUI.Button(new Rect(6, 572, 112, 22), new GUIContent("Get Score Data", "Will show the additional data\nof the current user's score entry.")))
			{
				string scoreText;
				if (detailsLength <= 1) // one integer is needed to save the array's length -> at least 2 integers are required to save any data
				{
					scoreText = "Please set entry details length to a value > 1, then hit 'Load Scores' in the top!";
				}
				else if (m_userScore != null)
				{
					scoreText =
						"#" + m_userScore.GlobalRank + " - " + m_userScore.UserName + " - " + m_userScore.ScoreString + "\nData: '" +
						m_userScore.DetailsDownloadedAsString + "'";
				}
				else
				{
					scoreText = "Please hit 'Load Scores' in the top first and make sure that the players score is returned in the list (try Source=AroundUser)!";
				}
				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
					.SetText("User Score", scoreText)
					.ShowButton(uMyGUI_PopupManager.BTN_OK);
			}

			GUI.EndScrollView();

			// Tooltip
			if (!string.IsNullOrEmpty(GUI.tooltip))
			{
				GUI.Box(new Rect(82, 110, 270, 50), GUI.tooltip);
			}
		}
	}

	private bool GetLeaderboardNameAndScoreFromSimpleExampleScript(out string o_leaderboardName, out int o_uploadScore)
	{
		o_leaderboardName = "";
		o_uploadScore = 0;
		SteamLeaderboardsExampleStatic exampleScriptStatic;
		SteamLeaderboardsExamplePopup exampleScriptPopup;
		if ((exampleScriptStatic = FindObjectOfType<SteamLeaderboardsExampleStatic>()) != null)
		{
			o_leaderboardName = exampleScriptStatic.LeaderboardName;
			o_uploadScore = exampleScriptStatic.UploadScore;
			return true; // found static example script
		}
		else if ((exampleScriptPopup = FindObjectOfType<SteamLeaderboardsExamplePopup>()) != null)
		{
			o_leaderboardName = exampleScriptPopup.LeaderboardName;
			o_uploadScore = exampleScriptPopup.UploadScore;
			return true; // found popup example script
		}
		return false; // no example script found
	}
}
