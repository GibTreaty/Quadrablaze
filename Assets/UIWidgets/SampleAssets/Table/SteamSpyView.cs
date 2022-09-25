using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UIWidgets;

namespace UIWidgetsSamples
{
	/// <summary>
	/// SteamSpy sort fields.
	/// </summary>
	public enum SteamSpySortFields
	{
		/// <summary>
		/// Name.
		/// </summary>
		Name,

		/// <summary>
		/// ScoreRank.
		/// </summary>
		ScoreRank,

		/// <summary>
		/// Owners.
		/// </summary>
		Owners,

		/// <summary>
		/// Players.
		/// </summary>
		Players,

		/// <summary>
		/// PlayersIn2Week.
		/// </summary>
		PlayersIn2Week,

		/// <summary>
		/// Time.
		/// </summary>
		Time,
	}

	/// <summary>
	/// SteamSpyView.
	/// </summary>
	public class SteamSpyView : ListViewCustomHeight<SteamSpyComponent,SteamSpyItem>
	{
		bool isSteamSpyViewStarted;

		/// <summary>
		/// Start this instance.
		/// </summary>
		public override void Start()
		{
			if (isSteamSpyViewStarted)
			{
				return ;
			}

			isSteamSpyViewStarted = true;

			sortComparers = new Dictionary<int,Comparison<SteamSpyItem>>(){
				{(int)SteamSpySortFields.Name, NameComparer},
				{(int)SteamSpySortFields.ScoreRank, ScoreRankComparer},
				{(int)SteamSpySortFields.Owners, OwnersComparer},
				{(int)SteamSpySortFields.Players, PlayersComparer},
				{(int)SteamSpySortFields.PlayersIn2Week, PlayersIn2WeekComparer},
				{(int)SteamSpySortFields.Time, TimeComparer},
			};

			Sort = false;
			base.Start();

			StartCoroutine(LoadData());
		}

		/// <summary>
		/// Handle enable event.
		/// </summary>
		public override void OnEnable()
		{
			base.OnEnable();
			if (isSteamSpyViewStarted && DataSource.Count==0)
			{
				StartCoroutine(LoadData());
			}
		}

		IEnumerator LoadData()
		{
			WWW www = new WWW("https://ilih.ru/steamspy/");
			yield return www;

			var lines = www.text.Split('\n');

			www.Dispose();

			DataSource.BeginUpdate();

			DataSource.Clear();

			lines.ForEach(ParseLine);

			DataSource.EndUpdate();
		}

		void ParseLine(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				return ;
			}
			var info = line.Split('\t');

			var item = new SteamSpyItem(){
				Name = info[0],
				ScoreRank = (string.IsNullOrEmpty(info[1])) ? -1 : int.Parse(info[1]),

				Owners = int.Parse(info[2]),
				OwnersVariance = int.Parse(info[3]),

				Players = int.Parse(info[4]),
				PlayersVariance = int.Parse(info[5]),

				PlayersIn2Week = int.Parse(info[6]),
				PlayersIn2WeekVariance = int.Parse(info[7]),

				AverageTimeIn2Weeks = int.Parse(info[8]),
				MedianTimeIn2Weeks = int.Parse(info[9]),
			};
			DataSource.Add(item);
		}

		SteamSpySortFields currentSortField = SteamSpySortFields.Players;

		Dictionary<int,Comparison<SteamSpyItem>> sortComparers;

		/// <summary>
		/// Toggle sort.
		/// </summary>
		/// <param name="field">Sort field.</param>
		public void ToggleSort(SteamSpySortFields field)
		{
			if (field==currentSortField)
			{
				DataSource.Reverse();
			}
			else if (sortComparers.ContainsKey((int)field))
			{
				currentSortField = field;

				DataSource.Sort(sortComparers[(int)field]);
			}
		}

		#region used in Button.OnClick()
		/// <summary>
		/// Sort by Name.
		/// </summary>
		public void SortByName()
		{
			ToggleSort(SteamSpySortFields.Name);
		}

