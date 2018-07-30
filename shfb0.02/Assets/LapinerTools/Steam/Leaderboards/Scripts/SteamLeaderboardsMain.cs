using UnityEngine;

using System.Text;
using System.Collections;
using System.Collections.Generic;

using LapinerTools.Steam.Data;

using Steamworks;
 
namespace LapinerTools.Steam
{
	/// <summary>
	/// SteamLeaderboardsMain is the easy to use lightweight Steam Leaderboards API of the Easy Steamworks Integration Unity plugin.
	/// Use this class to download sorted leaderboards, submit highscores, handle additional score entry data and load avatar textures.
	/// </summary>
	public class SteamLeaderboardsMain : SteamMainBase<SteamLeaderboardsMain>
	{
#region Members

		// contains already loaded score entries of the leaderboard that is being loaded
		private List<LeaderboardsScoreEntry> m_scores = new List<LeaderboardsScoreEntry>();
		// contains a list of user names, which are not yet loaded from Steam
		private List<CSteamID> m_scoresMissingUserNames = new List<CSteamID>();
		// the name of the leaderboard that is being loaded
		private string m_scoresLeaderboardName = null;

#endregion


#region Events

		/// <summary>
		/// Invoked when a score entry was successfully uploaded or updated. See also SteamLeaderboardsMain.UploadScore.
		/// </summary>
		public event System.Action<LeaderboardsUploadedScoreEventArgs> OnUploadedScore;
		/// <summary>
		/// Invoked when the leaderboard score entries are fully loaded. See also SteamLeaderboardsMain.DownloadScores.
		/// </summary>
		public event System.Action<LeaderboardsDownloadedScoresEventArgs> OnDownloadedScores;

#endregion


#region API

		[SerializeField, Tooltip("If you use SteamLeaderboardsMain.UploadScore to create leaderboards, then set the correct sorting mode here. Alternatively, you can also pass the desired sorting mode to the SteamLeaderboardsMain.UploadScore method.")]
		private ELeaderboardSortMethod m_scoreSorting = ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending;
		/// <summary>
		/// If you use SteamLeaderboardsMain.UploadScore to create leaderboards, then set the correct sorting mode here. Alternatively, you can also pass the desired sorting mode to the SteamLeaderboardsMain.UploadScore method.
		/// </summary>
		public ELeaderboardSortMethod ScoreSortMethod
		{
			get{ return m_scoreSorting; }
			set{ m_scoreSorting = value; }
		}

		[SerializeField, Tooltip("If you use SteamLeaderboardsMain.UploadScore to create leaderboards, then set the correct display type here. Alternatively, you can also pass the desired display type to the SteamLeaderboardsMain.UploadScore method.")]
		private ELeaderboardDisplayType m_scoreType = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric;
		/// <summary>
		/// If you use SteamLeaderboardsMain.UploadScore to create leaderboards, then set the correct display type here. Alternatively, you can also pass the desired display type to the SteamLeaderboardsMain.UploadScore method.
		/// </summary>
		public ELeaderboardDisplayType ScoreType
		{
			get{ return m_scoreType; }
			set{ m_scoreType = value; }
		}
		
		[SerializeField, Tooltip("Define the leaderboard source here.\nGlobal: global highscores.\nAroundUser: scores around user's score (adapt range!).\nFriends: scores for friends only.")]
		private ELeaderboardDataRequest m_scoreDownloadSource = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal;
		/// <summary>
		/// Define the leaderboard source here.<br />Global: global highscores.<br />AroundUser: scores around user's score (adapt range!).<br />Friends: scores for friends only.
		/// </summary>
		public ELeaderboardDataRequest ScoreDownloadSource
		{
			get{ return m_scoreDownloadSource; }
			set{ m_scoreDownloadSource = value; }
		}

		[SerializeField, Tooltip("Score entries range to load. For example, [0,10] for Global, [-4,5] for AroundUser.")]
		private int m_scoreDownloadRangeStart = 0;
		/// <summary>
		/// Score entries range to load. For example, [0,10] for Global, [-4,5] for AroundUser.
		/// </summary>
		public int ScoreDownloadRangeStart
		{
			get{ return m_scoreDownloadRangeStart; }
			set{ m_scoreDownloadRangeStart = value; }
		}

		[SerializeField, Tooltip("Score entries range to load. For example, [0,10] for Global, [-4,5] for AroundUser.")]
		private int m_scoreDownloadRangeEnd = 10;
		/// <summary>
		/// Score entries range to load. For example, [0,10] for Global, [-4,5] for AroundUser.
		/// </summary>
		public int ScoreDownloadRangeEnd
		{
			get{ return m_scoreDownloadRangeEnd; }
			set{ m_scoreDownloadRangeEnd = value; }
		}

