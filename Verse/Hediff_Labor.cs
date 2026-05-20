using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.AI.Group;

namespace Verse;

public class Hediff_Labor : HediffWithParents
{
	private RitualRole testForDoctor;

	private int laborPushingDuration;

	private Effecter progressBar;

	public override string LabelBase => "Labor".Translate();

	public override string LabelInBrackets
	{
		get
		{
			string labelInBrackets = base.LabelInBrackets;
			if (!labelInBrackets.NullOrEmpty())
			{
				return "LaborDilation".Translate() + ", " + labelInBrackets;
			}
			return "LaborDilation".Translate();
		}
	}

	public float Progress
	{
		get
		{
			HediffComp_Disappears hediffComp_Disappears = this.TryGetComp<HediffComp_Disappears>();
			return 1f - (float)(hediffComp_Disappears.ticksToDisappear + laborPushingDuration) / (float)Mathf.Max(1, hediffComp_Disappears.disappearsAfterTicks + laborPushingDuration);
		}
	}

	private static int GetRandomLaborPushingDuration()
	{
		return HediffDefOf.PregnancyLaborPushing.CompProps<HediffCompProperties_Disappears>().disappearsAfterTicks.RandomInRange;
	}

	public override void PostAdd(DamageInfo? dinfo)
	{
		base.PostAdd(dinfo);
		Severity = def.stages.RandomElement().minSeverity;
		laborPushingDuration = GetRandomLaborPushingDuration();
	}

	private static TargetInfo BestBed(Pawn mother)
	{
		return mother.CurrentBed() ?? RestUtility.FindPatientBedFor(mother);
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		Lord lord = pawn.GetLord();
		if (lord == null || lord.LordJob == null)
		{
			Precept_Ritual precept_Ritual = (Precept_Ritual)pawn.Ideo.GetPrecept(PreceptDefOf.ChildBirth);
			TargetInfo targetInfo = BestBed(pawn);
			Command_Ritual command_Ritual = new Command_Ritual(precept_Ritual, targetInfo, null, new Dictionary<string, Pawn> { { "mother", pawn } });
			if (!targetInfo.IsValid)
			{
				command_Ritual.Disabled = true;
				command_Ritual.disabledReason = "NoAppropriateBedChildBirth".Translate();
			}
			else if (!FoundBirthDoctor(precept_Ritual))
			{
				command_Ritual.Disabled = true;
				command_Ritual.disabledReason = "NoDoctorChildBirth".Translate();
			}
			yield return command_Ritual;
		}
		if (DebugSettings.ShowDevGizmos)
		{
			yield return PregnancyUtility.BirthQualityGizmo(pawn);
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Force progress to labor pushing";
			command_Action.action = delegate
			{
				pawn.health.RemoveHediff(this);
			};
			yield return command_Action;
		}
	}

	public override void PreRemoved()
	{
		base.PreRemoved();
		Hediff_LaborPushing obj = (Hediff_LaborPushing)pawn.health.AddHediff(HediffDefOf.PregnancyLaborPushing);
		if (PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			Messages.Message("MessageColonistInFinalStagesOfLabor".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
		}
		obj.SetParents(base.Mother, base.Father, geneSet);
		HediffComp_Disappears hediffComp_Disappears = obj.TryGetComp<HediffComp_Disappears>();
		hediffComp_Disappears.disappearsAfterTicks = laborPushingDuration;
		hediffComp_Disappears.ticksToDisappear = laborPushingDuration;
		obj.laborDuration = this.TryGetComp<HediffComp_Disappears>().disappearsAfterTicks;
	}

	private bool FoundBirthDoctor(Precept_Ritual birthRitual)
	{
		if (testForDoctor == null)
		{
			testForDoctor = birthRitual.behavior.def.roles.First((RitualRole r) => r.id == "doctor");
		}
		foreach (Pawn item in pawn.MapHeld.mapPawns.FreeColonistsSpawned)
		{
			if (item != pawn && RitualRoleAssignments.PawnNotAssignableReason(item, testForDoctor, birthRitual, null, null, out var _) == null)
			{
				return true;
			}
		}
		return false;
	}

	public override void TickInterval(int delta)
	{
		if (pawn.SpawnedOrAnyParentSpawned)
		{
			if (progressBar == null)
			{
				progressBar = EffecterDefOf.ProgressBarAlwaysVisible.Spawn();
			}
			progressBar.EffectTick(pawn, TargetInfo.Invalid);
			MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBar.children[0]).mote;
			if (mote != null)
			{
				mote.progress = Progress;
			}
		}
		else
		{
			progressBar?.Cleanup();
			progressBar = null;
		}
	}

	public override void PostRemoved()
	{
		progressBar?.Cleanup();
		progressBar = null;
		base.PostRemoved();
	}

	public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
	{
		base.Notify_PawnDied(dinfo, culprit);
		progressBar?.Cleanup();
		progressBar = null;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref laborPushingDuration, "laborPushingDuration", -1);
		if (Scribe.mode == LoadSaveMode.LoadingVars && laborPushingDuration == -1)
		{
			laborPushingDuration = GetRandomLaborPushingDuration();
		}
	}
}
