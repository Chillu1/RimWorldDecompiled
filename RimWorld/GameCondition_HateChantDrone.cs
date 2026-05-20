using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GameCondition_HateChantDrone : GameCondition
{
	public HateChantDroneLevel level = HateChantDroneLevel.VeryLow;

	private List<SkyOverlay> overlays = new List<SkyOverlay>();

	public override string Label => def.label + ": " + level.GetLabel().CapitalizeFirst();

	public override string LetterText => def.letterText.Formatted(level.GetLabel());

	public override int TransitionTicks => 180;

	public override void PostMake()
	{
		base.PostMake();
		base.Permanent = true;
	}

	public override void Init()
	{
		if (!ModLister.CheckAnomaly("Hate chant drone"))
		{
			End();
		}
		else
		{
			base.Init();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref level, "level", HateChantDroneLevel.None);
	}

	public override float SkyTargetLerpFactor(Map map)
	{
		return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
	}

	public override void GameConditionTick()
	{
		base.GameConditionTick();
		List<Map> affectedMaps = base.AffectedMaps;
		for (int i = 0; i < overlays.Count; i++)
		{
			for (int j = 0; j < affectedMaps.Count; j++)
			{
				overlays[i].TickOverlay(affectedMaps[j], 1f);
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

	public override List<SkyOverlay> SkyOverlays(Map map)
	{
		return overlays;
	}
}
