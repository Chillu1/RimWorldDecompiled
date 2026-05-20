using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_BestowingCeremony_Wait : LordToil_Wait
{
	public Pawn target;

	public Pawn bestower;

	public LordToil_BestowingCeremony_Wait(Pawn target, Pawn bestower)
	{
		this.target = target;
		this.bestower = bestower;
	}

	public override void Init()
	{
		Messages.Message("MessageBestowerWaiting".Translate(target.Named("TARGET"), lord.ownedPawns[0].Named("BESTOWER")), new LookTargets(new Pawn[2]
		{
			target,
			lord.ownedPawns[0]
		}), MessageTypeDefOf.NeutralEvent);
	}

	protected override void DecoratePawnDuty(PawnDuty duty)
	{
		duty.focus = target;
	}

	public override void DrawPawnGUIOverlay(Pawn pawn)
	{
		pawn.Map.overlayDrawer.DrawOverlay(pawn, OverlayTypes.QuestionMark);
	}

	public override IEnumerable<Gizmo> GetPawnGizmos(Pawn p)
	{
		if (p == bestower)
		{
			LordJob_BestowingCeremony job = (LordJob_BestowingCeremony)lord.LordJob;
			yield return new Command_BestowerCeremony(job, bestower, target, StartRitual);
		}
	}

	public override IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn target, Pawn forPawn)
	{
		LordJob_BestowingCeremony lordJob = (LordJob_BestowingCeremony)lord.LordJob;
		if (target != bestower)
		{
			yield break;
		}
		FloatMenuOption floatMenuOption = new FloatMenuOption("BeginRitual".Translate("RitualBestowingCeremony".Translate()), delegate
		{
			Find.WindowStack.Add(new Dialog_BeginRitual("RitualBestowingCeremony".Translate(), null, lordJob.targetSpot.ToTargetInfo(bestower.Map), bestower.Map, delegate(RitualRoleAssignments assignments)
			{
				StartRitual(assignments.Participants.Where((Pawn p) => p != bestower).ToList());
				return true;
			}, bestower, null, delegate(Pawn pawn, bool voluntary, bool allowOtherIdeos)
			{
				if (pawn.GetLord()?.LordJob is LordJob_Ritual)
				{
					return false;
				}
				if (pawn.IsSubhuman)
				{
					return false;
				}
				return !pawn.IsPrisonerOfColony && !pawn.RaceProps.Animal;
			}, "Begin".Translate(), new List<Pawn> { bestower, this.target }, null, RitualOutcomeEffectDefOf.BestowingCeremony));
		});
		if (!LordJob_BestowingCeremony.TryGetUsableSpotAdjacentToBestower(bestower).IsValid)
		{
			floatMenuOption.Disabled = true;
			floatMenuOption.Label += " (" + "BestowingSpotUnavailable".Translate() + ")";
		}
		yield return floatMenuOption;
	}

	private void StartRitual(List<Pawn> pawns)
	{
		foreach (Pawn pawn in pawns)
		{
			if (pawn.GetLord()?.LordJob is LordJob_VoluntarilyJoinable)
			{
				pawn.GetLord().Notify_PawnLost(pawn, PawnLostCondition.LeftVoluntarily);
			}
		}
		lord.AddPawns(pawns);
		((LordJob_BestowingCeremony)lord.LordJob).colonistParticipants.AddRange(pawns);
		lord.ReceiveMemo(LordJob_BestowingCeremony.MemoCeremonyStarted);
		foreach (Pawn pawn2 in pawns)
		{
			if (pawn2.drafter != null)
			{
				pawn2.drafter.Drafted = false;
			}
			if (!pawn2.Awake())
			{
				RestUtility.WakeUp(pawn2);
			}
		}
	}
}