		[SerializeField, Tooltip("Select when the users score should be updated here.\nKeepBest: overwrite existing scores only with new records.\nForceUpdate: always overwrite existing scores.")]
		private ELeaderboardUploadScoreMethod m_scoreUploadMethod = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest;
		/// <summary>
		/// Select when the users score should be updated here.<br />KeepBest: overwrite existing scores only with new records.<br />ForceUpdate: always overwrite existing scores.
		/// </summary>
		public ELeaderboardUploadScoreMethod ScoreUploadMethod
		{
			get{ return m_scoreUploadMethod; }
			set{ m_scoreUploadMethod = value; }
		}

		[SerializeField, Tooltip("Determines the maximal integers count of additional score entry data downloaded from Steam. Could be used to load replays. Max. 64 integers -> use SteamUGC to upload big data amounts.")]
		private int m_scoreDownloadDetailsLength = 0;
		/// <summary>
		/// Determines the maximal integers count of additional score entry data downloaded from Steam. Could be used to load replays.
		/// </summary>
		public int ScoreDownloadDetailsLength
		{
			get{ return m_scoreDownloadDetailsLength; }
			set
			{
				if (value > 64)
				{
					Debug.LogError("ScoreDownloadDetailsLength: max. 64 integers! Tried to set '" + value + "'!");
				}
				m_scoreDownloadDetailsLength = Mathf.Clamp(value, 0, 64);
			}
		}

		[SerializeField, Tooltip("The pattern used to format scores of Numeric type.")]
		private string m_scoreFormatNumeric = "";
		/// <summary>
		/// The pattern used to format scores of Numeric type.
		/// </summary>
		public string ScoreFormatNumeric
		{
			get{ return m_scoreFormatNumeric; }
			set{ m_scoreFormatNumeric = value; }
		}

		[SerializeField, Tooltip("The pattern used to format scores of Seconds type.")]
		private string m_scoreFormatSeconds = "{0:00}:{1:D2}";
		/// <summary>
		/// The pattern used to format scores of Seconds type.
		/// </summary>
		public string ScoreFormatSeconds
		{
			get{ return m_scoreFormatSeconds; }
			set{ m_scoreFormatSeconds = value; }
		}

		[SerializeField, Tooltip("The pattern used to format scores of MilliSeconds type.")]
		private string m_scoreFormatMilliSeconds = "{0:00}:{1:D2}:{2:D3}";
		/// <summary>
		/// The pattern used to format scores of MilliSeconds type.
		/// </summary>
		public string ScoreFormatMilliSeconds
		{
			get{ return m_scoreFormatMilliSeconds; }
			set{ m_scoreFormatMilliSeconds = value; }
		}

		/// <summary>
		/// Upload or update a score entry in the given leaderboard. The p_onUploadedScore callback is invoked when done.<br />
		/// If the leaderboard with the given name does not yet exist, then it will be created with the display type given in SteamLeaderboardsMain.ScoreType.<br />
		/// If the leaderboard with the given name does not yet exist, then it will be created with the sorting given in SteamLeaderboardsMain.ScoreSortMethod.<br />
		/// See also SteamLeaderboardsMain.OnUploadedScore.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_leaderboardName">name of the leaderboard to update.</param>
		/// <param name="p_score">users score to submit.</param>
		/// <param name="p_onUploadedScore">invoked when the score upload is successfull or an error has occured.</param>
		public bool UploadScore(string p_leaderboardName, int p_score, System.Action<LeaderboardsUploadedScoreEventArgs> p_onUploadedScore)
		{
			return UploadScore(p_leaderboardName, p_score, m_scoreSorting, m_scoreType, p_onUploadedScore);
		}

		/// <summary>
		/// Upload or update a score entry in the given leaderboard. The p_onUploadedScore callback is invoked when done.<br />
		/// If the leaderboard with the given name does not yet exist, then it will be created with the display type given in SteamLeaderboardsMain.ScoreType.<br />
		/// See also SteamLeaderboardsMain.OnUploadedScore.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_leaderboardName">name of the leaderboard to update.</param>
		/// <param name="p_score">users score to submit.</param>
		/// <param name="p_scoreSorting">if the leaderboard with the given name does not yet exist, then it will be created with the given sorting</param>
		/// <param name="p_onUploadedScore">invoked when the score upload is successfull or an error has occured.</param>
		public bool UploadScore(string p_leaderboardName, int p_score, ELeaderboardSortMethod p_scoreSorting, System.Action<LeaderboardsUploadedScoreEventArgs> p_onUploadedScore)
		{
			return UploadScore(p_leaderboardName, p_score, p_scoreSorting, m_scoreType, p_onUploadedScore);
		}
		
