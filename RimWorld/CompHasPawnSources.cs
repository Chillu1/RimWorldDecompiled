using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompHasPawnSources : ThingComp
{
	public List<Pawn> pawnSources = new List<Pawn>();

	public CompProperties_HasPawnSources Props => (CompProperties_HasPawnSources)props;

	public void AddSource(Pawn pawn)
	{
		if (pawn != null && !pawnSources.Contains(pawn))
		{
			Find.WorldPawns.AddPawnSource(pawn, this);
			pawnSources.Add(pawn);
		}
	}

	public override bool AllowStackWith(Thing other)
	{
		CompHasPawnSources compHasPawnSources = other.TryGetComp<CompHasPawnSources>();
		if (compHasPawnSources == null || compHasPawnSources.pawnSources.Count != pawnSources.Count)
		{
			return false;
		}
		foreach (Pawn pawnSource in pawnSources)
		{
			if (!compHasPawnSources.pawnSources.Contains(pawnSource))
			{
				return false;
			}
		}
		return base.AllowStackWith(other);
	}

	public override string TransformLabel(string label)
	{
		if (!Props.affectLabel || pawnSources.NullOrEmpty() || pawnSources.Count > 2)
		{
			return label;
		}
		if (pawnSources.Count == 2)
		{
			return "ThingOfTwoSources".Translate(label, pawnSources[0].LabelShortCap, pawnSources[1].LabelShortCap);
		}
		return "ThingOfSource".Translate(label, pawnSources[0].LabelShortCap);
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		base.PostDestroy(mode, previousMap);
		Find.WorldPawns.RemovePawnSources(pawnSources, this);
	}

	public override void PostSplitOff(Thing piece)
	{
		CompHasPawnSources compHasPawnSources = piece.TryGetComp<CompHasPawnSources>();
		if (compHasPawnSources == null || piece == parent)
		{
			return;
		}
		foreach (Pawn pawnSource in pawnSources)
		{
			compHasPawnSources.AddSource(pawnSource);
		}
	}

	public override void PostExposeData()
	{
		Scribe_Collections.Look(ref pawnSources, "pawnSources", true, LookMode.Reference);
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		pawnSources.RemoveAll((Pawn x) => x == null);
		foreach (Pawn pawnSource in pawnSources)
		{
			Find.WorldPawns.AddPawnSource(pawnSource, this);
		}
	}
}