		/// <summary>
		/// Sort by ScoreRank.
		/// </summary>
		public void SortByScoreRank()
		{
			ToggleSort(SteamSpySortFields.ScoreRank);
		}

		/// <summary>
		/// Sort by Owners.
		/// </summary>
		public void SortByOwners()
		{
			ToggleSort(SteamSpySortFields.Owners);
		}

		/// <summary>
		/// Sort by Players.
		/// </summary>
		public void SortByPlayers()
		{
			ToggleSort(SteamSpySortFields.Players);
		}

		/// <summary>
		/// Sort by PlayersIn2Week.
		/// </summary>
		public void SortByPlayersIn2Week()
		{
			ToggleSort(SteamSpySortFields.PlayersIn2Week);
		}

		/// <summary>
		/// Sort by Time.
		/// </summary>
		public void SortByTime()
		{
			ToggleSort(SteamSpySortFields.Time);
		}
		#endregion

		#region Items comparers
		/// <summary>
		/// Name comparer.
		/// </summary>
		/// <param name="x">First SteamSpyItem.</param>
		/// <param name="y">Second SteamSpyItem.</param>
		/// <returns>A 32-bit signed integer that indicates whether X precedes, follows, or appears in the same position in the sort order as the Y parameter.</returns>
		static protected int NameComparer(SteamSpyItem x, SteamSpyItem y)
		{
			return x.Name.CompareTo(y.Name);
		}

		/// <summary>
		/// ScoreRank comparer.
		/// </summary>
		/// <param name="x">First SteamSpyItem.</param>
		/// <param name="y">Second SteamSpyItem.</param>
		/// <returns>A 32-bit signed integer that indicates whether X precedes, follows, or appears in the same position in the sort order as the Y parameter.</returns>
		static protected int ScoreRankComparer(SteamSpyItem x, SteamSpyItem y)
		{
			return x.ScoreRank.CompareTo(y.ScoreRank);
		}

		/// <summary>
		/// Owners comparer.
		/// </summary>
		/// <param name="x">First SteamSpyItem.</param>
		/// <param name="y">Second SteamSpyItem.</param>
		/// <returns>A 32-bit signed integer that indicates whether X precedes, follows, or appears in the same position in the sort order as the Y parameter.</returns>
		static protected int OwnersComparer(SteamSpyItem x, SteamSpyItem y)
		{
			return x.Owners.CompareTo(y.Owners);
		}

		/// <summary>
		/// Players comparer.
		/// </summary>
		/// <param name="x">First SteamSpyItem.</param>
		/// <param name="y">Second SteamSpyItem.</param>
		/// <returns>A 32-bit signed integer that indicates whether X precedes, follows, or appears in the same position in the sort order as the Y parameter.</returns>
		static protected int PlayersComparer(SteamSpyItem x, SteamSpyItem y)
		{
			return x.Players.CompareTo(y.Players);
		}

		/// <summary>
		/// PlayersIn2Week comparer.
		/// </summary>
		/// <param name="x">First SteamSpyItem.</param>
		/// <param name="y">Second SteamSpyItem.</param>
		/// <returns>A 32-bit signed integer that indicates whether X precedes, follows, or appears in the same position in the sort order as the Y parameter.</returns>
		static protected int PlayersIn2WeekComparer(SteamSpyItem x, SteamSpyItem y)
		{
			return x.PlayersIn2Week.CompareTo(y.PlayersIn2Week);
		}

		/// <summary>
		/// AverageTimeIn2Weeks comparer.
		/// </summary>
		/// <param name="x">First SteamSpyItem.</param>
		/// <param name="y">Second SteamSpyItem.</param>
		/// <returns>A 32-bit signed integer that indicates whether X precedes, follows, or appears in the same position in the sort order as the Y parameter.</returns>
		static protected int TimeComparer(SteamSpyItem x, SteamSpyItem y)
		{
			return x.AverageTimeIn2Weeks.CompareTo(y.AverageTimeIn2Weeks);
		}
		#endregion
	}
}