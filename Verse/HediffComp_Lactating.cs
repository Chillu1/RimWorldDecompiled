using RimWorld;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class HediffComp_Lactating : HediffComp_Chargeable
	{
		public override string CompTipStringExtra
		{
			get
			{
				if (base.Charge >= base.Props.fullChargeAmount)
				{
					return "LactatingStoppedBecauseFull".Translate();
				}
				float num = PawnUtility.BodyResourceGrowthSpeed(base.Pawn);
				if (num == 0f)
				{
					return "LactatingStoppedBecauseHungry".Translate().Colorize(ColorLibrary.RedReadable);
				}
				float f = AddedNutritionPerDay() * num;
				return "LactatingAddedNutritionPerDay".Translate(f.ToStringByStyle(ToStringStyle.FloatMaxTwo), num);
			}
		}

		public override void TryCharge(float desiredChargeAmount)
		{
			if (base.Pawn?.needs?.food != null)
			{
				desiredChargeAmount *= PawnUtility.BodyResourceGrowthSpeed(base.Pawn);
				float num = Mathf.Min(desiredChargeAmount, base.Pawn.needs.food.CurLevel);
				base.Pawn.needs.food.CurLevel -= num;
				base.TryCharge(num);
			}
		}

		public float AddedNutritionPerDay()
		{
			return base.Props.fullChargeAmount * 60000f / (float)base.Props.ticksToFullCharge;
		}
	}
}
