using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse.AI.Group;

public abstract class LordToil : IDisposable
{
	public Lord lord;

	public LordToilData data;

	private List<Func<bool>> failConditions = new List<Func<bool>>();

	public bool useAvoidGrid;

	public Map Map => lord.lordManager.map;

	public virtual IntVec3 FlagLoc => IntVec3.Invalid;

	public virtual bool AllowSatisfyLongNeeds => true;

	public virtual float? CustomWakeThreshold => null;

	public virtual bool AllowRestingInBed => true;

	public virtual bool AllowAggressiveTargetingOfRoamers => false;

	public virtual bool AllowSelfTend => true;

	public virtual bool ShouldFail
	{
		get
		{
			for (int i = 0; i < failConditions.Count; i++)
			{
				if (failConditions[i]())
				{
					return true;
				}
			}
			return false;
		}
	}

	public virtual bool ForceHighStoryDanger => false;

	public virtual bool AssignsDuties => true;

	public virtual void Init()
	{
	}

	public abstract void UpdateAllDuties();

	public virtual void LordToilTick()
	{
	}

	public virtual void Cleanup()
	{
	}

	public virtual ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
	{
		return ThinkTreeDutyHook.None;
	}

	public virtual void DrawPawnGUIOverlay(Pawn pawn)
	{
	}

	public virtual IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn target, Pawn forPawn)
	{
		return Enumerable.Empty<FloatMenuOption>();
	}

	public virtual IEnumerable<Gizmo> GetPawnGizmos(Pawn p)
	{
		return Enumerable.Empty<Gizmo>();
	}

	public virtual IEnumerable<Gizmo> GetBuildingGizmos(Building building)
	{
		return Enumerable.Empty<Gizmo>();
	}

	public virtual bool CanAddPawn(Pawn p)
	{
		return true;
	}

	public virtual void Notify_PawnLost(Pawn victim, PawnLostCondition cond)
	{
	}

	public virtual void Notify_PawnJobDone(Pawn p, JobCondition condition)
	{
	}

	public virtual void Notify_PawnAcquiredTarget(Pawn detector, Thing newTarg)
	{
	}

	public virtual void Notify_PawnDamaged(Pawn victim, DamageInfo dinfo)
	{
	}

	public virtual void Notify_BuildingLost(Building b)
	{
	}

	public virtual void Notify_CorpseLost(Corpse c)
	{
	}

	public virtual void Notify_ReachedDutyLocation(Pawn pawn)
	{
	}

	public virtual void Notify_ConstructionFailed(Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
	{
	}

	public virtual void Notify_ConstructionCompleted(Pawn pawn, Building building)
	{
	}

	public virtual void Notify_BuildingSpawnedOnMap(Building b)
	{
	}

	public virtual void Notify_BuildingDespawnedOnMap(Building b)
	{
	}

	public void AddFailCondition(Func<bool> failCondition)
	{
		failConditions.Add(failCondition);
	}

	public override string ToString()
	{
		string text = GetType().ToString();
		if (text.Contains('.'))
		{
			text = text.Substring(text.LastIndexOf('.') + 1);
		}
		if (text.Contains('_'))
		{
			text = text.Substring(text.LastIndexOf('_') + 1);
		}
		return text;
	}

	public void Dispose()
	{
		if (data is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}
}
