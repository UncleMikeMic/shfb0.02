using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;

namespace LapinerTools.Steam.UI
{
	/// <summary>
	/// This class manages the uGUI of the Steam leaderboards UI.
	/// It registers to events of SteamLeaderboardsMain class, e.g. SteamLeaderboardsMain.OnDownloadedScores.
	/// You can replace this class with your own UI e.g. NGUI.
	/// In this case you need to take care of registering to events and calling methods of the SteamLeaderboardsMain class from your new implementation.
	/// </summary>
	public class SteamLeaderboardsUI : MonoBehaviour
	{
		protected static SteamLeaderboardsUI s_instance;
		/// <summary>
		/// You can use the static Instance property to access the SteamLeaderboardsUI class from wherever you need it in your code.
		/// Use this property only if you know that you have a static SteamLeaderboardsUI uGUI in your scene.
		/// If you use the SteamLeaderboardsPopup, then there is no guarantee that the SteamLeaderboardsUI was already created.
		/// </summary>
		public static SteamLeaderboardsUI Instance
		{
			get
			{
				// try to find an existing instance
				if (s_instance == null)
				{
					s_instance = FindObjectOfType<SteamLeaderboardsUI>();
				}
				return s_instance;
			}
		}

		/// <summary>
		/// Invoked when the data of the LeaderboardListEntry prefab (SteamLeaderboardsScoreEntryNode class) is updated.
		/// You can use this event to initialize fields of your custom UI. For example, you could add a replay button to the LeaderboardListEntry prefab.
		/// Then you would search the replay uGUI button object and set the button callback when this event is triggered.
		/// </summary>
		public event System.Action<SteamLeaderboardsScoreEntryNode.EntryDataSetEventArgs> OnEntryDataSet;

		
		/// <summary>
		/// Internal method triggering the SteamLeaderboardsUI.OnEntryDataSet event with exception handling. Required to ensure code execution even if your code throws exceptions.
		/// </summary>
		public void InvokeOnEntryDataSet(LeaderboardsScoreEntry p_entryData, SteamLeaderboardsScoreEntryNode p_entryUI) { InvokeEventHandlerSafely(OnEntryDataSet, new SteamLeaderboardsScoreEntryNode.EntryDataSetEventArgs() { EntryData = p_entryData, EntryUI = p_entryUI }); }

		[SerializeField]
		protected Text LEADERBOARD_NAME_TEXT = null;
		[SerializeField]
		protected Text LEADERBOARD_EMPTY_MESSAGE = null;
		[SerializeField]
		protected uMyGUI_TreeBrowser SCORES_BROWSER = null;
		[SerializeField, Tooltip("If set greater 0,then the height of the entries in the SCORES_BROWSER will be scaled, so that the given amount of entries is visible without the need for a scrollbar.")]
		protected int ENTRIES_PER_PAGE = 10;

		/// <summary>
		/// Call SetScores to refresh the leaderboard UI.
		/// Calling this method will remove all currently visible score entries and replace them with those passed in the p_scores argument.
		/// </summary>
		/// <param name="p_scores">list of items to be visualized.</param>
		public void SetScores(List<LeaderboardsScoreEntry> p_scores)
		{
			if (LEADERBOARD_EMPTY_MESSAGE != null) { LEADERBOARD_EMPTY_MESSAGE.enabled = p_scores.Count == 0; }

			if (SCORES_BROWSER != null)
			{
				SCORES_BROWSER.Clear();
				if (ENTRIES_PER_PAGE > 0 && SCORES_BROWSER.ParentScroller != null)
				{
					SCORES_BROWSER.ForcedEntryHeight = SCORES_BROWSER.ParentScroller.GetComponent<RectTransform>().rect.height / (float)ENTRIES_PER_PAGE;
				}
				SCORES_BROWSER.BuildTree(ConvertScoresToNodes(p_scores));
			}
			else
			{
				Debug.LogError("SteamLeaderboardsUI: SetScores: SCORES_BROWSER is not set in inspector!");
			}
		}

