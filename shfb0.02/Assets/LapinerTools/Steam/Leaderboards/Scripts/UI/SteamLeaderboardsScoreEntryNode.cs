using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using System.Collections;

using LapinerTools.Steam.Data;

namespace LapinerTools.Steam.UI
{
	/// <summary>
	/// The SteamLeaderboardsUI class will use the SteamLeaderboardsScoreEntryNode class to display single items in the list.
	/// The LeaderboardListEntry prefab has this script attached. The LeaderboardListEntry prefab is referenced by the SteamLeaderboard prefab.
	/// There are two options to customize the item UI:<br />1. Listen to SteamLeaderboardsUI.OnEntryDataSet event and modify UI when it is triggered.<br />2.
	/// Derive from this class to customize the item UI. Keep in mind to change the script component of the LeaderboardListEntry prefab to your deriving class.
	/// Override SteamLeaderboardsScoreEntryNode.uMyGUI_TreeBrowser_InitNode to apply your customization, e.g. new entries such as a replay button.
	/// </summary>
	public class SteamLeaderboardsScoreEntryNode : MonoBehaviour, IScrollHandler
	{
		/// This is the argument of the SteamLeaderboardsUI.OnEntryDataSet event.
		/// Use the EntryDataSetEventArgs.EntryUI property to find children of your customized UI e.g. replay button.
		/// Use the ItemDataSetEventArgs.EntryData property to get Steam data of the score entry.
		public class EntryDataSetEventArgs : EventArgsBase
		{
			/// <summary>
			/// The Steam data of the score entry.
			/// </summary>
			public LeaderboardsScoreEntry EntryData { get; set; }
			
			/// <summary>
			/// The uGUI object reference of the score entry. Can be used to find children of customized UI e.g. replay button.
			/// </summary>
			/// <value>The item U.</value>
			public SteamLeaderboardsScoreEntryNode EntryUI { get; set; }
		}

		/// Internal class used by SteamLeaderboardsUI to pass Steam score data to the SteamLeaderboardsScoreEntryNode class.
		public class SendMessageInitData
		{
			/// <summary>
			/// The Steam data of the score entry.
			/// </summary>
			public LeaderboardsScoreEntry ScoreEntry { get; set; }
		}
		
		[SerializeField]
		protected Text m_textRank;

		[SerializeField]
		protected Text m_textUserName;
		
		[SerializeField]
		protected Text m_textScore;

		[SerializeField]
		protected RawImage m_image;
		public RawImage Image { get{ return m_image; } }

		protected ScrollRect m_parentScroller = null;
		protected Texture2D m_avatarTexture = null;

		/// <summary>
		/// Called from the SteamLeaderboardsUI class to initialze the item UI.
		/// </summary>
		/// <param name="p_data">is of type SendMessageInitData. Contains the Steam data of the score entry.</param>
		public virtual void uMyGUI_TreeBrowser_InitNode(object p_data)
		{
			if (p_data is SendMessageInitData)
			{
				SendMessageInitData data = (SendMessageInitData)p_data;
				// avatar image
				if (m_image != null) { StartCoroutine(LoadAvatarTexture(data.ScoreEntry)); }
				// highlight if this is the score of the current player
				string textFormat = data.ScoreEntry.IsCurrentUserScore ? "<color=lime>{0}</color>" : "{0}";
				// user name, rank and score
				if (m_textUserName != null) { m_textUserName.text = string.Format(textFormat, data.ScoreEntry.UserName); }
				if (m_textRank != null) { m_textRank.text = string.Format(textFormat, data.ScoreEntry.GlobalRank); }
				if (m_textScore != null) { m_textScore.text = string.Format(textFormat, data.ScoreEntry.ScoreString); }

				// invoke event
				if (SteamLeaderboardsUI.Instance != null)
				{
					SteamLeaderboardsUI.Instance.InvokeOnEntryDataSet(data.ScoreEntry, this);
				}
			}
			else
			{
				Debug.LogError("SteamLeaderboardsScoreEntryNode: uMyGUI_TreeBrowser_InitNode: expected p_data to be a SteamLeaderboardsScoreEntryNode.SendMessageInitData! p_data: " + p_data);
			}
		}

		/// <summary>
		/// Internal method implementing the IScrollHandler interface. Required for mouse wheel scrolling of the item list.
		/// </summary>
		/// <param name="data">mouse wheel event data.</param>
		public virtual void OnScroll(PointerEventData data)
		{
			// try to find the parent ScrollRect
			if (m_parentScroller == null)
			{
				m_parentScroller = GetComponentInParent<ScrollRect>();
			}
			
			// cannot do anything without a parent ScrollRect -> return
			if (m_parentScroller == null)
			{
				return;
			}
			
			// forward the scroll event data to the parent
			m_parentScroller.OnScroll(data);
		}

		protected void OnDestroy()
		{
			if (m_avatarTexture != null) { Destroy(m_avatarTexture); }
		}

		protected virtual IEnumerator LoadAvatarTexture(LeaderboardsScoreEntry p_entry)
		{
			// if the user of this score has no avatar image set, then do nothing
			bool isAvatarLoaded = !SteamLeaderboardsMain.Instance.IsAvatarTextureSet(p_entry);
			while (!isAvatarLoaded)
			{
				if (m_avatarTexture != null) { Destroy(m_avatarTexture); }
				// Steam will load the avatar image asynchronously -> check if it is already loaded and repeat later if it is not yet available
				m_avatarTexture = SteamLeaderboardsMain.Instance.GetAvatarTexture(p_entry);
				if (m_avatarTexture != null)
				{
					isAvatarLoaded = true;
					if (m_image != null) // might have been destroyed in the meantime -> check again
					{
						m_image.texture = m_avatarTexture;
					}
				}
				yield return new WaitForSeconds(0.35f);
			}
		}
	}
}
