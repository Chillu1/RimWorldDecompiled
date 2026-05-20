using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

[StaticConstructorOnStartup]
public class HumanOvum : ThingWithComps
{
	public static readonly CachedTexture FertilizeIcon = new CachedTexture("UI/Gizmos/Fertilize");

	private Pawn fertilizingMan;

	private static readonly Texture2D CancelFertilizeIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (!ModsConfig.BiotechActive || base.MapHeld == null)
		{
			yield break;
		}
		if (fertilizingMan?.CurJob?.def != JobDefOf.FertilizeOvum || fertilizingMan.CurJob.GetTarget(TargetIndex.A).Thing != this)
		{
			fertilizingMan = null;
		}
		if (fertilizingMan == null)
		{
			List<FloatMenuOption> maleOptions = new List<FloatMenuOption>();
			foreach (Pawn item in base.MapHeld.mapPawns.FreeColonistsSpawned)
			{
				FloatMenuOption floatMenuOption = CanFertilizeFloatOption(item, pawnAsLabel: true);
				if (floatMenuOption != null)
				{
					maleOptions.Add(floatMenuOption);
				}
			}
			Command_Action command_Action = new Command_Action
			{
				defaultLabel = string.Format("{0}...", "Fertilize".Translate()),
				defaultDesc = "FertilizeGizmoDescription".Translate(),
				icon = FertilizeIcon.Texture,
				action = delegate
				{
					Find.WindowStack.Add(new FloatMenu(maleOptions));
				}
			};
			if (maleOptions.Count == 0)
			{
				command_Action.Disable("FertilizeDisabledNoMales".Translate());
			}
			yield return command_Action;
		}
		else
		{
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "FertilizeCancel".Translate(fertilizingMan.LabelShort);
			command_Action2.defaultDesc = "FertilizeCancel".Translate(fertilizingMan.LabelShort);
			command_Action2.icon = CancelFertilizeIcon;
			command_Action2.action = delegate
			{
				fertilizingMan.jobs.EndCurrentJob(JobCondition.InterruptForced);
			};
			command_Action2.activateSound = SoundDefOf.Designate_Cancel;
			yield return command_Action2;
		}
	}

	public Thing ProduceEmbryo(Pawn father)
	{
		HumanEmbryo humanEmbryo = (HumanEmbryo)ThingMaker.MakeThing(ThingDefOf.HumanEmbryo);
		CompHasPawnSources comp = humanEmbryo.GetComp<CompHasPawnSources>();
		List<Pawn> pawnSources = GetComp<CompHasPawnSources>().pawnSources;
		if (!pawnSources.NullOrEmpty())
		{
			foreach (Pawn item in pawnSources)
			{
				comp.AddSource(item);
			}
		}
		comp.AddSource(father);
		if (!humanEmbryo.TryPopulateGenes())
		{
			humanEmbryo.Destroy();
			return null;
		}
		return humanEmbryo;
	}

	public override Thing SplitOff(int count)
	{
		return base.SplitOff(count) as HumanOvum;
	}

	public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
		foreach (FloatMenuOption floatMenuOption2 in base.GetFloatMenuOptions(selPawn))
		{
			yield return floatMenuOption2;
		}
		FloatMenuOption floatMenuOption = CanFertilizeFloatOption(selPawn, pawnAsLabel: false);
		if (floatMenuOption != null)
		{
			yield return floatMenuOption;
		}
	}

	private FloatMenuOption CanFertilizeFloatOption(Pawn pawn, bool pawnAsLabel)
	{
		AcceptanceReport acceptanceReport = CanFertilizeReport(pawn);
		if (acceptanceReport.Accepted)
		{
			return new FloatMenuOption(pawnAsLabel ? pawn.LabelShort : "FertilizeThing".Translate(this).ToString(), delegate
			{
				Pawn pawn2 = GetComp<CompHasPawnSources>().pawnSources.FirstOrDefault();
				PawnRelationDef relation;
				float num = PregnancyUtility.InbredChanceFromParents(pawn, pawn2, out relation);
				if (num > 0f)
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("FertilizeOvumInbredChance".Translate(pawn2.Named("MOTHER"), pawn.Named("FATHER"), relation.label.Named("RELATION"), num.ToStringPercent().Named("CHANCE")), delegate
					{
						TakeJob();
					}));
				}
				else
				{
					TakeJob();
				}
			});
		}
		if (!acceptanceReport.Reason.NullOrEmpty())
		{
			return new FloatMenuOption(pawnAsLabel ? "DisabledOption".Translate(pawn.LabelShort, acceptanceReport.Reason).ToString() : ("FertilizeCannotFloatOption".Translate().ToString() + ": " + acceptanceReport.Reason), null);
		}
		return null;
		void TakeJob()
		{
			Job job = JobMaker.MakeJob(JobDefOf.FertilizeOvum, this);
			job.count = 1;
			if (pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc))
			{
				fertilizingMan = pawn;
			}
		}
	}

	private AcceptanceReport CanFertilizeReport(Pawn pawn)
	{
		if (pawn.gender != Gender.Male)
		{
			return false;
		}
		if (pawn.IsQuestLodger())
		{
			return false;
		}
		if (!pawn.CanReach(this, PathEndMode.OnCell, Danger.Deadly))
		{
			return "NoPath".Translate();
		}
		if ((float)pawn.ageTracker.AgeBiologicalYears < 14f)
		{
			return "CannotMustBeAge".Translate(14f).CapitalizeFirst();
		}
		if (pawn.Sterile())
		{
			return "CannotSterile".Translate();
		}
		if (pawn.Downed || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return "Incapacitated".Translate().ToLower();
		}
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref fertilizingMan, "fertilizingMan");
	}
}
