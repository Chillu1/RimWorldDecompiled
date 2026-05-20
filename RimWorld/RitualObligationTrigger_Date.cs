using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RitualObligationTrigger_Date : RitualObligationTrigger
{
	public int triggerDaysSinceStartOfYear;

	public string DateString => GenDate.QuadrumDateStringAt(triggerDaysSinceStartOfYear * 60000, 0f);

	public override void Init(RitualObligationTriggerProperties props)
	{
		base.Init(props);
		triggerDaysSinceStartOfYear = RandomDate();
	}

	public int RandomDate()
	{
		List<int> list = new List<int>();
		foreach (Precept item in ritual.ideo.PreceptsListForReading)
		{
			if (item == ritual || !(item is Precept_Ritual precept_Ritual))
			{
				continue;
			}
			foreach (RitualObligationTrigger obligationTrigger in precept_Ritual.obligationTriggers)
			{
				if (obligationTrigger is RitualObligationTrigger_Date ritualObligationTrigger_Date)
				{
					list.Add(ritualObligationTrigger_Date.triggerDaysSinceStartOfYear);
				}
			}
		}
		List<int> source = Enumerable.Range(0, 60).Except(list).ToList();
		int num = 20;
		bool flag = false;
		int num2 = 0;
		while (num >= 5 && !flag)
		{
			for (int i = 0; i < 10; i++)
			{
				num2 = source.RandomElement();
				bool flag2 = false;
				for (int j = 0; j < list.Count; j++)
				{
					int num3 = list[j];
					int num4 = Mathf.Abs(num2 - num3);
					if (Mathf.Min(num4, 59 - num4) < num)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					flag = true;
					break;
				}
			}
			num -= 5;
		}
		return num2;
	}

	public override void Tick()
	{
		if (!ritual.isAnytime)
		{
			int num = CurrentTickRelative();
			int num2 = OccursOnTick();
			if ((!mustBePlayerIdeo || Faction.OfPlayer.ideos.Has(ritual.ideo)) && num == num2)
			{
				ritual.AddObligation(new RitualObligation(ritual));
			}
		}
	}

	public int CurrentTickRelative()
	{
		Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
		long num = GenDate.LocalTicksOffsetFromLongitude((anyPlayerHomeMap != null) ? Find.WorldGrid.LongLatOf(anyPlayerHomeMap.Tile).x : 0f);
		return (int)(GenTicks.TicksAbs % 3600000 + num);
	}

	public int OccursOnTick()
	{
		int num = triggerDaysSinceStartOfYear * 60000;
		Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
		float longitude = ((anyPlayerHomeMap != null) ? Find.WorldGrid.LongLatOf(anyPlayerHomeMap.Tile).x : 0f);
		return num + (int)GenDate.LocalTicksOffsetFromLongitude(longitude);
	}

	public override void CopyTo(RitualObligationTrigger other)
	{
		base.CopyTo(other);
		((RitualObligationTrigger_Date)other).triggerDaysSinceStartOfYear = triggerDaysSinceStartOfYear;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref triggerDaysSinceStartOfYear, "triggerDaysSinceStartOfYear", 0);
	}
}
