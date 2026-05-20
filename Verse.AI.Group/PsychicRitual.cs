using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI.Group
{
	public class PsychicRitual : IExposable, ILoadReferenceable
	{
		public enum LeftOrCanceled
		{
			Invalid,
			Left,
			Canceled
		}

		public Lord lord;

		public PsychicRitualDef def;

		public PsychicRitualRoleAssignments assignments;

		public bool canceled;

		public bool succeeded;

		private int loadID = -1;

		public float power;

		public float? maxPower;

		public Map Map => lord.Map;

		public float PowerPercent
		{
			get
			{
				if (!maxPower.HasValue)
				{
					return Mathf.Clamp01(power);
				}
				return Mathf.Clamp01(power / maxPower.Value);
			}
		}

		public PsychicRitual()
		{
			loadID = Find.UniqueIDsManager.GetNextPsychicRitualID();
		}

		public virtual void Start()
		{
			power = 0f;
			foreach (var (psychicRitualRoleDef2, list2) in assignments.RoleAssignments)
			{
				foreach (Pawn item in list2)
				{
					if (!item.Awake() && !psychicRitualRoleDef2.ConditionAllowed(PsychicRitualRoleDef.Condition.Sleeping))
					{
						if (!item.health.capacities.CanBeAwake)
						{
							Log.Error($"Attempting to wake {item.ToStringSafe()} for the {def.LabelCap} psychic ritual, but they cannot be awake.");
						}
						RestUtility.WakeUp(item, startNewJob: false);
					}
					item?.jobs?.EndCurrentJob(JobCondition.InterruptForced, startNewJob: false);
					item?.jobs?.ClearQueuedJobs();
					if (item.Drafted && !psychicRitualRoleDef2.ConditionAllowed(PsychicRitualRoleDef.Condition.Drafted))
					{
						item.drafter.Drafted = false;
					}
				}
			}
		}

		public virtual LeftOrCanceled LeaveOrCancelPsychicRitual(PsychicRitualRoleDef role, Pawn pawn, TaggedString reason)
		{
			if (def.nonRequiredPawnsMayLeave && assignments.RoleAssignedCount(role) > role.MinCount)
			{
				LeavePsychicRitual(pawn, reason);
				return LeftOrCanceled.Left;
			}
			CancelPsychicRitual(reason);
			return LeftOrCanceled.Canceled;
		}

		public virtual void CancelPsychicRitual(TaggedString reason)
		{
			if (lord?.faction != null && lord.faction == Faction.OfPlayer)
			{
				Find.PsychicRitualManager.ClearCooldown(def);
				Messages.Message(def.PsychicRitualCanceledMessage(reason).CapitalizeFirst().EndWithPeriod(), assignments.Target, MessageTypeDefOf.NeutralEvent);
			}
			if (!(lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual) || lordToil_PsychicRitual.RitualData.removeLordOnCancel)
			{
				lord.lordManager.RemoveLord(lord);
			}
			canceled = true;
		}

		public virtual void LeavePsychicRitual(Pawn pawn, TaggedString reason)
		{
			TryMessagePawnLost(pawn, reason);
			if (lord.ownedPawns.Contains(pawn))
			{
				lord.Notify_PawnLost(pawn, PawnLostCondition.LordRejected);
			}
		}

		public virtual void ReleaseAllPawnsAndBuildings()
		{
			lord.RemoveAllPawns();
			lord.RemoveAllBuildings();
		}

		public void TryMessagePawnLost(Pawn pawn, TaggedString reason)
		{
			if (lord?.faction == Faction.OfPlayer && PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				Messages.Message(def.LeftPsychicRitualMessage(pawn, reason).CapitalizeFirst().EndWithPeriod(), pawn, MessageTypeDefOf.NeutralEvent, historical: false);
			}
		}

		public virtual void Notify_PawnLost(Pawn pawn, PawnLostCondition cond)
		{
			if (pawn.jobs?.curJob?.lord == lord)
			{
				pawn.jobs?.EndCurrentJob(JobCondition.InterruptForced);
			}
			if (cond != PawnLostCondition.LordRejected && !succeeded)
			{
				TaggedString reason = PsychicRitualRoleDef.PawnLostConditionToPsychicRitualReason(this, pawn, cond);
				PsychicRitualRoleDef psychicRitualRoleDef = assignments.RoleForPawn(pawn);
				if (psychicRitualRoleDef != null && assignments.RoleAssignedCount(psychicRitualRoleDef) <= psychicRitualRoleDef.MinCount)
				{
					CancelPsychicRitual(reason);
				}
				else if (!def.nonRequiredPawnsMayLeave)
				{
					CancelPsychicRitual(reason);
				}
				else if (lord.ownedPawns.Contains(pawn))
				{
					TryMessagePawnLost(pawn, reason);
				}
			}
		}

		public virtual void ExposeData()
		{
			Scribe_References.Look(ref lord, "lord");
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref power, "power", 0f);
			Scribe_Values.Look(ref maxPower, "maxPower");
			Scribe_Values.Look(ref canceled, "canceled", defaultValue: false);
			Scribe_Values.Look(ref succeeded, "succeeded", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				if (lord.LordJob is LordJob_PsychicRitual lordJob_PsychicRitual)
				{
					assignments = lordJob_PsychicRitual.assignments;
				}
				else
				{
					Log.Error("Error during loading psychic ritual: LordJob is not a LordJob_PsychicRitual.");
				}
			}
		}

		public string GetUniqueLoadID()
		{
			return "PsychicRitual_" + loadID;
		}
	}
}
