using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class CompUseEffect_FixWorstHealthCondition : CompUseEffect
	{
		private float HandCoverageAbsWithChildren => ThingDefOf.Human.race.body.GetPartsWithDef(BodyPartDefOf.Hand).First().coverageAbsWithChildren;

		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);
			Hediff hediff = FindLifeThreateningHediff(usedBy);
			if (hediff != null)
			{
				Cure(hediff);
				return;
			}
			if (HealthUtility.TicksUntilDeathDueToBloodLoss(usedBy) < 2500)
			{
				Hediff hediff2 = FindMostBleedingHediff(usedBy);
				if (hediff2 != null)
				{
					Cure(hediff2);
					return;
				}
			}
			if (usedBy.health.hediffSet.GetBrain() != null)
			{
				Hediff_Injury hediff_Injury = FindPermanentInjury(usedBy, Gen.YieldSingle(usedBy.health.hediffSet.GetBrain()));
				if (hediff_Injury != null)
				{
					Cure(hediff_Injury);
					return;
				}
			}
			BodyPartRecord bodyPartRecord = FindBiggestMissingBodyPart(usedBy, HandCoverageAbsWithChildren);
			if (bodyPartRecord != null)
			{
				Cure(bodyPartRecord, usedBy);
				return;
			}
			Hediff_Injury hediff_Injury2 = FindPermanentInjury(usedBy, from x in usedBy.health.hediffSet.GetNotMissingParts()
				where x.def == BodyPartDefOf.Eye
				select x);
			if (hediff_Injury2 != null)
			{
				Cure(hediff_Injury2);
				return;
			}
			Hediff hediff3 = FindImmunizableHediffWhichCanKill(usedBy);
			if (hediff3 != null)
			{
				Cure(hediff3);
				return;
			}
			Hediff hediff4 = FindNonInjuryMiscBadHediff(usedBy, onlyIfCanKill: true);
			if (hediff4 != null)
			{
				Cure(hediff4);
				return;
			}
			Hediff hediff5 = FindNonInjuryMiscBadHediff(usedBy, onlyIfCanKill: false);
			if (hediff5 != null)
			{
				Cure(hediff5);
				return;
			}
			if (usedBy.health.hediffSet.GetBrain() != null)
			{
				Hediff_Injury hediff_Injury3 = FindInjury(usedBy, Gen.YieldSingle(usedBy.health.hediffSet.GetBrain()));
				if (hediff_Injury3 != null)
				{
					Cure(hediff_Injury3);
					return;
				}
			}
			BodyPartRecord bodyPartRecord2 = FindBiggestMissingBodyPart(usedBy);
			if (bodyPartRecord2 != null)
			{
				Cure(bodyPartRecord2, usedBy);
				return;
			}
			Hediff_Addiction hediff_Addiction = FindAddiction(usedBy);
			if (hediff_Addiction != null)
			{
				Cure(hediff_Addiction);
				return;
			}
			Hediff_Injury hediff_Injury4 = FindPermanentInjury(usedBy);
			if (hediff_Injury4 != null)
			{
				Cure(hediff_Injury4);
				return;
			}
			Hediff_Injury hediff_Injury5 = FindInjury(usedBy);
			if (hediff_Injury5 != null)
			{
				Cure(hediff_Injury5);
			}
		}

		private Hediff FindLifeThreateningHediff(Pawn pawn)
		{
			Hediff hediff = null;
			float num = -1f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (!hediffs[i].Visible || !hediffs[i].def.everCurableByItem || hediffs[i].FullyImmune())
				{
					continue;
				}
				bool num2 = hediffs[i].CurStage?.lifeThreatening ?? false;
				bool flag = hediffs[i].def.lethalSeverity >= 0f && hediffs[i].Severity / hediffs[i].def.lethalSeverity >= 0.8f;
				if (num2 || flag)
				{
					float num3 = (hediffs[i].Part != null) ? hediffs[i].Part.coverageAbsWithChildren : 999f;
					if (hediff == null || num3 > num)
					{
						hediff = hediffs[i];
						num = num3;
					}
				}
			}
			return hediff;
		}

		private Hediff FindMostBleedingHediff(Pawn pawn)
		{
			float num = 0f;
			Hediff hediff = null;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].Visible && hediffs[i].def.everCurableByItem)
				{
					float bleedRate = hediffs[i].BleedRate;
					if (bleedRate > 0f && (bleedRate > num || hediff == null))
					{
						num = bleedRate;
						hediff = hediffs[i];
					}
				}
			}
			return hediff;
		}

		private Hediff FindImmunizableHediffWhichCanKill(Pawn pawn)
		{
			Hediff hediff = null;
			float num = -1f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].Visible && hediffs[i].def.everCurableByItem && hediffs[i].TryGetComp<HediffComp_Immunizable>() != null && !hediffs[i].FullyImmune() && CanEverKill(hediffs[i]))
				{
					float severity = hediffs[i].Severity;
					if (hediff == null || severity > num)
					{
						hediff = hediffs[i];
						num = severity;
					}
				}
			}
			return hediff;
		}

		private Hediff FindNonInjuryMiscBadHediff(Pawn pawn, bool onlyIfCanKill)
		{
			Hediff hediff = null;
			float num = -1f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].Visible && hediffs[i].def.isBad && hediffs[i].def.everCurableByItem && !(hediffs[i] is Hediff_Injury) && !(hediffs[i] is Hediff_MissingPart) && !(hediffs[i] is Hediff_Addiction) && !(hediffs[i] is Hediff_AddedPart) && (!onlyIfCanKill || CanEverKill(hediffs[i])))
				{
					float num2 = (hediffs[i].Part != null) ? hediffs[i].Part.coverageAbsWithChildren : 999f;
					if (hediff == null || num2 > num)
					{
						hediff = hediffs[i];
						num = num2;
					}
				}
			}
			return hediff;
		}

		private BodyPartRecord FindBiggestMissingBodyPart(Pawn pawn, float minCoverage = 0f)
		{
			BodyPartRecord bodyPartRecord = null;
			foreach (Hediff_MissingPart missingPartsCommonAncestor in pawn.health.hediffSet.GetMissingPartsCommonAncestors())
			{
				if (!(missingPartsCommonAncestor.Part.coverageAbsWithChildren < minCoverage) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(missingPartsCommonAncestor.Part) && (bodyPartRecord == null || missingPartsCommonAncestor.Part.coverageAbsWithChildren > bodyPartRecord.coverageAbsWithChildren))
				{
					bodyPartRecord = missingPartsCommonAncestor.Part;
				}
			}
			return bodyPartRecord;
		}

		private Hediff_Addiction FindAddiction(Pawn pawn)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff_Addiction hediff_Addiction = hediffs[i] as Hediff_Addiction;
				if (hediff_Addiction != null && hediff_Addiction.Visible && hediff_Addiction.def.everCurableByItem)
				{
					return hediff_Addiction;
				}
			}
			return null;
		}

		private Hediff_Injury FindPermanentInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
		{
			Hediff_Injury hediff_Injury = null;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff_Injury hediff_Injury2 = hediffs[i] as Hediff_Injury;
				if (hediff_Injury2 != null && hediff_Injury2.Visible && hediff_Injury2.IsPermanent() && hediff_Injury2.def.everCurableByItem && (allowedBodyParts == null || allowedBodyParts.Contains(hediff_Injury2.Part)) && (hediff_Injury == null || hediff_Injury2.Severity > hediff_Injury.Severity))
				{
					hediff_Injury = hediff_Injury2;
				}
			}
			return hediff_Injury;
		}

		private Hediff_Injury FindInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
		{
			Hediff_Injury hediff_Injury = null;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff_Injury hediff_Injury2 = hediffs[i] as Hediff_Injury;
				if (hediff_Injury2 != null && hediff_Injury2.Visible && hediff_Injury2.def.everCurableByItem && (allowedBodyParts == null || allowedBodyParts.Contains(hediff_Injury2.Part)) && (hediff_Injury == null || hediff_Injury2.Severity > hediff_Injury.Severity))
				{
					hediff_Injury = hediff_Injury2;
				}
			}
			return hediff_Injury;
		}

		private void Cure(Hediff hediff)
		{
			HealthUtility.CureHediff(hediff);
			Messages.Message("MessageHediffCuredByItem".Translate(hediff.LabelBase.CapitalizeFirst()), hediff.pawn, MessageTypeDefOf.PositiveEvent);
		}

		private void Cure(BodyPartRecord part, Pawn pawn)
		{
			pawn.health.RestorePart(part);
			Messages.Message("MessageBodyPartCuredByItem".Translate(part.LabelCap), pawn, MessageTypeDefOf.PositiveEvent);
		}

		private bool CanEverKill(Hediff hediff)
		{
			if (hediff.def.stages != null)
			{
				for (int i = 0; i < hediff.def.stages.Count; i++)
				{
					if (hediff.def.stages[i].lifeThreatening)
					{
						return true;
					}
				}
			}
			return hediff.def.lethalSeverity >= 0f;
		}
	}
}
