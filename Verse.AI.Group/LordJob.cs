using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Unity.Collections;

namespace Verse.AI.Group;

public abstract class LordJob : IExposable, IDisposable
{
	public Lord lord;

	public virtual bool LostImportantReferenceDuringLoading => false;

	public virtual bool AllowStartNewGatherings => true;

	public virtual bool AllowStartNewRituals => AllowStartNewGatherings;

	public virtual bool NeverInRestraints => false;

	public virtual bool GuiltyOnDowned => false;

	public virtual bool CanBlockHostileVisitors => true;

	public virtual bool AddFleeToil => true;

	public virtual bool OrganizerIsStartingPawn => false;

	public virtual bool KeepExistingWhileHasAnyBuilding => false;

	public virtual bool AlwaysShowWeapon => false;

	public virtual bool IsCaravanSendable => false;

	public virtual bool ManagesRopableAnimals => false;

	public virtual bool DontInterruptLayingPawnsOnCleanup => false;

	public virtual bool CanAutoAddPawns => true;

	public virtual bool ShouldExistWithoutPawns => false;

	public Map Map => lord.lordManager.map;

	public abstract StateGraph CreateGraph();

	public virtual void LordJobTick()
	{
	}

	public virtual void ExposeData()
	{
	}

	public virtual void Cleanup()
	{
	}

	public virtual void PostCleanup()
	{
	}

	public virtual void Notify_AddedToLord()
	{
	}

	public virtual void Notify_PawnAdded(Pawn p)
	{
	}

	public virtual void Notify_PawnLost(Pawn p, PawnLostCondition condition)
	{
	}

	public virtual void Notify_PawnJobDone(Pawn p, JobCondition condition)
	{
	}

	public virtual void Notify_InMentalState(Pawn pawn, MentalStateDef stateDef)
	{
	}

	public virtual void Notify_BuildingAdded(Building b)
	{
	}

	public virtual void Notify_CorpseAdded(Corpse c)
	{
	}

	public virtual void Notify_BuildingLost(Building b)
	{
	}

	public virtual void Notify_CorpseLost(Corpse c)
	{
	}

	public virtual void Notify_LordDestroyed()
	{
	}

	public virtual void Notify_MapRemoved()
	{
	}

	public virtual void Notify_PawnUndowned(Pawn p)
	{
		lord.CurLordToil?.UpdateAllDuties();
	}

	public virtual void Notify_PawnDowned(Pawn p)
	{
		lord.CurLordToil?.UpdateAllDuties();
	}

	public virtual ThinkResult Notify_DutyConstantResult(ThinkResult result, Pawn pawn, JobIssueParams issueParams)
	{
		return result;
	}

	public virtual ThinkResult Notify_DutyResult(ThinkResult result, Pawn pawn, JobIssueParams issueParams)
	{
		return result;
	}

	public virtual string GetJobReport(Pawn pawn)
	{
		return null;
	}

	public virtual string GetReport(Pawn pawn)
	{
		return null;
	}

	public virtual bool CanOpenAnyDoor(Pawn p)
	{
		return false;
	}

	public virtual bool ShouldRemovePawn(Pawn p, PawnLostCondition reason)
	{
		return true;
	}

	public virtual IEnumerable<Gizmo> GetPawnGizmos(Pawn p)
	{
		return Enumerable.Empty<Gizmo>();
	}

	public virtual bool EndPawnJobOnCleanup(Pawn p)
	{
		return true;
	}

	public virtual bool BlocksSocialInteraction(Pawn pawn)
	{
		return false;
	}

	public virtual bool DutyActiveWhenDown(Pawn pawn)
	{
		return false;
	}

	public virtual AcceptanceReport AllowsFloatMenu(Pawn pawn)
	{
		return true;
	}

	public virtual bool PrisonerSecure(Pawn pawn)
	{
		return false;
	}

	public virtual AcceptanceReport AbilityAllowed(Ability ability)
	{
		return true;
	}

	public virtual AcceptanceReport AllowsDrafting(Pawn pawn)
	{
		return true;
	}

	public virtual bool ValidateAttackTarget(Pawn searcher, Thing target)
	{
		return true;
	}

	public virtual NativeBitArray GetWalkGrid(Pawn pawn)
	{
		return default(NativeBitArray);
	}

	public virtual void Dispose()
	{
	}
}
