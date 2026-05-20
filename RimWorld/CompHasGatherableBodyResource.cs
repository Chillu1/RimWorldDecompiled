using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class CompHasGatherableBodyResource : ThingComp
{
	protected float fullness;

	protected abstract int GatherResourcesIntervalDays { get; }

	protected abstract int ResourceAmount { get; }

	protected abstract ThingDef ResourceDef { get; }

	protected abstract string SaveKey { get; }

	public float Fullness => fullness;

	protected virtual bool Active
	{
		get
		{
			if (parent.Faction == null)
			{
				return false;
			}
			if (parent.Suspended)
			{
				return false;
			}
			return true;
		}
	}

	public bool ActiveAndFull
	{
		get
		{
			if (!Active)
			{
				return false;
			}
			return fullness >= 1f;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref fullness, SaveKey, 0f);
	}

	public override void CompTick()
	{
		if (Active)
		{
			float num = 1f / (float)(GatherResourcesIntervalDays * 60000);
			if (parent is Pawn pawn)
			{
				num *= PawnUtility.BodyResourceGrowthSpeed(pawn);
			}
			fullness += num;
			if (fullness > 1f)
			{
				fullness = 1f;
			}
		}
	}

	public void Gathered(Pawn doer)
	{
		if (!Active)
		{
			Log.Error(doer?.ToString() + " gathered body resources while not Active: " + parent);
		}
		if (!Rand.Chance(doer.GetStatValue(StatDefOf.AnimalGatherYield)))
		{
			MoteMaker.ThrowText((doer.DrawPos + parent.DrawPos) / 2f, parent.Map, "TextMote_ProductWasted".Translate(), 3.65f);
		}
		else
		{
			int num = GenMath.RoundRandom((float)ResourceAmount * fullness);
			while (num > 0)
			{
				int num2 = Mathf.Clamp(num, 1, ResourceDef.stackLimit);
				num -= num2;
				Thing thing = ThingMaker.MakeThing(ResourceDef);
				thing.stackCount = num2;
				GenPlace.TryPlaceThing(thing, doer.Position, doer.Map, ThingPlaceMode.Near);
			}
		}
		fullness = 0f;
	}
}
