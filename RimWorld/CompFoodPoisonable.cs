using Verse;

namespace RimWorld;

public class CompFoodPoisonable : ThingComp
{
	private float poisonPct;

	private FoodPoisonCause cause;

	public float PoisonPercent => poisonPct;

	public FoodPoisonCause Cause => cause;

	public void SetPoisoned(FoodPoisonCause newCause)
	{
		poisonPct = 1f;
		cause = newCause;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref poisonPct, "poisonPct", 0f);
		Scribe_Values.Look(ref cause, "cause", FoodPoisonCause.Unknown);
	}

	public override void PostSplitOff(Thing piece)
	{
		base.PostSplitOff(piece);
		CompFoodPoisonable compFoodPoisonable = piece.TryGetComp<CompFoodPoisonable>();
		compFoodPoisonable.poisonPct = poisonPct;
		compFoodPoisonable.cause = cause;
	}

	public override void Notify_RecipeProduced(Pawn pawn)
	{
		if (Rand.Chance(pawn.GetRoom()?.GetStat(RoomStatDefOf.FoodPoisonChance) ?? RoomStatDefOf.FoodPoisonChance.roomlessScore))
		{
			SetPoisoned(FoodPoisonCause.FilthyKitchen);
		}
		else if (Rand.Chance(pawn.GetStatValue(StatDefOf.FoodPoisonChance)))
		{
			SetPoisoned(FoodPoisonCause.IncompetentCook);
		}
	}

	public override void PreAbsorbStack(Thing otherStack, int count)
	{
		base.PreAbsorbStack(otherStack, count);
		CompFoodPoisonable compFoodPoisonable = otherStack.TryGetComp<CompFoodPoisonable>();
		if (cause == FoodPoisonCause.Unknown && compFoodPoisonable.cause != FoodPoisonCause.Unknown)
		{
			cause = compFoodPoisonable.cause;
		}
		else if (compFoodPoisonable.cause != FoodPoisonCause.Unknown && cause != FoodPoisonCause.Unknown)
		{
			float num = poisonPct * (float)parent.stackCount;
			float num2 = compFoodPoisonable.poisonPct * (float)count;
			cause = ((num > num2) ? cause : compFoodPoisonable.cause);
		}
		poisonPct = GenMath.WeightedAverage(poisonPct, parent.stackCount, compFoodPoisonable.poisonPct, count);
	}

	public override void PostIngested(Pawn ingester)
	{
		if (Rand.Chance(poisonPct * FoodUtility.GetFoodPoisonChanceFactor(ingester)))
		{
			FoodUtility.AddFoodPoisoningHediff(ingester, parent, cause);
		}
	}
}