		/// <summary>
		/// Call SetLeaderboardName to update the text of the uGUI Text object linked in the LEADERBOARD_NAME_TEXT inspector property.
		/// </summary>
		public void SetLeaderboardName(string p_leaderboardName)
		{
			if (LEADERBOARD_NAME_TEXT != null)
			{
				LEADERBOARD_NAME_TEXT.text = p_leaderboardName;
			}
			else
			{
				Debug.LogError("SteamLeaderboardsUI: SetLeaderboardName: LEADERBOARD_NAME_TEXT is not set in inspector!");
			}
		}

		/// <summary>
		/// The same as SteamLeaderboardsMain.DownloadScores with an additional loading popup.
		/// </summary>
		/// <param name="p_leaderboardName">leaderboard name.</param>
		public void DownloadScores(string p_leaderboardName)
		{
			// update leaderboard name UI
			SetLeaderboardName(p_leaderboardName);
			if (LEADERBOARD_EMPTY_MESSAGE != null) { LEADERBOARD_EMPTY_MESSAGE.enabled = false; }
			// clear current entries
			if (SCORES_BROWSER != null)
			{
				SCORES_BROWSER.Clear();
			}
			// show loading popup while leaderboard is loading
			uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
			SteamLeaderboardsMain.Instance.DownloadScores(p_leaderboardName, (LeaderboardsDownloadedScoresEventArgs p_leaderboardArgs) =>
			{
				// leaderboard is loaded -> hide loading popup
				uMyGUI_PopupManager.Instance.HidePopup(uMyGUI_PopupManager.POPUP_LOADING);
			});
		}

		/// <summary>
		/// The same as SteamLeaderboardsMain.DownloadScoresAroundUser with an additional loading popup.
		/// </summary>
		/// <param name="p_leaderboardName">leaderboard name.</param>
		/// <param name="p_range">range around user's score.</param>
		public void DownloadScoresAroundUser(string p_leaderboardName, int p_range)
		{
			// update leaderboard name UI
			SetLeaderboardName(p_leaderboardName);
			if (LEADERBOARD_EMPTY_MESSAGE != null) { LEADERBOARD_EMPTY_MESSAGE.enabled = false; }
			// clear current entries
			if (SCORES_BROWSER != null)
			{
				SCORES_BROWSER.Clear();
			}
			// show loading popup while leaderboard is loading
			uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
			SteamLeaderboardsMain.Instance.DownloadScoresAroundUser(p_leaderboardName, p_range, (LeaderboardsDownloadedScoresEventArgs p_leaderboardArgs) =>
			{
				// leaderboard is loaded -> hide loading popup
				uMyGUI_PopupManager.Instance.HidePopup(uMyGUI_PopupManager.POPUP_LOADING);
			});
		}

		/// <summary>
		/// The same as SteamLeaderboardsMain.UploadScore with an additional loading popup and a new record popup.
		/// </summary>
		/// <param name="p_leaderboardName">leaderboard name.</param>
		/// <param name="p_score">player score.</param>
		public static void UploadScore(string p_leaderboardName, int p_score)
		{
			UploadScore(p_leaderboardName, p_score, null);
		}

