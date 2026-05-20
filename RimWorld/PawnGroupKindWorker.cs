using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class PawnGroupKindWorker
{
	public PawnGroupKindDef def;

	public static List<List<Pawn>> pawnsBeingGeneratedNow = new List<List<Pawn>>();

	public abstract float MinPointsToGenerateAnything(PawnGroupMaker groupMaker, FactionDef faction, PawnGroupMakerParms parms = null);

	public List<Pawn> GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, bool errorOnZeroResults = true)
	{
		List<Pawn> list = new List<Pawn>();
		pawnsBeingGeneratedNow.Add(list);
		try
		{
			GeneratePawns(parms, groupMaker, list, errorOnZeroResults);
		}
		catch (Exception ex)
		{
			Log.Error("Exception while generating pawn group: " + ex);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Destroy();
			}
			list.Clear();
		}
		finally
		{
			pawnsBeingGeneratedNow.Remove(list);
		}
		return list;
	}

	protected abstract void GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns, bool errorOnZeroResults = true);

	public virtual bool CanGenerateFrom(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
	{
		return true;
	}

	public abstract IEnumerable<PawnKindDef> GeneratePawnKindsExample(PawnGroupMakerParms parms, PawnGroupMaker groupMaker);
}
