using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public class DebugArena : WorldObjectComp
{
	public List<Pawn> lhs;

	public List<Pawn> rhs;

	public Action<ArenaUtility.ArenaResult> callback;

	private int tickCreated;

	private int tickFightStarted;

	public DebugArena()
	{
		tickCreated = Find.TickManager.TicksGame;
	}

	public override void CompTickInterval(int delta)
	{
		if (lhs == null || rhs == null)
		{
			Log.ErrorOnce("DebugArena improperly set up", 73785616);
			return;
		}
		if ((tickFightStarted == 0 && Find.TickManager.TicksGame - tickCreated > 10000) || (tickFightStarted != 0 && Find.TickManager.TicksGame - tickFightStarted > 60000))
		{
			Log.Message("Fight timed out");
			ArenaUtility.ArenaResult obj = new ArenaUtility.ArenaResult
			{
				tickDuration = Find.TickManager.TicksGame - tickCreated,
				winner = ArenaUtility.ArenaResult.Winner.Other
			};
			callback(obj);
			parent.Destroy();
			return;
		}
		if (tickFightStarted == 0)
		{
			foreach (Pawn item in lhs.Concat(rhs))
			{
				if (item.records.GetValue(RecordDefOf.ShotsFired) > 0f || (item.CurJob != null && item.CurJob.def == JobDefOf.AttackMelee && item.Position.DistanceTo(item.CurJob.targetA.Thing.Position) <= 2f))
				{
					tickFightStarted = Find.TickManager.TicksGame;
					break;
				}
			}
		}
		if (tickFightStarted == 0)
		{
			return;
		}
		bool flag = !lhs.Any((Pawn pawn) => !pawn.Dead && !pawn.Downed && pawn.Spawned);
		bool flag2 = !rhs.Any((Pawn pawn) => !pawn.Dead && !pawn.Downed && pawn.Spawned);
		if (!(flag || flag2))
		{
			return;
		}
		ArenaUtility.ArenaResult obj2 = new ArenaUtility.ArenaResult
		{
			tickDuration = Find.TickManager.TicksGame - tickFightStarted
		};
		if (flag && !flag2)
		{
			obj2.winner = ArenaUtility.ArenaResult.Winner.Rhs;
		}
		else if (!flag && flag2)
		{
			obj2.winner = ArenaUtility.ArenaResult.Winner.Lhs;
		}
		else
		{
			obj2.winner = ArenaUtility.ArenaResult.Winner.Other;
		}
		callback(obj2);
		foreach (Pawn item2 in lhs.Concat(rhs))
		{
			if (!item2.Destroyed)
			{
				item2.Destroy();
			}
		}
		parent.Destroy();
	}
}
