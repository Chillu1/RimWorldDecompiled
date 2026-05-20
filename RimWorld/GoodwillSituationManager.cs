using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GoodwillSituationManager
{
	public struct CachedSituation
	{
		public GoodwillSituationDef def;

		public int maxGoodwill;

		public int naturalGoodwillOffset;
	}

	private Dictionary<Faction, List<CachedSituation>> cachedData = new Dictionary<Faction, List<CachedSituation>>();

	private const int RecacheEveryTicks = 1000;

	public List<CachedSituation> GetSituations(Faction other)
	{
		if (other == null || other.IsPlayer)
		{
			Log.Error("Called GetSituations() for faction " + other);
			return null;
		}
		if (cachedData.TryGetValue(other, out var value))
		{
			return value;
		}
		Recalculate(other, canSendHostilityChangedLetter: true);
		return cachedData[other];
	}

	public int GetMaxGoodwill(Faction other)
	{
		List<CachedSituation> situations = GetSituations(other);
		int num = 100;
		for (int i = 0; i < situations.Count; i++)
		{
			num = Mathf.Min(num, situations[i].maxGoodwill);
		}
		return num;
	}

	public int GetNaturalGoodwill(Faction other)
	{
		List<CachedSituation> situations = GetSituations(other);
		int num = 0;
		for (int i = 0; i < situations.Count; i++)
		{
			num += situations[i].naturalGoodwillOffset;
		}
		return num;
	}

	public string GetExplanation(Faction other)
	{
		if (other == null || other == Faction.OfPlayer)
		{
			Log.Error("Tried to get CachedGoodwillData explanation for faction " + other);
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		List<CachedSituation> situations = GetSituations(other);
		for (int i = 0; i < situations.Count; i++)
		{
			stringBuilder.AppendInNewLine(situations[i].def.LabelCap);
		}
		return stringBuilder.ToString();
	}

	public void GoodwillManagerTick()
	{
		if (Find.TickManager.TicksGame % 1000 == 0)
		{
			RecalculateAll(canSendHostilityChangedLetter: true);
		}
	}

	public void RecalculateAll(bool canSendHostilityChangedLetter)
	{
		List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
		for (int i = 0; i < allFactionsListForReading.Count; i++)
		{
			if (allFactionsListForReading[i] != Faction.OfPlayer && allFactionsListForReading[i].HasGoodwill)
			{
				Recalculate(allFactionsListForReading[i], canSendHostilityChangedLetter);
			}
		}
	}

	private void Recalculate(Faction other, bool canSendHostilityChangedLetter)
	{
		if (cachedData.TryGetValue(other, out var value))
		{
			Recalculate(other, value);
		}
		else
		{
			List<CachedSituation> list = new List<CachedSituation>();
			Recalculate(other, list);
			cachedData.Add(other, list);
		}
		CheckHostilityChanged(other, canSendHostilityChangedLetter);
	}

	private void Recalculate(Faction other, List<CachedSituation> outSituations)
	{
		outSituations.Clear();
		if (!other.HasGoodwill)
		{
			return;
		}
		List<GoodwillSituationDef> allDefsListForReading = DefDatabase<GoodwillSituationDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			int maxGoodwill = allDefsListForReading[i].Worker.GetMaxGoodwill(other);
			int naturalGoodwillOffset = allDefsListForReading[i].Worker.GetNaturalGoodwillOffset(other);
			if (maxGoodwill < 100 || naturalGoodwillOffset != 0)
			{
				outSituations.Add(new CachedSituation
				{
					def = allDefsListForReading[i],
					maxGoodwill = maxGoodwill,
					naturalGoodwillOffset = naturalGoodwillOffset
				});
			}
		}
	}

	private void CheckHostilityChanged(Faction other, bool canSendHostilityChangedLetter)
	{
		if (Current.ProgramState != ProgramState.Entry && other.HasGoodwill)
		{
			Faction.OfPlayer.Notify_GoodwillSituationsChanged(other, canSendHostilityChangedLetter, null, null);
		}
	}
}