		/// <summary>
		/// The same as SteamLeaderboardsMain.UploadScore with an additional loading popup and new record popup.
		/// </summary>
		/// <param name="p_leaderboardName">leaderboard name.</param>
		/// <param name="p_score">player score.</param>
		/// <param name="p_onUploadedScoreSuccessfully">action to call when the score is uploaded successfully.</param>
		public static void UploadScore(string p_leaderboardName, int p_score, System.Action<LeaderboardsUploadedScoreEventArgs> p_onUploadedScoreSuccessfully)
		{
			// show loading popup while score is uploading
			uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
			SteamLeaderboardsMain.Instance.UploadScore(p_leaderboardName, p_score, (LeaderboardsUploadedScoreEventArgs p_leaderboardArgs) =>
			{
				// score is uploaded -> hide loading popup
				uMyGUI_PopupManager.Instance.HidePopup(uMyGUI_PopupManager.POPUP_LOADING);

				// do nothing on error (error message will be displayed by SteamLeaderboardsUI)
				if (!p_leaderboardArgs.IsError)
				{
					if (p_onUploadedScoreSuccessfully != null) { p_onUploadedScoreSuccessfully(p_leaderboardArgs); }

					// player has made a new record -> show a new record popup
					if (p_leaderboardArgs.IsScoreChanged)
					{
						string statusText = "Score: " + p_leaderboardArgs.ScoreString;
						statusText += "\nGlobal Rank: " + p_leaderboardArgs.GlobalRankNew;
						int rankDif = p_leaderboardArgs.GlobalRankNew - p_leaderboardArgs.GlobalRankPrevious;
						if (rankDif != 0 && p_leaderboardArgs.GlobalRankPrevious > 0)
						{
							statusText += " (";
							statusText += rankDif < 0 ? "<color=green>" : "<color=red>";
							statusText += (p_leaderboardArgs.GlobalRankPrevious - p_leaderboardArgs.GlobalRankNew).ToString("+#;-#;0") + "</color>";
							statusText += ")";
						}
						((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
							.SetText("New Record", statusText)
							.ShowButton(uMyGUI_PopupManager.BTN_OK);
					}
				}
			});
		}

		protected virtual void Start()
		{
			// refresh leaderboard as soon as it is loaded
			SteamLeaderboardsMain.Instance.OnDownloadedScores += SetScores;
			// show error popups when something goes wrong
			SteamLeaderboardsMain.Instance.OnError += ShowErrorMessage;
		}

		protected virtual void Update()
		{
			// hide scrollbar if no scrolling is required
			UpdateScrollbarVisibility();
		}

		protected virtual void OnDestroy()
		{
			if (SteamLeaderboardsMain.IsInstanceSet)
			{
				SteamLeaderboardsMain.Instance.OnDownloadedScores -= SetScores;
				SteamLeaderboardsMain.Instance.OnError -= ShowErrorMessage;
			}
		}

		protected virtual void ShowErrorMessage(ErrorEventArgs p_errorArgs)
		{
			// hide the loading popup if visible
			uMyGUI_PopupManager.Instance.HidePopup(uMyGUI_PopupManager.POPUP_LOADING);
			// show the error message in a new popup
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
				.SetText("Steam Error", p_errorArgs.ErrorMessage)
				.ShowButton(uMyGUI_PopupManager.BTN_OK);
		}

		protected virtual void SetScores(LeaderboardsDownloadedScoresEventArgs p_leaderboardArgs)
		{
			if (!p_leaderboardArgs.IsError)
			{
				SetLeaderboardName(p_leaderboardArgs.LeaderboardName);
				SetScores(p_leaderboardArgs.Scores);
			}
			else
			{
				Debug.LogError("SteamLeaderboardsUI: SetScores: Steam Error: " + p_leaderboardArgs.ErrorMessage);
			}
		}

		/// <summary>
		/// This method will convert the gives Steam score entries to UI nodes, which can be passed to the item browser.
		/// </summary>
		protected virtual uMyGUI_TreeBrowser.Node[] ConvertScoresToNodes(List<LeaderboardsScoreEntry> p_scores)
		{
			uMyGUI_TreeBrowser.Node[] nodes = new uMyGUI_TreeBrowser.Node[p_scores.Count];
			for (int i = 0; i < p_scores.Count; i++)
			{
				if (p_scores[i] != null)
				{
					SteamLeaderboardsScoreEntryNode.SendMessageInitData data = new SteamLeaderboardsScoreEntryNode.SendMessageInitData()
					{
						ScoreEntry = p_scores[i]
					};
					uMyGUI_TreeBrowser.Node node = new uMyGUI_TreeBrowser.Node(data, null);
					nodes[i] = node;
				}
				else
				{
					Debug.LogError("SteamLeaderboardsUI: ConvertScoresToNodes: score at index '" + i + "' is null!");
				}
			}
			return nodes;
		}

		protected virtual void UpdateScrollbarVisibility()
		{
			Scrollbar scrollbar;
			if (SCORES_BROWSER != null && SCORES_BROWSER.ParentScroller != null && (scrollbar = SCORES_BROWSER.ParentScroller.verticalScrollbar) != null)
			{
				scrollbar.gameObject.SetActive(scrollbar.size < 0.9925f);
			}
		}
		
		protected virtual void InvokeEventHandlerSafely<T>(System.Action<T> p_handler, T p_data)
		{
			try
			{
				if (p_handler != null) { p_handler(p_data); }
			}
			catch (System.Exception ex)
			{
				Debug.LogError("SteamLeaderboardsUI: your event handler ("+p_handler.Target+" - System.Action<"+typeof(T)+">) has thrown an excepotion!\n" + ex);
			}
		}
	}
}