		/// <summary>
		/// Upload or update a score entry in the given leaderboard. The p_onUploadedScore callback is invoked when done.<br />
		/// If the leaderboard with the given name does not yet exist, then it will be created with the sorting given in SteamLeaderboardsMain.ScoreSortMethod.<br />
		/// See also SteamLeaderboardsMain.OnUploadedScore.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_leaderboardName">name of the leaderboard to update.</param>
		/// <param name="p_score">users score to submit.</param>
		/// <param name="p_scoreType">if the leaderboard with the given name does not yet exist, then it will be created with the given score display type</param>
		/// <param name="p_onUploadedScore">invoked when the score upload is successfull or an error has occured.</param>
		public bool UploadScore(string p_leaderboardName, int p_score, ELeaderboardDisplayType p_scoreType, System.Action<LeaderboardsUploadedScoreEventArgs> p_onUploadedScore)
		{
			return UploadScore(p_leaderboardName, p_score, m_scoreSorting, p_scoreType, p_onUploadedScore);
		}

		/// <summary>
		/// Upload or update a score entry in the given leaderboard. The p_onUploadedScore callback is invoked when done.<br />
		/// See also SteamLeaderboardsMain.OnUploadedScore.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_leaderboardName">name of the leaderboard to update.</param>
		/// <param name="p_score">users score to submit.</param>
		/// <param name="p_scoreSorting">if the leaderboard with the given name does not yet exist, then it will be created with the given sorting</param>
		/// <param name="p_scoreType">if the leaderboard with the given name does not yet exist, then it will be created with the given score display type</param>
		/// <param name="p_onUploadedScore">invoked when the score upload is successfull or an error has occured.</param>
		public bool UploadScore(string p_leaderboardName, int p_score, ELeaderboardSortMethod p_scoreSorting, ELeaderboardDisplayType p_scoreType, System.Action<LeaderboardsUploadedScoreEventArgs> p_onUploadedScore)
		{
			return UploadScore(p_leaderboardName, p_score, p_scoreSorting, p_scoreType, (int[])null, p_onUploadedScore);
		}

		/// <summary>
		/// Upload or update a score entry in the given leaderboard. The p_onUploadedScore callback is invoked when done.<br />
		/// See also SteamLeaderboardsMain.OnUploadedScore.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_leaderboardName">name of the leaderboard to update.</param>
		/// <param name="p_score">users score to submit.</param>
		/// <param name="p_scoreSorting">if the leaderboard with the given name does not yet exist, then it will be created with the given sorting</param>
		/// <param name="p_scoreType">if the leaderboard with the given name does not yet exist, then it will be created with the given score display type</param>
		/// <param name="p_scoreDetails">additional game-defined information regarding how the user got that score. E.g. a replay, a screenshot, etc...</param>
		/// <param name="p_onUploadedScore">invoked when the score upload is successfull or an error has occured.</param>
		public bool UploadScore(string p_leaderboardName, int p_score, ELeaderboardSortMethod p_scoreSorting, ELeaderboardDisplayType p_scoreType, string p_scoreDetails, System.Action<LeaderboardsUploadedScoreEventArgs> p_onUploadedScore)
		{
			if (p_scoreDetails != null && p_scoreDetails.Length > 252)
			{
				Debug.LogError("UploadScore: max. score details string length is 252 characters! Provided '" + p_scoreDetails.Length + "'! Data will be cutoff!");
				p_scoreDetails = p_scoreDetails.Substring(0, 252);
			}
			return UploadScore(p_leaderboardName, p_score, p_scoreSorting, p_scoreType, ConvertStrToIntArray(p_scoreDetails), p_onUploadedScore);
		}

		/// <summary>
		/// Upload or update a score entry in the given leaderboard. The p_onUploadedScore callback is invoked when done.<br />
		/// See also SteamLeaderboardsMain.OnUploadedScore.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_leaderboardName">name of the leaderboard to update.</param>
		/// <param name="p_score">users score to submit.</param>
		/// <param name="p_scoreSorting">if the leaderboard with the given name does not yet exist, then it will be created with the given sorting</param>
		/// <param name="p_scoreType">if the leaderboard with the given name does not yet exist, then it will be created with the given score display type</param>
		/// <param name="p_scoreDetails">additional game-defined information regarding how the user got that score. E.g. a replay, a screenshot, etc...</param>
		/// <param name="p_onUploadedScore">invoked when the score upload is successfull or an error has occured.</param>
		public bool UploadScore(string p_leaderboardName, int p_score, ELeaderboardSortMethod p_scoreSorting, ELeaderboardDisplayType p_scoreType, int[] p_scoreDetails, System.Action<LeaderboardsUploadedScoreEventArgs> p_onUploadedScore)
		{
			if (SteamManager.Initialized)
			{
				if (p_scoreDetails != null && p_scoreDetails.Length > 64)
				{
					Debug.LogError("UploadScore: max. score details array length is 64 integers! Provided '" + p_scoreDetails.Length + "'! Data will be cutoff!");
				}
				SetSingleShotEventHandler(GetEventNameForOnUploadedScore(p_leaderboardName, p_score), ref OnUploadedScore, p_onUploadedScore);
				Execute<LeaderboardFindResult_t>(SteamUserStats.FindOrCreateLeaderboard(p_leaderboardName, p_scoreSorting, p_scoreType), (p_callback, p_bIOFailure) => OnUploadScoreFindOrCreateLeaderboardCallCompleted(p_leaderboardName, p_score, p_scoreSorting, p_scoreType, p_scoreDetails, p_callback, p_bIOFailure));
				return true; // request started
			}
			else
			{
				ErrorEventArgs errorArgs = ErrorEventArgs.CreateSteamNotInit();
				InvokeEventHandlerSafely(p_onUploadedScore, new LeaderboardsUploadedScoreEventArgs(errorArgs));
				HandleError("UploadScore: failed! ", errorArgs);
				return false; // no request, because there is no connection to steam
			}
		}

