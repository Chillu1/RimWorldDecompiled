using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_Ritual_Duel : LordJob_Ritual
{
	public List<Pawn> duelists = new List<Pawn>();

	public bool usedWeapon;

	private bool duelStarted;

	private int attacksThisStage;

	private int movingTicks;

	private static readonly IntRange AttacksPerStage = new IntRange(4, 8);

	private static readonly IntRange MovingTicksPerStage = new IntRange(360, 600);

	public DuelBehaviorStage CurrentDuelStage
	{
		get
		{
			if (attacksThisStage <= 0)
			{
				return DuelBehaviorStage.Move;
			}
			return DuelBehaviorStage.Attack;
		}
	}

	public override bool NeverInRestraints => true;

	public LordJob_Ritual_Duel()
	{
	}

	public LordJob_Ritual_Duel(TargetInfo selectedTarget, Precept_Ritual ritual, RitualObligation obligation, List<RitualStage> allStages, RitualRoleAssignments assignments, Pawn organizer = null)
		: base(selectedTarget, ritual, obligation, allStages, assignments, organizer)
	{
		foreach (RitualRole item2 in assignments.AllRolesForReading)
		{
			if (item2 != null && item2.id.Contains("duelist"))
			{
				Pawn item = assignments.FirstAssignedPawn(item2);
				duelists.Add(item);
				pawnsDeathIgnored.Add(item);
			}
		}
	}

	public override bool ShouldRemovePawn(Pawn p, PawnLostCondition reason)
	{
		if (reason == PawnLostCondition.Incapped)
		{
			return false;
		}
		return base.ShouldRemovePawn(p, reason);
	}

	public void StartDuelIfNotStartedYet()
	{
		if (!duelStarted)
		{
			duelStarted = true;
			StartDuel();
		}
	}

	public override float VoluntaryJoinPriorityFor(Pawn p)
	{
		if (duelists.Contains(p))
		{
			return 1f;
		}
		return base.VoluntaryJoinPriorityFor(p);
	}

	private void InterruptDuelistJobs()
	{
		foreach (Pawn duelist in duelists)
		{
			duelist.jobs?.CheckForJobOverride();
		}
	}

	private void StartDuel()
	{
		StartMoving();
	}

	private void StartMoving()
	{
		attacksThisStage = 0;
		movingTicks = MovingTicksPerStage.RandomInRange;
		InterruptDuelistJobs();
	}

	private void StartAttacking()
	{
		movingTicks = 0;
		attacksThisStage = AttacksPerStage.RandomInRange;
		InterruptDuelistJobs();
	}

	public override void LordJobTick()
	{
		base.LordJobTick();
		if (movingTicks > 0)
		{
			movingTicks--;
			if (movingTicks <= 0)
			{
				StartAttacking();
			}
		}
	}

	public override void ApplyOutcome(float progress, bool showFinishedMessage = true, bool showFailedMessage = true, bool cancelled = false)
	{
		foreach (Pawn duelist in duelists)
		{
			if (duelist.equipment.Primary != null)
			{
				duelist.equipment.TryDropEquipment(duelist.equipment.Primary, out var _, duelist.PositionHeld);
			}
		}
		base.ApplyOutcome(progress, showFinishedMessage, showFailedMessage, cancelled);
	}

	public void Notify_MeleeAttack(Pawn duelist, Thing victim)
	{
		attacksThisStage--;
		if (attacksThisStage <= 0)
		{
			StartMoving();
		}
		if (!usedWeapon)
		{
			Verb verb = duelist.meleeVerbs.TryGetMeleeVerb(victim);
			if (verb != null && verb.EquipmentSource != null && verb.EquipmentSource.def.IsMeleeWeapon)
			{
				usedWeapon = true;
			}
		}
	}

	protected override bool ShouldCallOffBecausePawnNoLongerOwned(Pawn p)
	{
		int num = 0;
		foreach (Pawn duelist in duelists)
		{
			if (!duelist.DeadOrDowned)
			{
				num++;
			}
		}
		return num <= 1;
	}

	public Pawn Opponent(Pawn duelist)
	{
		return duelists[(duelists.IndexOf(duelist) == 0) ? 1 : 0];
	}

	public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
	{
		if (!p.DeadOrDowned)
		{
			Cancel();
		}
		if (p.Dead && duelists.Contains(p))
		{
			p.health.killedByRitual = true;
		}
	}

	public override bool BlocksSocialInteraction(Pawn pawn)
	{
		return duelists.Contains(pawn);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref duelists, "duelists", true, LookMode.Reference);
		Scribe_Values.Look(ref usedWeapon, "usedWeapon", defaultValue: false);
		Scribe_Values.Look(ref movingTicks, "movingTicks", 0);
		Scribe_Values.Look(ref attacksThisStage, "attacksThisStage", 0);
		Scribe_Values.Look(ref duelStarted, "duelStarted", defaultValue: false);
	}
}
