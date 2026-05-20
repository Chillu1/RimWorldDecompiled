using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Hediff_Addiction : HediffWithComps
{
	private const int DefaultStageIndex = 0;

	private const int WithdrawalStageIndex = 1;

	public Need_Chemical Need
	{
		get
		{
			if (pawn.Dead)
			{
				return null;
			}
			if (pawn.needs.TryGetNeed(def.chemicalNeed, out var need))
			{
				return (Need_Chemical)need;
			}
			return null;
		}
	}

	public ChemicalDef Chemical
	{
		get
		{
			List<ChemicalDef> allDefsListForReading = DefDatabase<ChemicalDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].addictionHediff == def)
				{
					return allDefsListForReading[i];
				}
			}
			return null;
		}
	}

	public override string LabelInBrackets
	{
		get
		{
			string labelInBrackets = base.LabelInBrackets;
			string text = (1f - Severity).ToStringPercent("F0");
			if (def.CompProps<HediffCompProperties_SeverityPerDay>() != null)
			{
				if (!labelInBrackets.NullOrEmpty())
				{
					return labelInBrackets + " " + text;
				}
				return text;
			}
			return labelInBrackets;
		}
	}

	public override string TipStringExtra
	{
		get
		{
			string text = base.TipStringExtra;
			Need_Chemical need = Need;
			if (need != null)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text += "CreatesNeed".Translate() + ": " + need.LabelCap + " (" + need.CurLevelPercentage.ToStringPercent("F0") + ")";
			}
			return text;
		}
	}

	public override int CurStageIndex
	{
		get
		{
			Need_Chemical need = Need;
			if (need == null || need.CurCategory != DrugDesireCategory.Withdrawal)
			{
				return 0;
			}
			return 1;
		}
	}

	public void Notify_NeedCategoryChanged()
	{
		pawn.health.Notify_HediffChanged(this);
	}
}
