using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_RolesEmpty : Alert
{
	private List<Precept_Role> emptyRoles = new List<Precept_Role>();

	private List<string> targetLabels = new List<string>();

	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	public const int DayEnable = 10;

	public Alert_RolesEmpty()
	{
		requireIdeology = true;
	}

	private void GetTargets()
	{
		emptyRoles.Clear();
		targets.Clear();
		targetLabels.Clear();
		foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
		{
			foreach (Precept item in allIdeo.PreceptsListForReading)
			{
				if (!item.def.leaderRole && item.def.createsRoleEmptyThought && item is Precept_Role precept_Role && precept_Role.ChosenPawnSingle() == null && precept_Role.Active)
				{
					emptyRoles.Add(precept_Role);
				}
			}
		}
		if (!emptyRoles.Any())
		{
			return;
		}
		foreach (Pawn p in Find.CurrentMap.mapPawns.FreeColonistsSpawned)
		{
			if (!p.IsSlave && emptyRoles.Any((Precept_Role x) => x.ideo == p.Ideo))
			{
				targets.Add(p);
				targetLabels.Add(p.NameFullColored.Resolve());
			}
		}
	}

	public override string GetLabel()
	{
		if (emptyRoles.Count == 1)
		{
			return "IdeoRoleEmpty".Translate(emptyRoles[0].LabelCap);
		}
		return "IdeoRolesEmpty".Translate(emptyRoles.Count);
	}

	public override TaggedString GetExplanation()
	{
		return "IdeoRolesEmptyDesc".Translate(emptyRoles.Select((Precept_Role x) => x.LabelCap.ApplyTag(x.ideo).Resolve()).ToLineList("  - ")) + ":\n" + targetLabels.ToLineList("  - ");
	}

	public override AlertReport GetReport()
	{
		if (GenDate.DaysPassed < 10)
		{
			return false;
		}
		if (Find.CurrentMap == null)
		{
			return false;
		}
		GetTargets();
		return AlertReport.CulpritsAre(targets);
	}
}
