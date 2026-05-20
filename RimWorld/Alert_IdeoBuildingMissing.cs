using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_IdeoBuildingMissing : Alert_Precept
{
	public IdeoBuildingPresenceDemand demand;

	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	private List<string> targetNames = new List<string>();

	private List<GlobalTargetInfo> Targets
	{
		get
		{
			targets.Clear();
			targetNames.Clear();
			Map currentMap = Find.CurrentMap;
			if (currentMap == null)
			{
				return null;
			}
			foreach (Pawn item in currentMap.mapPawns.FreeColonistsSpawned)
			{
				if (!item.IsSlave && item.Ideo == demand.parent.ideo)
				{
					targets.Add(item);
					targetNames.Add(item.LabelShort.Colorize(ColoredText.NameColor));
				}
			}
			return targets;
		}
	}

	public Alert_IdeoBuildingMissing()
	{
	}

	public Alert_IdeoBuildingMissing(IdeoBuildingPresenceDemand demand)
	{
		this.demand = demand;
		label = "IdeoBuildingMissing".Translate(demand.parent.LabelCap);
		sourcePrecept = demand.parent;
	}

	public override AlertReport GetReport()
	{
		if (!ModsConfig.IdeologyActive)
		{
			return false;
		}
		Map currentMap = Find.CurrentMap;
		if (currentMap == null || demand.BuildingPresent(currentMap))
		{
			return false;
		}
		if (!demand.AppliesTo(currentMap))
		{
			return false;
		}
		if (!Faction.OfPlayer.ideos.Has(demand.parent.ideo))
		{
			return false;
		}
		if (!Faction.OfPlayer.ideos.IsPrimary(demand.parent.ideo) && demand.parent.ideo.ColonistBelieverCountCached < 3)
		{
			return false;
		}
		List<GlobalTargetInfo> list = Targets;
		return new AlertReport
		{
			active = list.Any(),
			culpritsTargets = list
		};
	}

	public override TaggedString GetExplanation()
	{
		string text = "";
		if (!demand.roomRequirements.NullOrEmpty())
		{
			text = "\n\n" + "IdeoBuildingRoomRequirementsDesc".Translate(demand.parent.Label.Colorize(ColoredText.NameColor)).Resolve() + ":\n" + demand.RoomRequirementsInfo.ToLineList(" -  ");
		}
		string arg = string.Empty;
		if (demand.parent.ThingDef.designationCategory != null)
		{
			arg = "IdeoBuildingArchitectTabDesc".Translate(demand.parent.ThingDef.designationCategory.label.Named("TABNAME"));
		}
		else
		{
			IEnumerable<string> enumerable = (from u in DefDatabase<RecipeDef>.AllDefsListForReading.Where((RecipeDef r) => r.recipeUsers != null && r.products.Count == 1 && r.products.Any((ThingDefCountClass p) => p.thingDef == demand.parent.ThingDef) && !r.IsSurgery).SelectMany((RecipeDef r) => r.recipeUsers)
				select u.label).Distinct();
			if (enumerable.Any())
			{
				arg = "IdeoBuildingCanBeCraftedAt".Translate(enumerable.ToLineList(" -  ", capitalizeItems: true));
			}
		}
		return "IdeoBuildingMissingDesc".Translate(demand.parent.Label.Colorize(ColoredText.NameColor), demand.parent.ideo.Named("IDEO"), ("IdeoBuildingExpectations".Translate() + " " + demand.minExpectation.label).Colorize(ColoredText.ExpectationsColor), targetNames.ToLineList(" -  "), text, 3.Named("MINBELIEVERS"), NamedArgumentUtility.Named(demand.parent.ThingDef, "BASEBUILDING"), arg.Named("BUILDINGINFO"));
	}
}
