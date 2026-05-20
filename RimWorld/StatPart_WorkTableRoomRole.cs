using UnityEngine;
using Verse;

namespace RimWorld;

public class StatPart_WorkTableRoomRole : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.HasThing && req.Thing.def.building != null && Applies(req.Thing))
		{
			val *= req.Thing.def.building.workTableNotInRoomRoleFactor;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Thing.def.building != null && Applies(req.Thing))
		{
			return "NotInRoomRole".Translate(req.Thing.def.building.workTableRoomRole.label).CapitalizeFirst() + ": x" + req.Thing.def.building.workTableNotInRoomRoleFactor.ToStringPercent();
		}
		return null;
	}

	public static bool Applies(Thing parent)
	{
		if (parent.def?.building?.workTableRoomRole == null)
		{
			return false;
		}
		Room room = parent.GetRoom();
		if (room != null && !room.PsychologicallyOutdoors)
		{
			return room.Role != parent.def.building.workTableRoomRole;
		}
		return false;
	}

	public static bool WouldApplyToBuildingIfPlaced(ThingDef def, Map map, IntVec3 cell)
	{
		if (def?.building?.workTableRoomRole == null || Mathf.Approximately(def.building.workTableNotInRoomRoleFactor, 1f))
		{
			return false;
		}
		Room room = cell.GetRoom(map);
		if (room == null || room.PsychologicallyOutdoors)
		{
			return false;
		}
		if (room.Role == def.building.workTableRoomRole)
		{
			return false;
		}
		return room.GetRoomRoleIfBuildingPlaced(def) != def.building.workTableRoomRole;
	}
}