		/// <summary>
		/// Downloads the leaderboard with the given name. The p_onDownloadedScores callback is invoked when done.
		/// First all score entries are fetched, then all user names are loaded if they are not yet cached by Steam.
		/// Avatars are loaded as soon as they are requested with SteamLeaderboardsMain.GetAvatarTexture.
		/// See also SteamLeaderboardsMain.OnDownloadedScores.
		/// SteamLeaderboardsMain.ScoreDownloadSource, SteamLeaderboardsMain.ScoreDownloadRangeStart and SteamLeaderboardsMain.ScoreDownloadRangeEnd will be used as filter critera.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_leaderboardName">name of the leaderboard to load.</param>
		/// <param name="p_onDownloadedScores">invoked when the leaderboard is either fully loaded or an error has occured.</param>
		public bool DownloadScores(string p_leaderboardName, System.Action<LeaderboardsDownloadedScoresEventArgs> p_onDownloadedScores)
		{
			return DownloadScores(p_leaderboardName, ScoreDownloadSource, ScoreDownloadRangeStart, ScoreDownloadRangeEnd, p_onDownloadedScores);
		}

		/// <summary>
		/// Downloads the leaderboard entries around the current user's score record. The p_onDownloadedScores callback is invoked when done.
		/// First all score entries are fetched, then all user names are loaded if they are not yet cached by Steam.
		/// Avatars are loaded as soon as they are requested with SteamLeaderboardsMain.GetAvatarTexture.
		/// See also SteamLeaderboardsMain.OnDownloadedScores.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_leaderboardName">name of the leaderboard to load.</param>
		/// <param name="p_range">number of entries to load (if possible, then user's entry will be in the middle of the returned list).</param>
		/// <param name="p_onDownloadedScores">invoked when the leaderboard is either fully loaded or an error has occured.</param>
		public bool DownloadScoresAroundUser(string p_leaderboardName, int p_range, System.Action<LeaderboardsDownloadedScoresEventArgs> p_onDownloadedScores)
		{
			return DownloadScores(p_leaderboardName, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, -p_range/2, p_range - (p_range/2), p_onDownloadedScores);
		}

		/// <summary>
		/// Downloads the leaderboard for the filter criteria. The p_onDownloadedScores callback is invoked when done.
		/// First all score entries are fetched, then all user names are loaded if they are not yet cached by Steam.
		/// Avatars are loaded as soon as they are requested with SteamLeaderboardsMain.GetAvatarTexture.
		/// See also SteamLeaderboardsMain.OnDownloadedScores.
		/// </summary>
		/// <returns><c>true</c>, if a request was started, <c>false</c> when the request could not have been started due to an error.</returns>
		/// <param name="p_leaderboardName">name of the leaderboard to load.</param>
		/// <param name="p_scoreSource">source of the leaderboard entries (Global, AroundUser, Friends).</param>
		/// <param name="p_rangeStart">score entries range to load. For example, [0,10] for Global, [-4,5] for AroundUser.</param>
		/// <param name="p_rangeEnd">score entries range to load. For example, [0,10] for Global, [-4,5] for AroundUser.</param>
		/// <param name="p_onDownloadedScores">invoked when the leaderboard is either fully loaded or an error has occured.</param>
		public bool DownloadScores(string p_leaderboardName, ELeaderboardDataRequest p_scoreSource, int p_rangeStart, int p_rangeEnd, System.Action<LeaderboardsDownloadedScoresEventArgs> p_onDownloadedScores)
		{
			if (SteamManager.Initialized)
			{
				SetSingleShotEventHandler("OnDownloadedScores", ref OnDownloadedScores, p_onDownloadedScores);
				Execute<LeaderboardFindResult_t>(SteamUserStats.FindLeaderboard(p_leaderboardName), (p_callback, p_bIOFailure) => OnDownloadScoresFindLeaderboardCallCompleted(p_leaderboardName, p_scoreSource, p_rangeStart, p_rangeEnd, p_callback, p_bIOFailure));
				return true; // request started
			}
			else
			{
				ErrorEventArgs errorArgs = ErrorEventArgs.CreateSteamNotInit();
				InvokeEventHandlerSafely(p_onDownloadedScores, new LeaderboardsDownloadedScoresEventArgs(errorArgs));
				HandleError("DownloadScores: failed! ", errorArgs);
				return false; // no request, because there is no connection to steam
			}
		}

