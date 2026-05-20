using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompMetalhorrorInfectible : ThingComp
{
	private List<Pawn> infectedPawns = new List<Pawn>();

	private const float InfectionChance = 0.04f;

	public int Infections => infectedPawns.Count;

	public override void Notify_RecipeProduced(Pawn pawn)
	{
		if (Rand.Chance(0.04f) && MetalhorrorUtility.IsInfected(pawn))
		{
			infectedPawns.Add(pawn);
		}
	}

	public override void PostSplitOff(Thing piece)
	{
		base.PostSplitOff(piece);
		CompMetalhorrorInfectible compMetalhorrorInfectible = piece.TryGetComp<CompMetalhorrorInfectible>();
		if (TryTakeInfector(out var infector))
		{
			compMetalhorrorInfectible.infectedPawns.Add(infector);
		}
	}

	public override void PreAbsorbStack(Thing otherStack, int count)
	{
		base.PreAbsorbStack(otherStack, count);
		CompMetalhorrorInfectible compMetalhorrorInfectible = otherStack.TryGetComp<CompMetalhorrorInfectible>();
		infectedPawns.AddRange(compMetalhorrorInfectible.infectedPawns);
	}

	public override void PostIngested(Pawn ingester)
	{
		if (TryTakeInfector(out var infector))
		{
			MetalhorrorUtility.Infect(ingester, infector, "FoodImplant");
		}
	}

	public override string CompInspectStringExtra()
	{
		if (!DebugSettings.godMode || infectedPawns.Empty())
		{
			return null;
		}
		return $"Infections: {infectedPawns.Count}";
	}

	private bool TryTakeInfector(out Pawn infector)
	{
		if (Infections <= 0)
		{
			infector = null;
			return false;
		}
		int index = Rand.Range(0, infectedPawns.Count);
		Pawn pawn = infectedPawns[index];
		infectedPawns.RemoveAt(index);
		infector = pawn;
		return true;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Collections.Look(ref infectedPawns, "infectedPawns", LookMode.Reference);
	}
}
