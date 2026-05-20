using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Precept_Building : Precept_ThingStyle
{
	public IdeoBuildingPresenceDemand presenceDemand;

	public override bool CanRegenerate => true;

	protected override string NameRootSymbol => "r_ideoBuildingName";

	public override void Init(Ideo ideo, FactionDef generatingFor = null)
	{
		base.Init(ideo, (FactionDef)null);
		if (ModLister.CheckIdeology("Ideology building"))
		{
			GeneratePresenceDemand();
		}
	}

	public override void Regenerate(Ideo ideo, FactionDef generatingFor = null)
	{
		GeneratePresenceDemand();
		if (UsesGeneratedName)
		{
			RegenerateName();
		}
		ClearTipCache();
		Notify_ThingDefSet();
	}

	protected void GeneratePresenceDemand()
	{
		presenceDemand = new IdeoBuildingPresenceDemand(this);
		presenceDemand.minExpectation = def.buildingMinExpectations.RandomElement();
		int num = Mathf.CeilToInt(def.roomRequirementCountCurve.Evaluate(Rand.Value));
		if ((base.ThingDef.ritualFocus == null || !base.ThingDef.ritualFocus.consumable) && num > 0)
		{
			presenceDemand.roomRequirements = new List<RoomRequirement>();
			List<RoomRequirement> list = def.buildingRoomRequirements.ToList();
			list.Shuffle();
			for (int i = 0; i < num; i++)
			{
				presenceDemand.roomRequirements.Add(list[i]);
			}
			presenceDemand.roomRequirements.AddRange(def.buildingRoomRequirementsFixed);
		}
	}

	public override string TransformThingLabel(string label)
	{
		return label + " (" + base.LabelCap + ")";
	}

	private string CostListString()
	{
		List<string> list = new List<string>();
		if (!base.ThingDef.CostList.NullOrEmpty())
		{
			for (int i = 0; i < base.ThingDef.CostList.Count; i++)
			{
				list.Add(string.Concat(base.ThingDef.CostList[i].thingDef.LabelCap + " x", base.ThingDef.CostList[i].count.ToString()));
			}
		}
		if (!base.ThingDef.stuffCategories.NullOrEmpty())
		{
			string text = "";
			for (int j = 0; j < base.ThingDef.stuffCategories.Count; j++)
			{
				if (j > 0)
				{
					text += "/";
				}
				text += base.ThingDef.stuffCategories[j].label;
			}
			list.Add(text.CapitalizeFirst() + " x" + base.ThingDef.CostStuffCount);
		}
		return list.ToCommaList().CapitalizeFirst();
	}

	public override string GetTip()
	{
		if (tipCached.NullOrEmpty())
		{
			Precept.tmpCompsDesc.Clear();
			if (base.ThingDef?.description != null)
			{
				Precept.tmpCompsDesc.Append(base.ThingDef.description);
			}
			else if (!base.Description.NullOrEmpty())
			{
				Precept.tmpCompsDesc.Append(base.Description);
			}
			Precept.tmpCompsDesc.AppendLine();
			Precept.tmpCompsDesc.AppendInNewLine(ColorizeDescTitle("IdeoBuildingVariationOf".Translate() + ": "));
			Precept.tmpCompsDesc.AppendInNewLine(base.ThingDef.LabelCap.Resolve().Indented());
			Precept.tmpCompsDesc.AppendInNewLine(ColorizeDescTitle("Size".Translate().CapitalizeFirst() + ": ").Indented());
			Precept.tmpCompsDesc.Append(base.ThingDef.size.x + "x" + base.ThingDef.size.z);
			Precept.tmpCompsDesc.AppendInNewLine(ColorizeDescTitle("Cost".Translate().CapitalizeFirst() + ": ").Indented());
			Precept.tmpCompsDesc.Append(CostListString());
			if (base.ThingDef.ritualFocus == null || !base.ThingDef.ritualFocus.consumable)
			{
				Precept.tmpCompsDesc.AppendLine();
				Precept.tmpCompsDesc.AppendInNewLine(ColorizeDescTitle("IdeoBuildingMinimumExpectations".Translate() + ": "));
				Precept.tmpCompsDesc.Append(presenceDemand.minExpectation.LabelCap);
				if (!presenceDemand.roomRequirements.NullOrEmpty())
				{
					Precept.tmpCompsDesc.AppendLine();
					Precept.tmpCompsDesc.AppendInNewLine(ColorizeDescTitle("RoomRequirements".Translate() + ":"));
					Precept.tmpCompsDesc.AppendInNewLine(presenceDemand.RoomRequirementsInfo.ToLineList(" -  ", capitalizeItems: true));
				}
			}
			tipCached = Precept.tmpCompsDesc.ToString();
		}
		return tipCached;
	}

	public override List<Thought_Situational> SituationThoughtsToAdd(Pawn pawn, List<Thought_Situational> activeThoughts)
	{
		tmpSituationalThoughts.Clear();
		if (!pawn.IsFreeColonist)
		{
			return tmpSituationalThoughts;
		}
		Map mapHeld = pawn.MapHeld;
		if (mapHeld != null)
		{
			if (!presenceDemand.AppliesTo(mapHeld))
			{
				return tmpSituationalThoughts;
			}
			if (!presenceDemand.BuildingPresent(mapHeld))
			{
				if (!activeThoughts.Any((Thought_Situational t) => t is Thought_IdeoMissingBuilding thought_IdeoMissingBuilding2 && thought_IdeoMissingBuilding2.demand == presenceDemand))
				{
					Thought_IdeoMissingBuilding thought_IdeoMissingBuilding = (Thought_IdeoMissingBuilding)ThoughtMaker.MakeThought(ThoughtDefOf.IdeoBuildingMissing);
					if (thought_IdeoMissingBuilding != null)
					{
						thought_IdeoMissingBuilding.pawn = pawn;
						thought_IdeoMissingBuilding.demand = presenceDemand;
						thought_IdeoMissingBuilding.sourcePrecept = this;
						tmpSituationalThoughts.Add(thought_IdeoMissingBuilding);
					}
				}
			}
			else if (!presenceDemand.RequirementsSatisfied(mapHeld) && !activeThoughts.Any((Thought_Situational t) => t is Thought_IdeoDisrespectedBuilding thought_IdeoDisrespectedBuilding2 && thought_IdeoDisrespectedBuilding2.demand == presenceDemand))
			{
				Thought_IdeoDisrespectedBuilding thought_IdeoDisrespectedBuilding = (Thought_IdeoDisrespectedBuilding)ThoughtMaker.MakeThought(ThoughtDefOf.IdeoBuildingDisrespected);
				if (thought_IdeoDisrespectedBuilding != null)
				{
					thought_IdeoDisrespectedBuilding.pawn = pawn;
					thought_IdeoDisrespectedBuilding.demand = presenceDemand;
					thought_IdeoDisrespectedBuilding.sourcePrecept = this;
					tmpSituationalThoughts.Add(thought_IdeoDisrespectedBuilding);
				}
			}
		}
		return tmpSituationalThoughts;
	}

	public override IEnumerable<FloatMenuOption> EditFloatMenuOptions()
	{
		yield return EditFloatMenuOption();
	}

	public override IEnumerable<Alert> GetAlerts()
	{
		Map currentMap = Find.CurrentMap;
		if (currentMap != null && presenceDemand.AppliesTo(currentMap))
		{
			if (!presenceDemand.BuildingPresent(currentMap))
			{
				yield return presenceDemand.AlertCachedMissingMissing;
			}
			else if (!presenceDemand.roomRequirements.NullOrEmpty())
			{
				yield return presenceDemand.AlertCachedMissingDisrespected;
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref presenceDemand, "presenceDemand");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			presenceDemand.parent = this;
		}
	}

	public override void CopyTo(Precept precept)
	{
		base.CopyTo(precept);
		Precept_Building precept_Building = (Precept_Building)precept;
		precept_Building.presenceDemand = presenceDemand.Copy();
		precept_Building.presenceDemand.parent = precept_Building;
	}

	public override string InspectStringExtra(Thing thing)
	{
		return ("Stat_Thing_RelatedToIdeos_Name".Translate() + ": " + ideo.name.ApplyTag(ideo)).Resolve();
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(Thing thing)
	{
		yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Stat_Thing_RelatedToIdeos_Name".Translate(), ideo.name.ApplyTag(ideo).Resolve(), "Stat_Thing_RelatedToIdeos_Desc".Translate(), 1110, null, new Dialog_InfoCard.Hyperlink[1]
		{
			new Dialog_InfoCard.Hyperlink(ideo)
		});
	}
}