		/// <summary>
		/// Most users who didn't set their avatar have a question mark as their avatar texture.
		/// However, Steam allows to check if there is no texture set at all, which will probably never happen.
		/// </summary>
		/// <returns><c>true</c> if the user of the given score entry has a custom avatar image or uses the default question mark texture as avatar; otherwise, <c>false</c>.</returns>
		/// <param name="p_scoreEntry">score entry of the user to look for.</param>
		public bool IsAvatarTextureSet(LeaderboardsScoreEntry p_scoreEntry)
		{
			return IsAvatarTextureSet(p_scoreEntry.SteamNative.m_steamIDUser);
		}

		/// <summary>
		/// Most users who didn't set their avatar have a question mark as their avatar texture.
		/// However, Steam allows to check if there is no texture set at all, which will probably never happen.
		/// </summary>
		/// <returns><c>true</c> if the given user has a custom avatar image or uses the default question mark texture as avatar; otherwise, <c>false</c>.</returns>
		/// <param name="p_steamIDUser">user ID to look for.</param>
		public bool IsAvatarTextureSet(CSteamID p_steamIDUser)
		{
			int avatarImageHandle = SteamFriends.GetLargeFriendAvatar(p_steamIDUser);
			return avatarImageHandle != 0;
		}

		/// <summary>
		/// If Steam has already downloaded the user's avatar, then it will be returned as Texture2D.
		/// Otherwise, will return null if the user has no avatar image at all or the image is still loading.
		/// Will start the download of the user's avatar if Steam is not yet loading it.
		/// </summary>
		/// <returns>if Steam has already downloaded the user's avatar, then it will be returned as Texture2D; otherwise, will return null if the user has no avatar image at all or the image is still loading.</returns>
		/// <param name="p_scoreEntry">score entry of the user to look for.</param>
		public Texture2D GetAvatarTexture(LeaderboardsScoreEntry p_scoreEntry)
		{
			return GetAvatarTexture(p_scoreEntry.SteamNative.m_steamIDUser);
		}

		/// <summary>
		/// If Steam has already downloaded the user's avatar, then it will be returned as Texture2D.
		/// Otherwise, will return null if the user has no avatar image at all or the image is still loading.
		/// Will start the download of the user's avatar if Steam is not yet loading it.
		/// </summary>
		/// <returns>if Steam has already downloaded the user's avatar, then it will be returned as Texture2D; otherwise, will return null if the user has no avatar image at all or the image is still loading.</returns>
		/// <param name="p_steamIDUser">user ID to look for.</param>
		public Texture2D GetAvatarTexture(CSteamID p_steamIDUser)
		{
			int avatarImageHandle = SteamFriends.GetLargeFriendAvatar(p_steamIDUser);
			if (avatarImageHandle == -1)
			{
				// -1 => avatar image has not yet been downloaded by Steam -> request
				SteamFriends.RequestUserInformation(p_steamIDUser, false);
			}
			else if (avatarImageHandle != 0) // 0 => this user has no avatar image set
			{
				return GetSteamImageAsTexture2D(avatarImageHandle);
			}
			return null;
		}

		/// <summary>
		/// Formats p_score with SteamLeaderboardsMain.ScoreFormatNumeric or SteamLeaderboardsMain.ScoreFormatSeconds or SteamLeaderboardsMain.ScoreFormatMilliSeconds taking p_scoreType into account.
		/// </summary>
		/// <returns>formatted score string.</returns>
		/// <param name="p_score">integer score value to format.</param>
		/// <param name="p_scoreType">score display type of the target format.</param>
		public string FormatScore(int p_score, ELeaderboardDisplayType p_scoreType)
		{
			try
			{
				switch (p_scoreType)
				{
					case ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeSeconds:
					{
						System.TimeSpan time = System.TimeSpan.FromSeconds(p_score);
						return string.Format(m_scoreFormatSeconds, (int)time.TotalMinutes, time.Seconds, time.Milliseconds);
					}
					case ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeMilliSeconds:
					{
						System.TimeSpan time = System.TimeSpan.FromMilliseconds(p_score);
						return string.Format(m_scoreFormatMilliSeconds, (int)time.TotalMinutes, time.Seconds, time.Milliseconds);
					}
					case ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNone:
					case ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric:
					default:
						return p_score.ToString(m_scoreFormatNumeric);
				}
			}
			catch (System.Exception ex)
			{
				HandleError("FormatScore: invalid format string! ", new ErrorEventArgs(ex.Message));
				return p_score.ToString();
			}
		}

