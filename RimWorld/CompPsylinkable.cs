using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class CompPsylinkable : ThingComp
	{
		private List<Pawn> pawnsThatCanPsylinkLastGrassGrow = new List<Pawn>();

		public const float MaxDistance = 3.9f;

		public CompProperties_Psylinkable Props => (CompProperties_Psylinkable)props;

		public CompSpawnSubplant CompSubplant => parent.TryGetComp<CompSpawnSubplant>();

		private IEnumerable<Pawn> GetPawnsThatCanPsylink(int level = -1)
		{
			return PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Where((Pawn p) => Props.requiredFocus.CanPawnUse(p) && GetRequiredPlantCount(p) <= CompSubplant.SubplantsForReading.Count && (level == -1 || p.GetPsylinkLevel() == level));
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			CompSubplant.onGrassGrown = OnGrassGrown;
		}

		private void OnGrassGrown()
		{
			bool flag = false;
			foreach (Pawn item in GetPawnsThatCanPsylink())
			{
				if (!pawnsThatCanPsylinkLastGrassGrow.Contains(item))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				CompSpawnSubplant compSpawnSubplant = parent.TryGetComp<CompSpawnSubplant>();
				string text = "";
				for (int i = 0; i < Props.requiredSubplantCountPerPsylinkLevel.Count; i++)
				{
					IEnumerable<string> enumerable = from p in GetPawnsThatCanPsylink(i)
						select p.LabelShort;
					if (enumerable.Count() > 0)
					{
						text = text + (string)("- " + "Level".Translate() + " ") + (i + 1) + ": " + Props.requiredSubplantCountPerPsylinkLevel[i] + " " + compSpawnSubplant.Props.subplant.label + " (" + enumerable.ToCommaList() + ")\n";
					}
				}
				Find.LetterStack.ReceiveLetter(Props.enoughPlantsLetterLabel, Props.enoughPlantsLetterText.Formatted(compSpawnSubplant.SubplantsForReading.Count, text.TrimEndNewlines()), LetterDefOf.NeutralEvent, new LookTargets(GetPawnsThatCanPsylink()));
			}
			pawnsThatCanPsylinkLastGrassGrow.Clear();
			pawnsThatCanPsylinkLastGrassGrow.AddRange(GetPawnsThatCanPsylink());
		}

		private int GetRequiredPlantCount(Pawn pawn)
		{
			int psylinkLevel = pawn.GetPsylinkLevel();
			if (parent.TryGetComp<CompSpawnSubplant>() == null)
			{
				Log.Warning("CompPsylinkable with requiredSubplantCountPerPsylinkLevel set on a Thing without CompSpawnSubplant!");
				return -1;
			}
			if (Props.requiredSubplantCountPerPsylinkLevel.Count <= psylinkLevel)
			{
				return Props.requiredSubplantCountPerPsylinkLevel.Last();
			}
			return Props.requiredSubplantCountPerPsylinkLevel[psylinkLevel];
		}

		public AcceptanceReport CanPsylink(Pawn pawn, LocalTargetInfo? knownSpot = null)
		{
			if (pawn.Dead || pawn.Faction != Faction.OfPlayer)
			{
				return false;
			}
			CompSpawnSubplant compSpawnSubplant = parent.TryGetComp<CompSpawnSubplant>();
			int requiredPlantCount = GetRequiredPlantCount(pawn);
			if (requiredPlantCount == -1)
			{
				return false;
			}
			if (!Props.requiredFocus.CanPawnUse(pawn))
			{
				return new AcceptanceReport("BeginLinkingRitualNeedFocus".Translate(Props.requiredFocus.label));
			}
			if (pawn.GetPsylinkLevel() >= pawn.GetMaxPsylinkLevel())
			{
				return new AcceptanceReport("InstallImplantAlreadyMaxLevel".Translate());
			}
			if (!pawn.Map.reservationManager.CanReserve(pawn, parent))
			{
				Pawn pawn2 = pawn.Map.reservationManager.FirstRespectedReserver(parent, pawn);
				return new AcceptanceReport((pawn2 == null) ? "Reserved".Translate() : "ReservedBy".Translate(pawn.LabelShort, pawn2));
			}
			if (compSpawnSubplant.SubplantsForReading.Count < requiredPlantCount)
			{
				return new AcceptanceReport("BeginLinkingRitualNeedSubplants".Translate(requiredPlantCount.ToString(), compSpawnSubplant.Props.subplant.label, compSpawnSubplant.SubplantsForReading.Count.ToString()));
			}
			LocalTargetInfo spot;
			if (knownSpot.HasValue)
			{
				if (!CanUseSpot(pawn, knownSpot.Value))
				{
					return new AcceptanceReport("BeginLinkingRitualNeedLinkSpot".Translate());
				}
			}
			else if (!TryFindLinkSpot(pawn, out spot))
			{
				return new AcceptanceReport("BeginLinkingRitualNeedLinkSpot".Translate());
			}
			return AcceptanceReport.WasAccepted;
		}

		public bool TryFindLinkSpot(Pawn pawn, out LocalTargetInfo spot)
		{
			spot = MeditationUtility.FindMeditationSpot(pawn).spot;
			if (CanUseSpot(pawn, spot))
			{
				return true;
			}
			int num = GenRadial.NumCellsInRadius(2.9f);
			int num2 = GenRadial.NumCellsInRadius(3.9f);
			for (int i = num; i < num2; i++)
			{
				IntVec3 c = parent.Position + GenRadial.RadialPattern[i];
				if (CanUseSpot(pawn, c))
				{
					spot = c;
					return true;
				}
			}
			spot = IntVec3.Zero;
			return false;
		}

		private bool CanUseSpot(Pawn pawn, LocalTargetInfo spot)
		{
			IntVec3 cell = spot.Cell;
			if (cell.DistanceTo(parent.Position) > 3.9f)
			{
				return false;
			}
			if (!cell.Standable(parent.Map))
			{
				return false;
			}
			if (!GenSight.LineOfSight(cell, parent.Position, parent.Map))
			{
				return false;
			}
			if (!pawn.CanReach(spot, PathEndMode.OnCell, Danger.Deadly))
			{
				return false;
			}
			return true;
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
		{
			Pawn pawn2 = pawn;
			CompPsylinkable compPsylinkable = this;
			if (pawn2.Dead || pawn2.Drafted)
			{
				yield break;
			}
			string text = "BeginLinkingRitualFloatMenu".Translate();
			AcceptanceReport acceptanceReport = CanPsylink(pawn2);
			if (!acceptanceReport.Accepted && !string.IsNullOrWhiteSpace(acceptanceReport.Reason))
			{
				text = text + ": " + acceptanceReport.Reason;
			}
			FloatMenuOption floatMenuOption = new FloatMenuOption(text, delegate
			{
				TaggedString psylinkAffectedByTraitsNegativelyWarning = RoyalTitleUtility.GetPsylinkAffectedByTraitsNegativelyWarning(pawn2);
				if ((string)psylinkAffectedByTraitsNegativelyWarning != null)
				{
					Find.WindowStack.Add(new Dialog_MessageBox(psylinkAffectedByTraitsNegativelyWarning, "Confirm".Translate(), delegate
					{
						compPsylinkable.BeginLinkingRitual(pawn2);
					}, "GoBack".Translate()));
				}
				else
				{
					compPsylinkable.BeginLinkingRitual(pawn2);
				}
			});
			floatMenuOption.Disabled = !acceptanceReport.Accepted;
			yield return floatMenuOption;
		}

		private void BeginLinkingRitual(Pawn pawn)
		{
			if (TryFindLinkSpot(pawn, out LocalTargetInfo spot) && CanPsylink(pawn, spot).Accepted)
			{
				Job job = JobMaker.MakeJob(JobDefOf.LinkPsylinkable, parent, spot);
				pawn.jobs.TryTakeOrderedJob(job);
			}
		}

		public void FinishLinkingRitual(Pawn pawn)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Psylinkables are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 5464564);
				return;
			}
			MoteMaker.MakeStaticMote(parent.Position, pawn.Map, ThingDefOf.Mote_PsycastAreaEffect, 10f);
			SoundDefOf.PsycastPsychicPulse.PlayOneShot(new TargetInfo(parent));
			CompSpawnSubplant compSpawnSubplant = parent.TryGetComp<CompSpawnSubplant>();
			int requiredPlantCount = GetRequiredPlantCount(pawn);
			List<Thing> list = compSpawnSubplant.SubplantsForReading.OrderByDescending((Thing p) => p.Position.DistanceTo(parent.Position)).ToList();
			for (int i = 0; i < requiredPlantCount && i < list.Count; i++)
			{
				list[i].Destroy();
			}
			compSpawnSubplant.Cleanup();
			pawn.ChangePsylinkLevel(1);
			string str = "LetterTextLinkingRitualCompleted".Translate(pawn.Named("PAWN"), parent.Named("LINKABLE"));
			Find.LetterStack.ReceiveLetter("LetterLabelLinkingRitualCompleted".Translate(), str, LetterDefOf.PositiveEvent, new LookTargets(pawn, parent));
		}

		public override void PostExposeData()
		{
			Scribe_Collections.Look(ref pawnsThatCanPsylinkLastGrassGrow, "pawnsThatCanPsylinkLastGrassGrow", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pawnsThatCanPsylinkLastGrassGrow.RemoveAll((Pawn x) => x == null);
			}
		}
	}
}
