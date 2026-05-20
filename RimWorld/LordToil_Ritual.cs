using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class LordToil_Ritual : LordToil_Gathering
{
	public Pawn organizer;

	public LordJob_Ritual ritual;

	public RitualStage stage;

	public Action startAction;

	public Action tickAction;

	protected List<LocalTargetInfo> reservedThings = new List<LocalTargetInfo>();

	private readonly List<CachedPawnRitualDuty> cachedDuties = new List<CachedPawnRitualDuty>();

	private List<Pawn> tmpPawns = new List<Pawn>();

	public List<LocalTargetInfo> ReservedThings => reservedThings;

	public LordToil_Ritual(IntVec3 spot, LordJob_Ritual ritual, RitualStage stage, Pawn organizer)
		: base(spot, null)
	{
		this.stage = stage;
		this.organizer = organizer;
		this.ritual = ritual;
	}

	public override void Init()
	{
		base.Init();
		startAction?.Invoke();
	}

	public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
	{
		return ThinkTreeDutyHook.HighPriority;
	}

	public override void LordToilTick()
	{
		base.LordToilTick();
		tickAction?.Invoke();
	}

	public override void UpdateAllDuties()
	{
		reservedThings.Clear();
		cachedDuties.Clear();
		LocalTargetInfo localTargetInfo = (LocalTargetInfo)ritual.selectedTarget;
		IntVec3 intVec = IntVec3.Invalid;
		tmpPawns = lord.ownedPawns.ToList();
		for (int i = 0; i < tmpPawns.Count; i++)
		{
			Pawn pawn = tmpPawns[i];
			DutyDef dutyDef = stage.defaultDuty;
			IntVec3 cell = spot;
			LocalTargetInfo secondFocus = LocalTargetInfo.Invalid;
			Thing usedThing = null;
			Rot4 overrideFacing = Rot4.Invalid;
			if (ritual.assignments.PawnParticipating(pawn))
			{
				dutyDef = stage.GetDuty(pawn, null, ritual) ?? dutyDef;
				PawnStagePosition pawnStagePosition = ritual.PawnPositionForStage(pawn, stage);
				cell = pawnStagePosition.cell;
				usedThing = pawnStagePosition.thing;
				overrideFacing = pawnStagePosition.orientation;
				TargetInfo targetInfo = ritual.SecondFocusForStage(stage, pawn);
				secondFocus = ((targetInfo.Thing != null) ? new LocalTargetInfo(targetInfo.Thing) : ((LocalTargetInfo)targetInfo.Cell));
			}
			cachedDuties.Add(new CachedPawnRitualDuty
			{
				duty = dutyDef,
				spot = cell,
				usedThing = usedThing,
				overrideFacing = overrideFacing,
				secondFocus = secondFocus
			});
			if (dutyDef.ritualSpectateTarget)
			{
				intVec = cell;
			}
		}
		tmpPawns = lord.ownedPawns.ToList();
		SortByDuty();
		for (int j = 0; j < tmpPawns.Count; j++)
		{
			Pawn pawn2 = tmpPawns[j];
			CachedPawnRitualDuty cachedPawnRitualDuty = cachedDuties[j];
			IntVec3 intVec2 = cachedPawnRitualDuty.spot;
			LocalTargetInfo secondFocus2 = cachedPawnRitualDuty.secondFocus;
			Rot4 overrideFacing2 = cachedPawnRitualDuty.overrideFacing;
			Thing usedThing2 = cachedPawnRitualDuty.usedThing;
			PawnDuty pawnDuty = new PawnDuty(cachedPawnRitualDuty.duty, intVec2, secondFocus2, localTargetInfo);
			pawnDuty.spectateRect = CellRect.CenteredOn(spot, 0);
			Thing thing = localTargetInfo.Thing;
			RitualFocusProperties ritualFocusProperties = thing?.def.ritualFocus;
			if (ritualFocusProperties != null)
			{
				pawnDuty.spectateRectAllowedSides = ritualFocusProperties.allowedSpectateSides;
				pawnDuty.spectateDistance = ritualFocusProperties.spectateDistance;
			}
			else
			{
				pawnDuty.spectateDistance = new IntRange(2, 2);
			}
			if (stage.allowedSpectateSidesOverride != SpectateRectSide.None)
			{
				pawnDuty.spectateRectAllowedSides = stage.allowedSpectateSidesOverride;
			}
			if (stage.spectateDistanceOverride != IntRange.Zero)
			{
				pawnDuty.spectateDistance = stage.spectateDistanceOverride;
			}
			if (thing != null)
			{
				pawnDuty.spectateRectAllowedSides = pawnDuty.spectateRectAllowedSides.Rotated(thing.Rotation);
				pawnDuty.spectateRect = thing.OccupiedRect();
			}
			if (intVec.IsValid && intVec != intVec2)
			{
				pawnDuty.spectateRect = CellRect.CenteredOn(intVec, 0);
			}
			if (cachedPawnRitualDuty.overrideFacing.IsValid)
			{
				pawnDuty.spectateRect = CellRect.CenteredOn(intVec2 + overrideFacing2.FacingCell, 0);
				pawnDuty.overrideFacing = cachedPawnRitualDuty.overrideFacing;
			}
			if (pawnDuty.spectateRectAllowedSides == SpectateRectSide.Horizontal)
			{
				pawnDuty.spectateRectPreferredSide = ((intVec.x < intVec2.x) ? SpectateRectSide.Right : SpectateRectSide.Left);
			}
			else if (pawnDuty.spectateRectAllowedSides == SpectateRectSide.Vertical)
			{
				pawnDuty.spectateRectPreferredSide = ((intVec.y >= intVec2.y) ? SpectateRectSide.Up : SpectateRectSide.Down);
			}
			else if (pawnDuty.spectateRectAllowedSides != SpectateRectSide.All && pawnDuty.spectateRectAllowedSides != SpectateRectSide.None)
			{
				pawnDuty.spectateRectPreferredSide = pawnDuty.spectateRectAllowedSides;
			}
			else
			{
				pawnDuty.spectateRectPreferredSide = SpectateRectSide.Down;
			}
			if (intVec2.IsValid && !reservedThings.Contains(intVec2))
			{
				reservedThings.Add(intVec2);
			}
			if (secondFocus2.IsValid && !reservedThings.Contains(secondFocus2))
			{
				reservedThings.Add(secondFocus2);
			}
			if (localTargetInfo.IsValid && !reservedThings.Contains(localTargetInfo))
			{
				reservedThings.Add(localTargetInfo);
			}
			if (!reservedThings.Contains(pawn2))
			{
				reservedThings.Add(pawn2);
			}
			foreach (Pawn item in ritual.pawnsDeathIgnored)
			{
				if (!reservedThings.Contains(item.Corpse))
				{
					reservedThings.Add(item.Corpse);
				}
			}
			if (usedThing2 != null && !ritual.usedThings.Contains(usedThing2))
			{
				ritual.usedThings.Add(usedThing2);
			}
			pawn2.mindState.duty = pawnDuty;
			pawn2.mindState.priorityWork.ClearPrioritizedWorkAndJobQueue();
			pawn2.jobs?.CheckForJobOverride();
		}
	}

	private void SortByDuty()
	{
		HashSet<Pawn> hashSet = new HashSet<Pawn>(ritual.assignments.SpectatorsForReading);
		int num = 0;
		for (int i = 0; i < tmpPawns.Count; i++)
		{
			if (!hashSet.Contains(tmpPawns[i]))
			{
				if (i != num)
				{
					List<Pawn> list = tmpPawns;
					int index = i;
					List<Pawn> list2 = tmpPawns;
					int index2 = num;
					Pawn pawn = tmpPawns[num];
					Pawn pawn2 = tmpPawns[i];
					Pawn pawn3 = (list[index] = pawn);
					pawn3 = (list2[index2] = pawn2);
					List<CachedPawnRitualDuty> list3 = cachedDuties;
					index2 = i;
					List<CachedPawnRitualDuty> list4 = cachedDuties;
					index = num;
					CachedPawnRitualDuty cachedPawnRitualDuty = cachedDuties[num];
					CachedPawnRitualDuty cachedPawnRitualDuty2 = cachedDuties[i];
					CachedPawnRitualDuty cachedPawnRitualDuty3 = (list3[index2] = cachedPawnRitualDuty);
					cachedPawnRitualDuty3 = (list4[index] = cachedPawnRitualDuty2);
				}
				num++;
			}
		}
	}
}