		/// <summary>
		/// Converts an integer array to a string. Can be used to parse additional score details.
		/// </summary>
		/// <returns>the ASCII representation of the int array, expecting the first int to be the character count.</returns>
		/// <param name="p_array">integer array to parse.</param>
		public static string ConvertIntArrayToStr(int[] p_array)
		{
			if (p_array == null || p_array.Length == 0) { return ""; }

			string result = "";
			for (int i = 1; i < p_array.Length; i++)
			{
				byte[] bytes = System.BitConverter.GetBytes(p_array[i]);
				if (p_array[0] > i * 4)
				{
					result += Encoding.ASCII.GetString(bytes);
				}
				else
				{
					result += Encoding.ASCII.GetString(bytes, 0, p_array[0] - ((i - 1) * 4));
				}
			}
			return result;
		}

		/// <summary>
		/// Converts an integer array to a string. Can be used to upload additional score details.
		/// </summary>
		/// <returns>the integer array of the ASCII string, preceded by the character count.</returns>
		/// <param name="p_str">string to encode.</param>
		public static int[] ConvertStrToIntArray(string p_str)
		{
			if (p_str == null || p_str.Length == 0) { return new int[0]; }

			List<int> result = new List<int>();
			result.Add(p_str.Length);
			byte[] bytes = Encoding.ASCII.GetBytes(p_str);
			byte[] bytes4 = new byte[4];
			for (int i = 0; i < bytes.Length; i += 4)
			{
				for (int j = 0; j < 4; j++)
				{
					if (j + i < bytes.Length)
					{
						bytes4[j] = bytes[j + i];
					}
					else
					{
						bytes4[j] = 0;
					}
				}
				result.Add(System.BitConverter.ToInt32(bytes4, 0));
			}
			return result.ToArray();
		}

#endregion


#region MonoBehaviour

		protected override void LateUpdate()
		{
			base.LateUpdate();
			lock(m_lock)
			{
				if (m_scores.Count > 0 && m_scoresMissingUserNames.Count > 0)
				{
					int startMissingUserNamesCount = m_scoresMissingUserNames.Count;
					for (int indexMissScore = m_scoresMissingUserNames.Count - 1; indexMissScore >= 0; indexMissScore--)
					{
						CSteamID userSteamID = m_scoresMissingUserNames[indexMissScore];
						if (!SteamFriends.RequestUserInformation(userSteamID, true)) // request name only, avatars will be requested if needed with GetAvatarTexture
						{
							for (int indexScore = 0; indexScore < m_scores.Count; indexScore++)
							{
								if (m_scores[indexScore].SteamNative.m_steamIDUser == userSteamID)
								{
									m_scores[indexScore].UserName = SteamFriends.GetFriendPersonaName(userSteamID);
									break;
								}
							}
							m_scoresMissingUserNames.RemoveAt(indexMissScore);
						}
					}
					if (IsDebugLogEnabled && startMissingUserNamesCount != m_scoresMissingUserNames.Count)
					{
						Debug.Log("SteamLeaderboardsMain: loaded '" + (startMissingUserNamesCount - m_scoresMissingUserNames.Count) + "' user names, still missing count: '" + m_scoresMissingUserNames.Count + "'");
					}

					if (m_scoresMissingUserNames.Count == 0)
					{
						// all missing names have been loaded
						InvokeEventHandlerSafely(OnDownloadedScores, new LeaderboardsDownloadedScoresEventArgs(m_scoresLeaderboardName, new List<LeaderboardsScoreEntry>(m_scores)));
						ClearSingleShotEventHandlers("OnDownloadedScores", ref OnDownloadedScores);
						m_scores.Clear();
					}
				}
			}
		}

#endregion


#region SteamResultCalls

