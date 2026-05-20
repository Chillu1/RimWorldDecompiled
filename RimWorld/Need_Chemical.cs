using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Need_Chemical : Need
{
	private const float ThreshDesire = 0.01f;

	private const float ThreshSatisfied = 0.1f;

	public override int GUIChangeArrow => -1;

	public DrugDesireCategory CurCategory
	{
		get
		{
			if (CurLevel > 0.1f)
			{
				return DrugDesireCategory.Satisfied;
			}
			if (CurLevel > 0.01f)
			{
				return DrugDesireCategory.Desire;
			}
			return DrugDesireCategory.Withdrawal;
		}
	}

	public override float CurLevel
	{
		get
		{
			return base.CurLevel;
		}
		set
		{
			DrugDesireCategory curCategory = CurCategory;
			base.CurLevel = value;
			if (CurCategory != curCategory)
			{
				CategoryChanged();
			}
		}
	}

	public Hediff_Addiction AddictionHediff
	{
		get
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i] is Hediff_Addiction hediff_Addiction && hediff_Addiction.def.chemicalNeed == def)
				{
					return hediff_Addiction;
				}
			}
			return null;
		}
	}

	private float ChemicalFallPerTick => def.fallPerDay / 60000f;

	public Need_Chemical(Pawn pawn)
		: base(pawn)
	{
		threshPercents = new List<float>();
		threshPercents.Add(0.1f);
	}

	public override void SetInitialLevel()
	{
		base.CurLevelPercentage = Rand.Range(0.8f, 1f);
	}

	public override void NeedInterval()
	{
		if (!IsFrozen)
		{
			CurLevel -= ChemicalFallPerTick * 150f;
		}
	}

	private void CategoryChanged()
	{
		AddictionHediff?.Notify_NeedCategoryChanged();
	}
}
