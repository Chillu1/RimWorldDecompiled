using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_UndignifiedBedroom : Alert
{
	private List<Pawn> targetsResult = new List<Pawn>();

	private StringBuilder sb = new StringBuilder();

	public List<Pawn> Targets
	{
		get
		{
			targetsResult.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				foreach (Pawn item in maps[i].mapPawns.FreeColonistsSpawned)
				{
					if (item.royalty != null && !item.Suspended && item.royalty.AnyUnmetBedroomRequirements())
					{
						targetsResult.Add(item);
					}
				}
			}
			return targetsResult;
		}
	}

	public Alert_UndignifiedBedroom()
	{
		defaultLabel = "UndignifiedBedroom".Translate();
		defaultExplanation = "UndignifiedBedroomDesc".Translate();
		requireRoyalty = true;
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(Targets);
	}

	public override TaggedString GetExplanation()
	{
		return defaultExplanation + "\n" + targetsResult.Select(delegate(Pawn t)
		{
			RoyalTitle royalTitle = t.royalty.HighestTitleWithBedroomRequirements();
			RoyalTitleDef royalTitleDef = (royalTitle.RoomRequirementGracePeriodActive(t) ? royalTitle.def.GetPreviousTitle(royalTitle.faction) : royalTitle.def);
			string[] array = t.royalty.GetUnmetBedroomRequirements(includeOnGracePeriod: false).ToArray();
			string[] array2 = t.royalty.GetUnmetBedroomRequirements(includeOnGracePeriod: true, onlyOnGracePeriod: true).ToArray();
			bool flag = royalTitleDef != null && array.Length != 0;
			sb.Length = 0;
			if (flag)
			{
				sb.Append(t.LabelShort + " (" + royalTitleDef.GetLabelFor(t.gender) + "):\n" + array.ToLineList("  - "));
			}
			if (array2.Length != 0)
			{
				if (flag)
				{
					sb.Append("\n\n");
				}
				sb.Append(t.LabelShort + " (" + royalTitle.def.GetLabelFor(t.gender) + ", " + "RoomRequirementGracePeriodDesc".Translate(royalTitle.RoomRequirementGracePeriodDaysLeft.ToString("0.0")) + ")" + ":\n" + array2.ToLineList("  - "));
			}
			return sb.ToString();
		}).ToLineList("\n");
	}
}
