using System.Collections.Generic;
using System.Linq;
using RimWorld.QuestGen;
using Verse;

namespace RimWorld;

public class QuestPart_SubquestGenerator_Gravcores : QuestPart_SubquestGenerator
{
	private float lastSubquestTick;

	private bool givenFirstSubquest;

	private const int MTBSubquestsTicks = 300000;

	private const int MTBFirstSubquestTicks = 120000;

	private const int MinTimeBetweenSubquests = 900000;

	private const int MaxTimeBetweenSubquests = 1800000;

	private const int MinTimeFirstSubquest = 300000;

	private const int MaxTimeFirstSubquest = 480000;

	private List<QuestScriptDef> tmpSubquestDefs = new List<QuestScriptDef>();

	private int MTB
	{
		get
		{
			if (!givenFirstSubquest)
			{
				return 120000;
			}
			return 300000;
		}
	}

	private int MinTime
	{
		get
		{
			if (!givenFirstSubquest)
			{
				return 300000;
			}
			return 900000;
		}
	}

	private int MaxTime
	{
		get
		{
			if (!givenFirstSubquest)
			{
				return 480000;
			}
			return 1800000;
		}
	}

	protected override bool CanGenerateSubquest
	{
		get
		{
			if (!base.CanGenerateSubquest)
			{
				return false;
			}
			if ((float)Find.TickManager.TicksGame - lastSubquestTick < (float)MinTime)
			{
				return false;
			}
			foreach (Map map in Find.Maps)
			{
				if (map.listerBuildings.ColonistsHaveBuilding(ThingDefOf.GravEngine))
				{
					return true;
				}
			}
			return false;
		}
	}

	protected override QuestScriptDef GetNextSubquestDef()
	{
		tmpSubquestDefs.Clear();
		GetPossibleSubquests(tmpSubquestDefs);
		if (tmpSubquestDefs.TryRandomElement(out var result))
		{
			return result;
		}
		return null;
	}

	private void GetPossibleSubquests(List<QuestScriptDef> outList)
	{
		IEnumerable<Quest> subquests = quest.GetSubquests();
		IEnumerable<Quest> source = subquests.Where((Quest x) => x.State == QuestState.Ongoing);
		IEnumerable<Quest> source2 = subquests.Where((Quest x) => x.State == QuestState.EndedSuccess);
		int num = source.Count() + source2.Count();
		bool flag = source.Any((Quest x) => x.root == QuestScriptDefOf.Gravcore_Mechhive) || source2.Any((Quest x) => x.root == QuestScriptDefOf.Gravcore_Mechhive);
		bool flag2 = flag && num >= subquestDefs.Count;
		foreach (QuestScriptDef def in subquestDefs)
		{
			if (def.root is QuestNode_Root_Gravcore questNode_Root_Gravcore && questNode_Root_Gravcore.requiredSubquestsGiven <= num && (flag2 || (!source2.Any((Quest x) => x.root == def) && !source.Any((Quest x) => x.root == def))) && (!flag || def != QuestScriptDefOf.Gravcore_Mechhive) && def.CanRun(InitSlate(), Find.World))
			{
				outList.Add(def);
			}
		}
	}

	protected override bool TryGenerateSubquest()
	{
		bool num = base.TryGenerateSubquest();
		if (num)
		{
			lastSubquestTick = Find.TickManager.TicksGame;
			givenFirstSubquest = true;
			return num;
		}
		Log.Warning("Failed to generate gravcore subquest, trying again in 6 hours");
		lastSubquestTick += 15000f;
		return num;
	}

	protected override Slate InitSlate()
	{
		float var = ((Find.AnyPlayerHomeMap == null) ? StorytellerUtility.DefaultThreatPointsNow(Find.World) : StorytellerUtility.DefaultThreatPointsNow(Find.AnyPlayerHomeMap));
		Slate slate = new Slate();
		slate.Set("points", var);
		return slate;
	}

	public override void QuestPartTick()
	{
		if (subquestDefs.Count != 0 && !base.Paused && CanGenerateSubquest && (Rand.MTBEventOccurs(MTB, 1f, 1f) || (float)Find.TickManager.TicksGame > lastSubquestTick + (float)MaxTime))
		{
			TryGenerateSubquest();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref lastSubquestTick, "lastSubquestTick", 0f);
		Scribe_Values.Look(ref givenFirstSubquest, "givenFirstSubquest", defaultValue: false);
	}
}
