using System;
using System.Collections.Generic;

namespace Verse;

public class RoomRoleDef : Def
{
	public Type workerClass;

	public bool avoidViewingArtIfUnowned;

	private List<RoomStatDef> relatedStats;

	private RoomRoleWorker workerInt;

	public RoomRoleWorker Worker => workerInt ?? (workerInt = (RoomRoleWorker)Activator.CreateInstance(workerClass));

	public string PostProcessedLabel(Room room)
	{
		return Worker.PostProcessedLabel(label, room);
	}

	public string PostProcessedLabelCap(Room room)
	{
		return PostProcessedLabel(room).CapitalizeFirst();
	}

	public bool IsStatRelated(RoomStatDef def)
	{
		if (relatedStats == null)
		{
			return false;
		}
		for (int i = 0; i < relatedStats.Count; i++)
		{
			if (relatedStats[i] == def)
			{
				return true;
			}
		}
		return false;
	}
}
