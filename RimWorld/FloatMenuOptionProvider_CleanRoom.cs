using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_CleanRoom : FloatMenuOptionProvider
{
	private static List<Filth> tmpFilth = new List<Filth>();

	protected override bool Drafted => false;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		if (context.ClickedRoom == null || context.ClickedRoom.PsychologicallyOutdoors || !context.ClickedRoom.ProperRoom || context.ClickedRoom.TouchesMapEdge || context.ClickedRoom.IsHuge)
		{
			return false;
		}
		if (context.map.IsPocketMap && !context.map.generatorDef.pocketMapProperties.canBeCleaned)
		{
			return false;
		}
		if (context.FirstSelectedPawn.WorkTypeIsDisabled(WorkTypeDefOf.Cleaning))
		{
			return false;
		}
		return true;
	}

	protected override FloatMenuOption GetSingleOption(FloatMenuContext context)
	{
		List<Filth> filth = GetRoomFilthCleanableByPawn(context.ClickedRoom, context.FirstSelectedPawn);
		if (filth.Count == 0)
		{
			return null;
		}
		string roomRoleLabel = context.ClickedRoom.GetRoomRoleLabel();
		return new FloatMenuOption("CleanRoom".Translate(roomRoleLabel), delegate
		{
			Job job = JobMaker.MakeJob(JobDefOf.Clean);
			foreach (Filth item in filth)
			{
				job.AddQueuedTarget(TargetIndex.A, item);
			}
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}, MenuOptionPriority.VeryLow);
	}

	private static List<Filth> GetRoomFilthCleanableByPawn(Room room, Pawn pawn)
	{
		if (pawn.Map.IsPocketMap && !pawn.Map.generatorDef.pocketMapProperties.canBeCleaned)
		{
			return null;
		}
		if (room == null || room.Dereferenced || room.Fogged)
		{
			return null;
		}
		if (room.IsHuge || room.TouchesMapEdge)
		{
			return null;
		}
		tmpFilth.Clear();
		foreach (Filth item in room.ContainedAndAdjacentThings.OfType<Filth>())
		{
			if (!item.IsForbidden(pawn) && item.Map.areaManager.Home[item.Position] && pawn.CanReach(item, PathEndMode.Touch, pawn.NormalMaxDanger()) && pawn.CanReserve(item))
			{
				tmpFilth.Add(item);
			}
		}
		return tmpFilth;
	}
}
