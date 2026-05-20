using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_GuardianShipDelay : QuestPartActivable
{
	public Pawn pawn;

	public int delayTicks;

	public int ticksPassed;

	private Gizmo gizmo;

	public int TicksLeft
	{
		get
		{
			if (base.State != QuestPartState.Enabled)
			{
				return 0;
			}
			return delayTicks - ticksPassed;
		}
	}

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (pawn != null)
			{
				yield return pawn;
			}
		}
	}

	protected override void Enable(SignalArgs receivedArgs)
	{
		base.Enable(receivedArgs);
		ticksPassed = 0;
	}

	public override void QuestPartTick()
	{
		base.QuestPartTick();
		if (!pawn.DestroyedOrNull() && !pawn.Suspended)
		{
			ticksPassed++;
			if (ticksPassed >= delayTicks)
			{
				Complete();
			}
		}
	}

	public override IEnumerable<Gizmo> ExtraGizmos(ISelectable target)
	{
		if (target == pawn)
		{
			if (gizmo == null)
			{
				gizmo = new GuardianShipGizmo(this);
			}
			yield return gizmo;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref pawn, "pawn");
		Scribe_Values.Look(ref delayTicks, "delayTicks", 0);
		Scribe_Values.Look(ref ticksPassed, "ticksPassed", 0);
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		pawn = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.RandomElement();
		delayTicks = Rand.RangeInclusive(833, 2500);
	}
}
