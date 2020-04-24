using System.Collections.Generic;
using Verse;

namespace RimWorld
{
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
				List<Need> allNeeds = pawn.needs.AllNeeds;
				for (int i = 0; i < allNeeds.Count; i++)
				{
					if (allNeeds[i].def == def.causesNeed)
					{
						return (Need_Chemical)allNeeds[i];
					}
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
				if (CurStageIndex == 1 && def.CompProps<HediffCompProperties_SeverityPerDay>() != null)
				{
					return base.LabelInBrackets + " " + (1f - Severity).ToStringPercent();
				}
				return base.LabelInBrackets;
			}
		}

		public override int CurStageIndex
		{
			get
			{
				Need_Chemical need = Need;
				if (need == null || need.CurCategory != 0)
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
}
