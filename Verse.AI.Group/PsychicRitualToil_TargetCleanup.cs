using RimWorld;

namespace Verse.AI.Group;

public class PsychicRitualToil_TargetCleanup : PsychicRitualToil
{
	private const int TargetFactionGoodwillChange = -25;

	private PsychicRitualRoleDef targetRole;

	private PsychicRitualRoleDef invokerRole;

	private Pawn invoker;

	private Pawn target;

	protected PsychicRitualToil_TargetCleanup()
	{
	}

	public PsychicRitualToil_TargetCleanup(PsychicRitualRoleDef invokerRole, PsychicRitualRoleDef targetRole)
	{
		this.invokerRole = invokerRole;
		this.targetRole = targetRole;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		base.Start(psychicRitual, parent);
		invoker = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
		target = psychicRitual.assignments.FirstAssignedPawn(targetRole);
		invoker?.jobs?.EndCurrentJob(JobCondition.InterruptForced);
		if (target != null)
		{
			if (!target.Dead && target.IsPrisonerOfColony)
			{
				WorkGiver_Warden_TakeToBed.TryTakePrisonerToBed(target, invoker);
				PawnUtility.ForceWait(target, 2500, null, maintainPosture: true);
			}
		}
		else
		{
			Log.Warning("Tried to cleanup prisoner target for ritual " + psychicRitual.def.defName + ", but no target pawn was found.");
		}
		psychicRitual.ReleaseAllPawnsAndBuildings();
		if (invoker.IsPlayerControlled && target.Faction != null && target.HomeFaction != null && target.HomeFaction != Faction.OfPlayer && target.HomeFaction.def.humanlikeFaction && !target.Faction.def.PermanentlyHostileTo(FactionDefOf.PlayerColony))
		{
			target.HomeFaction.TryAffectGoodwillWith(Faction.OfPlayer, -25, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.WasPsychicRitualTarget);
		}
		QuestUtility.SendQuestTargetSignals(target.questTags, "PsychicRitualTarget", target.Named("SUBJECT"));
	}

	public override bool Tick(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		if (target == null || target.Dead || !target.IsPrisonerOfColony)
		{
			psychicRitual.ReleaseAllPawnsAndBuildings();
			return true;
		}
		if (invoker.IsCarryingPawn(target))
		{
			psychicRitual.ReleaseAllPawnsAndBuildings();
			return true;
		}
		return false;
	}

	public override bool ClearJobOnStart(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
		Scribe_Defs.Look(ref targetRole, "targetRole");
		Scribe_References.Look(ref invoker, "invoker");
		Scribe_References.Look(ref target, "target");
	}
}
