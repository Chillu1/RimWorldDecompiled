using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_ZoneAdd_Growing : Designator_ZoneAdd
{
	protected override string NewZoneLabel => "GrowingZone".Translate();

	protected virtual bool ShowRightClickHideOptions => true;

	public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
	{
		get
		{
			if (!ShowRightClickHideOptions)
			{
				yield break;
			}
			foreach (FloatMenuOption hideOption in Command_Hide_ZoneGrow.GetHideOptions())
			{
				yield return hideOption;
			}
		}
	}

	public Designator_ZoneAdd_Growing()
	{
		zoneTypeToPlace = typeof(Zone_Growing);
		defaultLabel = "GrowingZone".Translate();
		defaultDesc = "DesignatorGrowingZoneDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Growing");
		tutorTag = "ZoneAdd_Growing";
		hotKey = KeyBindingDefOf.Misc2;
		soundSucceeded = SoundDefOf.Designate_ZoneAdd_Growing;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!base.CanDesignateCell(c).Accepted)
		{
			return false;
		}
		float num = (ModsConfig.BiotechActive ? 0.5f : ThingDefOf.Plant_Potato.plant.fertilityMin);
		if (ModsConfig.IdeologyActive && BuildCopyCommandUtility.FindAllowedDesignator(TerrainDefOf.FungalGravel) != null)
		{
			num = Mathf.Min(num, ThingDefOf.Plant_Nutrifungus.plant.fertilityMin);
		}
		if (c.GetFertility(base.Map) < num)
		{
			return false;
		}
		return true;
	}

	protected override Zone MakeNewZone()
	{
		PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.GrowingFood, KnowledgeAmount.Total);
		return new Zone_Growing(Find.CurrentMap.zoneManager);
	}
}
