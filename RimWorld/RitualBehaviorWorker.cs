using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class RitualBehaviorWorker : IExposable
{
	public RitualBehaviorDef def;

	public virtual string descriptionOverride => null;

	public virtual Sustainer SoundPlaying => null;

	public virtual bool ChecksReservations => true;

	public RitualBehaviorWorker()
	{
	}

	public RitualBehaviorWorker(RitualBehaviorDef def)
	{
		this.def = def;
	}

	public virtual string CanStartRitualNow(TargetInfo target, Precept_Ritual ritual, Pawn selectedPawn = null, Dictionary<string, Pawn> forcedForRole = null)
	{
		if (target.IsValid && target.Map.Tile.Valid)
		{
			PlanetLayerDef layerDef = target.Map.Tile.LayerDef;
			if ((!ritual.layerWhitelist.NullOrEmpty() && !ritual.layerWhitelist.Contains(layerDef)) || (!ritual.layerBlacklist.NullOrEmpty() && ritual.layerBlacklist.Contains(layerDef)))
			{
				return "CantStartRitualLayer".Translate(ritual.Label.Named("RITUAL"), layerDef.gerundLabel.Named("GERUND"), layerDef.LabelCap.Named("LAYER")).CapitalizeFirst();
			}
		}
		if (!ritual.allowOtherInstances)
		{
			foreach (LordJob_Ritual activeRitual in Find.IdeoManager.GetActiveRituals(target.Map))
			{
				if (activeRitual.Ritual == ritual)
				{
					return "CantStartRitualAlreadyInProgress".Translate(ritual.Label).CapitalizeFirst();
				}
			}
		}
		if (selectedPawn != null && ritual.behavior?.def.roles != null)
		{
			foreach (RitualRole role2 in ritual.behavior.def.roles)
			{
				if (role2.defaultForSelectedColonist && !role2.AppliesToPawn(selectedPawn, out var reason, target, null, null, ritual))
				{
					if (reason.NullOrEmpty())
					{
						return "CantStartRitualSelectedPawnCannotBeRole".Translate(selectedPawn.Named("PAWN"), role2.Label.Named("ROLE")).CapitalizeFirst();
					}
					return reason;
				}
			}
		}
		if (target.IsValid)
		{
			List<Pawn> list = target.Map.mapPawns.FreeColonistsAndPrisonersSpawned.ToList();
			list.AddRange(target.Map.mapPawns.SpawnedColonyAnimals);
			if (!ritual.behavior.def.roles.NullOrEmpty())
			{
				foreach (RitualRole role in ritual.behavior.def.roles)
				{
					if (!role.required || role.substitutable)
					{
						continue;
					}
					IEnumerable<RitualRole> source = ((role.mergeId == null) ? Gen.YieldSingle(role) : ritual.behavior.def.roles.Where((RitualRole r) => r.mergeId == role.mergeId));
					if (list.Count((Pawn p) => role.AppliesToPawn(p, out var _, target, null, null, null, skipReason: true)) < source.Count() && (forcedForRole == null || !forcedForRole.ContainsKey(role.id)))
					{
						Precept precept = ritual.ideo.PreceptsListForReading.FirstOrDefault((Precept p) => p.def == role.precept);
						if (precept != null)
						{
							return "MessageNeedAssignedRoleToBeginRitual".Translate(role.missingDesc ?? Find.ActiveLanguageWorker.WithIndefiniteArticle(precept.LabelCap), ritual.Label);
						}
						if (!role.noCandidatesGizmoDesc.NullOrEmpty())
						{
							return role.noCandidatesGizmoDesc;
						}
						if (source.Count() == 1)
						{
							return "MessageNoRequiredRolePawnToBeginRitual".Translate(role.missingDesc ?? Find.ActiveLanguageWorker.WithIndefiniteArticle(role.Label), ritual.Label);
						}
						return "MessageNoRequiredRolePawnToBeginRitual".Translate(source.Count() + " " + (role.missingDesc ?? Find.ActiveLanguageWorker.Pluralize(role.Label)), ritual.Label);
					}
				}
			}
		}
		return null;
	}

	public virtual string GetExplanation(Precept_Ritual ritual, RitualRoleAssignments assignments, float quality)
	{
		return null;
	}

	public virtual bool CanExecuteOn(TargetInfo target, RitualObligation obligation)
	{
		return obligation?.precept.obligationTargetFilter.CanUseTarget(target, obligation).canUse ?? true;
	}

	public virtual void TryExecuteOn(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments, bool playerForced = false)
	{
		if (CanStartRitualNow(target, ritual, null, assignments.ForcedRolesForReading) != null || !CanExecuteOn(target, obligation))
		{
			return;
		}
		if (playerForced)
		{
			foreach (Pawn participant in assignments.Participants)
			{
				RitualRole ritualRole = assignments.RoleForPawn(participant);
				if (ritualRole == null || !ritualRole.allowKeepLayingDown || participant.GetPosture() != PawnPosture.LayingInBed)
				{
					participant.jobs.EndCurrentJob(JobCondition.InterruptForced, startNewJob: false);
				}
			}
		}
		foreach (Pawn participant2 in assignments.Participants)
		{
			if (participant2.GetLord()?.LordJob is LordJob_VoluntarilyJoinable)
			{
				participant2.GetLord().Notify_PawnLost(participant2, PawnLostCondition.LeftVoluntarily);
			}
		}
		LordJob_Ritual lordJob = (LordJob_Ritual)CreateLordJob(target, organizer, ritual, obligation, assignments);
		LordMaker.MakeNewLord(Faction.OfPlayer, lordJob, target.Map, assignments.Participants.Where((Pawn p) => lordJob.RoleFor(p)?.addToLord ?? true));
		ritual.outcomeEffect?.ResetCompDatas();
		lordJob.PreparePawns();
		AbilityGroupDef useCooldownFromAbilityGroupDef = ritual.def.useCooldownFromAbilityGroupDef;
		if (useCooldownFromAbilityGroupDef != null && useCooldownFromAbilityGroupDef.cooldownTicks > 0 && !useCooldownFromAbilityGroupDef.ritualRoleIds.NullOrEmpty())
		{
			foreach (string ritualRoleId in useCooldownFromAbilityGroupDef.ritualRoleIds)
			{
				if (!assignments.AnyPawnAssigned(ritualRoleId))
				{
					continue;
				}
				foreach (Pawn item in assignments.AssignedPawns(ritualRoleId))
				{
					foreach (Ability item2 in item.abilities.AllAbilitiesForReading)
					{
						item2.Notify_GroupStartedCooldown(useCooldownFromAbilityGroupDef, useCooldownFromAbilityGroupDef.cooldownTicks);
					}
				}
			}
			ritual.Notify_CooldownFromAbilityStarted(useCooldownFromAbilityGroupDef.cooldownTicks);
		}
		PostExecute(target, organizer, ritual, obligation, assignments);
	}

	protected virtual void PostExecute(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
	{
		Messages.Message("RitualBegun".Translate(ritual.Label).CapitalizeFirst(), target, MessageTypeDefOf.NeutralEvent);
	}

	protected virtual LordJob CreateLordJob(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
	{
		return new LordJob_Ritual(target, ritual, obligation, def.stages, assignments);
	}

	public virtual void Tick(LordJob_Ritual ritual)
	{
	}

	public bool SpectatorsRequired()
	{
		if (def.stages.NullOrEmpty())
		{
			return true;
		}
		for (int i = 0; i < def.stages.Count; i++)
		{
			if (def.stages[i].spectatorsRequired)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyRole(Pawn p)
	{
		if (def.stages.NullOrEmpty())
		{
			return false;
		}
		for (int i = 0; i < def.stages.Count; i++)
		{
			if (def.stages[i].HasRole(p))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
	}

	public virtual void Cleanup(LordJob_Ritual ritual)
	{
	}

	public virtual void PostCleanup(LordJob_Ritual ritual)
	{
	}

	public virtual string ExpectedDuration(Precept_Ritual ritual, RitualRoleAssignments assignments, float quality)
	{
		return def.durationTicks.max.ToStringTicksToPeriod(allowSeconds: false);
	}

	public virtual bool ShouldInitAsSpectator(Pawn pawn, RitualRoleAssignments assignments)
	{
		return true;
	}

	public virtual void DrawPreviewOnTarget(TargetInfo targetInfo)
	{
	}

	public virtual bool TargetStillAllowed(TargetInfo selectedTarget, LordJob_Ritual ritual)
	{
		return true;
	}

	public virtual bool PawnCanFillRole(Pawn pawn, RitualRole role, out string s, TargetInfo ritualTarget)
	{
		s = null;
		return true;
	}
}
