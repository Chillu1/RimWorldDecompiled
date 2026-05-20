using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class Building_Throne : Building
{
	public static IEnumerable<RoyalTitleDef> AllTitlesForThroneStature => from title in DefDatabase<RoyalTitleDef>.AllDefsListForReading
		where !title.throneRoomRequirements.NullOrEmpty()
		orderby title.seniority
		select title;

	public Pawn AssignedPawn
	{
		get
		{
			if (!ModLister.CheckRoyalty("Throne"))
			{
				return null;
			}
			if (CompAssignableToPawn == null || !CompAssignableToPawn.AssignedPawnsForReading.Any())
			{
				return null;
			}
			return CompAssignableToPawn.AssignedPawnsForReading[0];
		}
	}

	public CompAssignableToPawn_Throne CompAssignableToPawn => GetComp<CompAssignableToPawn_Throne>();

	public RoyalTitleDef TitleStature
	{
		get
		{
			Room room = this.GetRoom();
			if (room == null || room.OutdoorsForWork)
			{
				return null;
			}
			RoyalTitleDef result = null;
			foreach (RoyalTitleDef item in AllTitlesForThroneStature)
			{
				bool flag = true;
				for (int i = 0; i < item.throneRoomRequirements.Count; i++)
				{
					if (!(item.throneRoomRequirements[i] is RoomRequirement_HasAssignedThroneAnyOf) && !item.throneRoomRequirements[i].MetOrDisabled(room))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					result = item;
					continue;
				}
				break;
			}
			return result;
		}
	}

	public override string GetInspectString()
	{
		string inspectString = base.GetInspectString();
		Room room = this.GetRoom();
		Pawn p = ((CompAssignableToPawn.AssignedPawnsForReading.Count == 1) ? CompAssignableToPawn.AssignedPawnsForReading[0] : null);
		RoyalTitleDef titleStature = TitleStature;
		inspectString += "\n" + "ThroneMaxSatisfiedTitle".Translate() + ": " + ((titleStature == null) ? "None".Translate() : ((TaggedString)titleStature.GetLabelCapFor(p)));
		string text = RoomRoleWorker_ThroneRoom.Validate(room);
		if (text != null)
		{
			return inspectString + "\n" + text;
		}
		return inspectString;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		if (!ModLister.CheckRoyalty("Throne"))
		{
			yield break;
		}
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
	}
}