		private void OnUploadScoreFindOrCreateLeaderboardCallCompleted(string p_leaderboardName, int p_score, ELeaderboardSortMethod p_scoreSorting, ELeaderboardDisplayType p_scoreType, int[] p_scoreDetails, LeaderboardFindResult_t p_callbackFind, bool p_bIOFailureFind)
		{
			EResult callResultType = p_callbackFind.m_bLeaderboardFound == 1 ? EResult.k_EResultOK : EResult.k_EResultFileNotFound;
			if (CheckAndLogResult<LeaderboardFindResult_t, LeaderboardsUploadedScoreEventArgs>("OnUploadScoreFindOrCreateLeaderboardCallCompleted", callResultType, p_bIOFailureFind, GetEventNameForOnUploadedScore(p_leaderboardName, p_score), ref OnUploadedScore))
			{
				// compare sort and type -> warning on mismatch
				ELeaderboardSortMethod sorting = SteamUserStats.GetLeaderboardSortMethod(p_callbackFind.m_hSteamLeaderboard);
				if (sorting != p_scoreSorting)
				{
					Debug.LogWarning("UploadScore: sorting mismatch for leaderboard '"+p_leaderboardName+"' sort mode on Steam is '"+sorting+"', expected '"+p_scoreSorting+"'!");
				}
				ELeaderboardDisplayType type = SteamUserStats.GetLeaderboardDisplayType(p_callbackFind.m_hSteamLeaderboard);
				if (type != p_scoreType)
				{
					Debug.LogWarning("UploadScore: type mismatch for leaderboard '"+p_leaderboardName+"' type on Steam is '"+type+"', expected '"+p_scoreType+"'!");
				}
				// upload score
				if (p_scoreDetails == null) { p_scoreDetails = new int[0]; }
				Execute<LeaderboardScoreUploaded_t>(SteamUserStats.UploadLeaderboardScore(p_callbackFind.m_hSteamLeaderboard, m_scoreUploadMethod, p_score, p_scoreDetails, Mathf.Min(64, p_scoreDetails.Length)), (p_callbackUpload, p_bIOFailureUpload) => OnUploadLeaderboardScoreCallCompleted(p_leaderboardName, p_score, p_callbackUpload, p_bIOFailureUpload));
				if (IsDebugLogEnabled) { Debug.Log("UploadScore: leaderboard '"+p_leaderboardName+"' found, starting score upload"); }
			}
		}

		private void OnUploadLeaderboardScoreCallCompleted(string p_leaderboardName, int p_score, LeaderboardScoreUploaded_t p_callback, bool p_bIOFailure)
		{
			EResult callResultType = p_callback.m_bSuccess == 1 ? EResult.k_EResultOK : EResult.k_EResultUnexpectedError;
			if (CheckAndLogResult<LeaderboardScoreUploaded_t, LeaderboardsUploadedScoreEventArgs>("OnUploadLeaderboardScoreCallCompleted", callResultType, p_bIOFailure, GetEventNameForOnUploadedScore(p_leaderboardName, p_score), ref OnUploadedScore))
			{
				// inform listeners
				if (OnUploadedScore != null)
				{
					ELeaderboardDisplayType scoreType = SteamUserStats.GetLeaderboardDisplayType(p_callback.m_hSteamLeaderboard);
					InvokeEventHandlerSafely(OnUploadedScore, new LeaderboardsUploadedScoreEventArgs()
					{
						LeaderboardName = p_leaderboardName,
						Score = p_callback.m_nScore,
						ScoreString = FormatScore(p_callback.m_nScore, scoreType),
						ScoreType = scoreType,
						IsScoreChanged = p_callback.m_bScoreChanged == 1,
						GlobalRankNew = p_callback.m_nGlobalRankNew,
						GlobalRankPrevious = p_callback.m_nGlobalRankPrevious,
						SteamNative = new LeaderboardsUploadedScoreEventArgs.SteamNativeData(p_callback.m_hSteamLeaderboard)
					});
					ClearSingleShotEventHandlers(GetEventNameForOnUploadedScore(p_leaderboardName, p_score), ref OnUploadedScore);
				}
			}
		}

		private string GetEventNameForOnUploadedScore(string p_leaderboardName, int p_score)
		{
			return "OnUploadedScore" + p_leaderboardName + p_score;
		}

		private void OnDownloadScoresFindLeaderboardCallCompleted(string p_leaderboardName, ELeaderboardDataRequest p_scoreSource, int p_rangeStart, int p_rangeEnd, LeaderboardFindResult_t p_callbackFind, bool p_bIOFailureFind)
		{
			EResult callResultType = p_callbackFind.m_bLeaderboardFound == 1 ? EResult.k_EResultOK : EResult.k_EResultFileNotFound;
			if (CheckAndLogResult<LeaderboardFindResult_t, LeaderboardsDownloadedScoresEventArgs>("OnDownloadScoresFindLeaderboardCallCompleted", callResultType, p_bIOFailureFind, "OnDownloadedScores", ref OnDownloadedScores))
			{
				// download scores
				Execute<LeaderboardScoresDownloaded_t>(SteamUserStats.DownloadLeaderboardEntries(p_callbackFind.m_hSteamLeaderboard, p_scoreSource, p_rangeStart, p_rangeEnd), (p_callbackDownload, p_bIOFailureDownload) => OnDownloadLeaderboardEntriesCallCompleted(p_leaderboardName, p_callbackDownload, p_bIOFailureDownload));
				if (IsDebugLogEnabled) { Debug.Log("DownloadScores: leaderboard '"+p_leaderboardName+"' found, starting scores download"); }
			}
		}

