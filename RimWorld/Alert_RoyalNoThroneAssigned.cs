using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_RoyalNoThroneAssigned : Alert
{
	private List<Pawn> targetsResult = new List<Pawn>();

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
					if (item.royalty == null || item.Suspended || !item.royalty.CanRequireThroneroom())
					{
						continue;
					}
					bool flag = false;
					List<RoyalTitle> allTitlesForReading = item.royalty.AllTitlesForReading;
					for (int j = 0; j < allTitlesForReading.Count; j++)
					{
						if (!allTitlesForReading[j].def.throneRoomRequirements.NullOrEmpty())
						{
							flag = true;
							break;
						}
					}
					if (flag && item.ownership.AssignedThrone == null)
					{
						targetsResult.Add(item);
					}
				}
			}
			return targetsResult;
		}
	}

	public Alert_RoyalNoThroneAssigned()
	{
		defaultLabel = "NeedThroneAssigned".Translate();
		defaultExplanation = "NeedThroneAssignedDesc".Translate();
		requireRoyalty = true;
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(Targets);
	}

	public override TaggedString GetExplanation()
	{
		string text = defaultExplanation;
		if (MoveColonyUtility.TitleAndRoleRequirementsGracePeriodActive)
		{
			text += "\n\n" + "RoomRequirementGracePeriodDesc".Translate(MoveColonyUtility.TitleAndRoleRequirementGracePeriodTicksLeft.TicksToDays().ToString("0.0"));
		}
		return text;
	}
}
