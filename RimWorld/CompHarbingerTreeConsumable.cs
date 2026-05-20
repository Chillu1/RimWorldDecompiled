using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompHarbingerTreeConsumable : ThingComp
{
	private const int ConsumeInterval = 5000;

	private static readonly IntRange ItemConsumeCountRange = new IntRange(4, 12);

	private int ticksPassed;

	private readonly List<HarbingerTree> tmpTreesInRange = new List<HarbingerTree>();

	public bool BeingConsumed => !tmpTreesInRange.Empty();

	private Corpse Corpse => parent as Corpse;

	public bool CanBeConsumed
	{
		get
		{
			if (!ModsConfig.AnomalyActive)
			{
				return false;
			}
			if (!parent.Spawned)
			{
				return false;
			}
			return parent.GetSlotGroup()?.parent.Isnt<Building>() ?? true;
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref ticksPassed, "ticksPassed", 0);
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		LongEventHandler.ExecuteWhenFinished(ConsumedRootsLazy);
	}

	private void ConsumedRootsLazy()
	{
		if (!CanBeConsumed)
		{
			return;
		}
		tmpTreesInRange.Clear();
		foreach (Thing item in parent.Map.listerThings.ThingsOfDef(ThingDefOf.Plant_TreeHarbinger))
		{
			HarbingerTree harbingerTree = item as HarbingerTree;
			if (harbingerTree.Position.InHorDistOf(parent.Position, harbingerTree.ConsumeRadius))
			{
				tmpTreesInRange.Add(harbingerTree);
			}
		}
	}

	public override void CompTick()
	{
		if (parent.IsHashIntervalTick(250))
		{
			CompTickRare();
		}
	}

	public override void CompTickRare()
	{
		if (!CanBeConsumed)
		{
			return;
		}
		ConsumedRootsLazy();
		if (!BeingConsumed)
		{
			return;
		}
		ticksPassed += 250;
		if (AvailableNutrition(applyDigestion: false) > 0f)
		{
			EffecterDefOf.HarbingerTreeWillConsume.Spawn(parent.Position, parent.Map);
		}
		if (ticksPassed >= 5000)
		{
			Map map = parent.Map;
			if (TryConsume())
			{
				EffecterDefOf.HarbingerTreeConsume.Spawn(parent.Position, map);
			}
			ticksPassed -= 5000;
		}
	}

	public float AvailableNutrition(bool applyDigestion)
	{
		if (Corpse != null && !Corpse.InnerPawn.RaceProps.IsFlesh)
		{
			return 0f;
		}
		float num = 0f;
		if (Corpse != null)
		{
			return GetNutritionFromCorpse(Corpse, applyDigestion);
		}
		if (parent.def.category == ThingCategory.Item)
		{
			return GetNutritionFromItemStack(parent, applyDigestion);
		}
		return 1f;
	}

	private bool TryConsume()
	{
		float num = AvailableNutrition(applyDigestion: true);
		if (num == 0f)
		{
			return false;
		}
		foreach (HarbingerTree item in tmpTreesInRange)
		{
			item.AddNutrition(num / (float)tmpTreesInRange.Count);
		}
		return true;
	}

	private static float GetNutritionFromCorpse(Corpse corpse, bool applyDigestion)
	{
		(from x in corpse.InnerPawn.health.hediffSet.GetNotMissingParts()
			where x.depth == BodyPartDepth.Outside && !x.def.conceptual && x != corpse.InnerPawn.RaceProps.body.corePart
			select x).TryRandomElement(out var result);
		float bodyPartNutrition;
		if (result == null)
		{
			bodyPartNutrition = FoodUtility.GetBodyPartNutrition(corpse, corpse.InnerPawn.RaceProps.body.corePart);
			if (applyDigestion)
			{
				corpse.Destroy();
			}
		}
		else
		{
			bodyPartNutrition = FoodUtility.GetBodyPartNutrition(corpse, result);
			if (applyDigestion)
			{
				Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, corpse.InnerPawn, result);
				hediff_MissingPart.IsFresh = true;
				hediff_MissingPart.lastInjury = HediffDefOf.Digested;
				corpse.InnerPawn.health.AddHediff(hediff_MissingPart);
			}
		}
		return bodyPartNutrition;
	}

	private static float GetNutritionFromItemStack(ThingWithComps item, bool applyDigestion)
	{
		int num = Mathf.Min(item.stackCount, ItemConsumeCountRange.RandomInRange);
		float num2 = 0f;
		if (applyDigestion)
		{
			Thing thing = item.SplitOff(num);
			num2 = thing.GetStatValue(StatDefOf.Nutrition) * (float)num;
			thing.Destroy();
		}
		else
		{
			num2 = item.GetStatValue(StatDefOf.Nutrition) * ((float)num / (float)item.stackCount);
		}
		return num2;
	}

	public override string CompInspectStringExtra()
	{
		if (BeingConsumed)
		{
			return "ConsumedByHarbingerTree".Translate();
		}
		return "";
	}
}
