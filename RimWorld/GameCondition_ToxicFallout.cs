using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GameCondition_ToxicFallout : GameCondition
{
	private const float MaxSkyLerpFactor = 0.5f;

	private const float SkyGlow = 0.85f;

	private SkyColorSet ToxicFalloutColors = new SkyColorSet(new ColorInt(216, 255, 0).ToColor, new ColorInt(234, 200, 255).ToColor, new Color(0.6f, 0.8f, 0.5f), 0.85f);

	private readonly List<SkyOverlay> overlays = new List<SkyOverlay>
	{
		new WeatherOverlay_Fallout()
	};

	private const float PlantKillChance = 0.0065f;

	private const float CorpseRotProgressAdd = 3000f;

	public override int TransitionTicks => 5000;

	public override void Init()
	{
		LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
		LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Critical);
	}

	public override void GameConditionTick()
	{
		List<Map> affectedMaps = base.AffectedMaps;
		if (Find.TickManager.TicksGame % 3451 == 0)
		{
			for (int i = 0; i < affectedMaps.Count; i++)
			{
				DoPawnsToxicDamage(affectedMaps[i]);
			}
		}
		for (int j = 0; j < overlays.Count; j++)
		{
			for (int k = 0; k < affectedMaps.Count; k++)
			{
				overlays[j].TickOverlay(affectedMaps[k], 1f);
			}
		}
	}

	private void DoPawnsToxicDamage(Map map)
	{
		IReadOnlyList<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < allPawnsSpawned.Count; i++)
		{
			if (!allPawnsSpawned[i].kindDef.immuneToGameConditionEffects)
			{
				ToxicUtility.DoAirbornePawnToxicDamage(allPawnsSpawned[i]);
			}
		}
	}

	public override void DoCellSteadyEffects(IntVec3 c, Map map)
	{
		if (c.Roofed(map))
		{
			return;
		}
		List<Thing> thingList = c.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			Thing thing = thingList[i];
			if (thing is Plant)
			{
				if (thing.def.plant.dieFromToxicFallout && Rand.Value < 0.0065f)
				{
					if (thing is Plant { IsCrop: not false } plant && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfPoison-" + thing.def.defName, 240f))
					{
						Messages.Message("MessagePlantDiedOfPoison".Translate(plant.GetCustomLabelNoCount(includeHp: false)), new TargetInfo(plant.Position, map), MessageTypeDefOf.NegativeEvent);
					}
					thing.Kill();
				}
			}
			else if (thing.def.category == ThingCategory.Item)
			{
				CompRottable compRottable = thing.TryGetComp<CompRottable>();
				if (compRottable != null && (int)compRottable.Stage < 2)
				{
					compRottable.RotProgress += 3000f;
				}
			}
		}
	}

	public override void GameConditionDraw(Map map)
	{
		for (int i = 0; i < overlays.Count; i++)
		{
			overlays[i].DrawOverlay(map);
		}
	}

	public override float SkyTargetLerpFactor(Map map)
	{
		return GameConditionUtility.LerpInOutValue(this, TransitionTicks, 0.5f);
	}

	public override SkyTarget? SkyTarget(Map map)
	{
		return new SkyTarget(0.85f, ToxicFalloutColors, 1f, 1f);
	}

	public override float AnimalDensityFactor(Map map)
	{
		return 0f;
	}

	public override float PlantDensityFactor(Map map)
	{
		return 0f;
	}

	public override bool AllowEnjoyableOutsideNow(Map map)
	{
		return false;
	}

	public override List<SkyOverlay> SkyOverlays(Map map)
	{
		return overlays;
	}
}
