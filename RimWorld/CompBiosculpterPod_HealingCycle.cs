using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public abstract class CompBiosculpterPod_HealingCycle : CompBiosculpterPod_Cycle
{
	private List<string> tmpWillHealHediffs = new List<string>();

	private List<string> tmpCanHealHediffs = new List<string>();

	private List<Hediff> tmpHediffs = new List<Hediff>();

	public new CompProperties_BiosculpterPod_HealingCycle Props => (CompProperties_BiosculpterPod_HealingCycle)props;

	public abstract bool Regenerate { get; }

	public override string Description(Pawn tunedFor)
	{
		string text = base.Description(tunedFor);
		if (!Props.extraRequiredIngredients.NullOrEmpty())
		{
			text += "\n\n" + "BiosculpterRequiresIngredients".Translate() + ":\n" + Props.extraRequiredIngredients.Select((ThingDefCountClass x) => x.Summary).ToLineList("  - ", capitalizeItems: true);
		}
		string healingDescriptionForPawn = GetHealingDescriptionForPawn(tunedFor);
		if (!healingDescriptionForPawn.NullOrEmpty())
		{
			text = text + "\n\n" + healingDescriptionForPawn;
		}
		return text;
	}

	public string GetHealingDescriptionForPawn(Pawn pawn)
	{
		try
		{
			string text = "";
			if (pawn != null)
			{
				foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
				{
					if (WillHeal(pawn, hediff))
					{
						tmpWillHealHediffs.Add(HediffLabel(hediff));
					}
					else if (CanPotentiallyHeal(pawn, hediff) && !tmpCanHealHediffs.Contains(HediffLabel(hediff)))
					{
						tmpCanHealHediffs.Add(HediffLabel(hediff));
					}
				}
				if (tmpCanHealHediffs.Count == 1)
				{
					tmpWillHealHediffs.AddRange(tmpCanHealHediffs);
					tmpCanHealHediffs.Clear();
				}
				string text2 = string.Empty;
				if (tmpWillHealHediffs.Any())
				{
					text2 += "HealingCycleWillHeal".Translate() + ":\n" + tmpWillHealHediffs.ToLineList("  - ", capitalizeItems: true);
				}
				if (tmpCanHealHediffs.Any())
				{
					if (!text2.NullOrEmpty())
					{
						text2 += "\n\n";
					}
					text2 += "HealingCycleOneWillHeal".Translate() + ":\n" + tmpCanHealHediffs.ToLineList("  - ", capitalizeItems: true);
				}
				if (!text2.NullOrEmpty())
				{
					text += text2;
				}
			}
			return text;
		}
		finally
		{
			tmpWillHealHediffs.Clear();
			tmpCanHealHediffs.Clear();
		}
		static string HediffLabel(Hediff hediff)
		{
			if (hediff.Part != null && !hediff.def.cureAllAtOnceIfCuredByItem)
			{
				return hediff.Part.Label + " (" + hediff.Label + ")";
			}
			return hediff.Label;
		}
	}

	public override void CycleCompleted(Pawn pawn)
	{
		if (pawn.health == null)
		{
			return;
		}
		tmpHediffs.Clear();
		tmpHediffs.AddRange(pawn.health.hediffSet.hediffs);
		try
		{
			foreach (Hediff tmpHediff in tmpHediffs)
			{
				if (WillHeal(pawn, tmpHediff))
				{
					HealthUtility.Cure(tmpHediff);
				}
				else if (tmpHediff is Hediff_MissingPart { IsFresh: not false } hediff_MissingPart)
				{
					hediff_MissingPart.IsFresh = false;
					pawn.health.Notify_HediffChanged(hediff_MissingPart);
				}
			}
			tmpHediffs.Clear();
			foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
			{
				if (CanPotentiallyHeal(pawn, hediff))
				{
					tmpHediffs.Add(hediff);
				}
			}
			if (tmpHediffs.TryRandomElement(out var result))
			{
				HealthUtility.Cure(result);
				Messages.Message("BiosculpterHealCompletedWithCureMessage".Translate(pawn.Named("PAWN"), result.Named("HEDIFF")), pawn, MessageTypeDefOf.PositiveEvent);
			}
			else
			{
				Messages.Message("BiosculpterHealCompletedMessage".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
			}
		}
		finally
		{
			tmpHediffs.Clear();
		}
	}

	private bool WillHeal(Pawn pawn, Hediff hediff)
	{
		if (!hediff.def.everCurableByItem)
		{
			return false;
		}
		if (hediff.def.chronic)
		{
			return false;
		}
		if (hediff.def.countsAsAddedPartOrImplant)
		{
			return false;
		}
		if (hediff.def == HediffDefOf.BloodLoss)
		{
			return true;
		}
		if (Regenerate && hediff.Part != null && Props.bodyPartsToRestore != null && Props.bodyPartsToRestore.Contains(hediff.Part.def) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(hediff.Part))
		{
			if (hediff.Part.def == BodyPartDefOf.Eye && pawn.Ideo != null && pawn.Ideo.IdeoApprovesOfBlindness())
			{
				return false;
			}
			return true;
		}
		if (hediff is Hediff_Injury && !hediff.IsPermanent())
		{
			return true;
		}
		return false;
	}

	private bool CanPotentiallyHeal(Pawn pawn, Hediff hediff)
	{
		if (!hediff.def.everCurableByItem)
		{
			return false;
		}
		if (hediff.def.countsAsAddedPartOrImplant)
		{
			return false;
		}
		if (Props.conditionsToPossiblyCure != null && Props.conditionsToPossiblyCure.Contains(hediff.def))
		{
			return true;
		}
		if (Regenerate && hediff is Hediff_Injury && hediff.IsPermanent())
		{
			return true;
		}
		return false;
	}
}