		private void OnDownloadLeaderboardEntriesCallCompleted(string p_leaderboardName, LeaderboardScoresDownloaded_t p_callback, bool p_bIOFailure)
		{
			if (CheckAndLogResult<LeaderboardScoresDownloaded_t, LeaderboardsDownloadedScoresEventArgs>("OnDownloadLeaderboardEntriesCallCompleted", EResult.k_EResultOK, p_bIOFailure, "OnDownloadedScores", ref OnDownloadedScores))
			{
				if (OnDownloadedScores != null)
				{
					lock(m_lock)
					{
						// get score list
						m_scores.Clear();
						m_scoresMissingUserNames.Clear();
						m_scoresLeaderboardName = p_leaderboardName;
						for (int i = 0; i < p_callback.m_cEntryCount; i++)
						{
							LeaderboardEntry_t entry;
							int[] details = new int[Mathf.Max(0, m_scoreDownloadDetailsLength)];
							if (SteamUserStats.GetDownloadedLeaderboardEntry(p_callback.m_hSteamLeaderboardEntries, i, out entry, details, details.Length))
							{
								if (SteamFriends.RequestUserInformation(entry.m_steamIDUser, true)) // request name only, avatars will be requested if needed with GetAvatarTexture
								{
									m_scoresMissingUserNames.Add(entry.m_steamIDUser);
								}
								int[] detailsDownloaded = new int[Mathf.Min(details.Length, entry.m_cDetails)];
								System.Array.Copy(details, detailsDownloaded, detailsDownloaded.Length);
								string userName = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser);
								ELeaderboardDisplayType scoreType = SteamUserStats.GetLeaderboardDisplayType(p_callback.m_hSteamLeaderboard);
								LeaderboardsScoreEntry parsedEntry = new LeaderboardsScoreEntry()
								{
									LeaderboardName = p_leaderboardName,
									UserName = userName,
									GlobalRank = entry.m_nGlobalRank,
									Score = entry.m_nScore,
									ScoreString = FormatScore(entry.m_nScore, scoreType),
									ScoreType = scoreType,
									DetailsAvailableLength = entry.m_cDetails,
									DetailsDownloaded = detailsDownloaded,
									IsCurrentUserScore = entry.m_steamIDUser == SteamUser.GetSteamID(),
									SteamNative = new LeaderboardsScoreEntry.SteamNativeData(p_callback.m_hSteamLeaderboard, entry.m_hUGC, entry.m_steamIDUser)
								};
								m_scores.Add(parsedEntry);
							}
						}
					}
					// inform listeners
					if (m_scoresMissingUserNames.Count == 0)
					{
						InvokeEventHandlerSafely(OnDownloadedScores, new LeaderboardsDownloadedScoresEventArgs(p_leaderboardName, new List<LeaderboardsScoreEntry>(m_scores)));
						ClearSingleShotEventHandlers("OnDownloadedScores", ref OnDownloadedScores);
					}
					else if (IsDebugLogEnabled)
					{
						Debug.Log("OnDownloadLeaderboardEntriesCallCompleted: missing user names count: '" + m_scoresMissingUserNames.Count + "'");
					}
				}
			}
		}

#endregion


#region InternalLogic

		private Texture2D GetSteamImageAsTexture2D(int p_imageID)
		{
			Texture2D texture = null;
			uint imageWidth;
			uint imageHeight;
			
			if (SteamUtils.GetImageSize(p_imageID, out imageWidth, out imageHeight))
			{
				byte[] imageBytes = new byte[imageWidth * imageHeight * 4];
				if (SteamUtils.GetImageRGBA(p_imageID, imageBytes, imageBytes.Length))
				{
					// vertically mirror image
					int byteWidth = (int)imageWidth * 4;
					byte[] imageBytesFlipped = new byte[imageBytes.Length];
					for (int i = 0, j = imageBytes.Length - byteWidth; i < imageBytes.Length; i += byteWidth, j -= byteWidth)
					{
						for (int k = 0; k < byteWidth; k++)
						{
							imageBytesFlipped[i + k] = imageBytes[j + k];
						}
					}
					// create Unity Texture2D
					texture = new Texture2D((int)imageWidth, (int)imageHeight, TextureFormat.RGBA32, false, true);
					texture.LoadRawTextureData(imageBytesFlipped);
					texture.Apply();
				}
			}
			
			return texture;
		}

#endregion
	}
}
