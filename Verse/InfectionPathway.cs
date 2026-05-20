using RimWorld;
using UnityEngine;

namespace Verse;

public class InfectionPathway : IExposable
{
	private InfectionPathwayDef def;

	private int ageTicks;

	private PawnKindDef sourcePawnKind;

	private Pawn ownerPawn;

	public InfectionPathwayDef Def => def;

	public bool SourceWasPawn => sourcePawnKind != null;

	public PawnKindDef SourcePawnKind => sourcePawnKind;

	public Pawn OwnerPawn => ownerPawn;

	public int AgeTicks => ageTicks;

	public bool Expired => ageTicks > def.ExpiryTicks;

	public float AgePercent => Mathf.Clamp01((float)ageTicks / (float)def.ExpiryTicks);

	public InfectionPathway()
	{
	}

	public InfectionPathway(InfectionPathwayDef def, Pawn ownerPawn, PawnKindDef sourcePawnKind = null)
	{
		this.def = def;
		this.sourcePawnKind = sourcePawnKind;
		this.ownerPawn = ownerPawn;
	}

	public string GetExplanation(HediffDef hediff)
	{
		foreach (HediffInfectionPathway possiblePathway in hediff.possiblePathways)
		{
			if (possiblePathway.PathwayDef == def)
			{
				if (sourcePawnKind != null)
				{
					return possiblePathway.Explanation.Formatted(NamedArgumentUtility.Named(sourcePawnKind, "SOURCEKIND"));
				}
				return possiblePathway.Explanation.Formatted();
			}
		}
		Log.ErrorOnce("Failed to get an explanation for infection vector " + def.defName + " for hediff def " + hediff.defName, 85784634);
		return null;
	}

	public void TickInterval(int delta)
	{
		ageTicks += delta;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref ageTicks, "ageTicks", 0);
		Scribe_Defs.Look(ref def, "def");
		Scribe_Defs.Look(ref sourcePawnKind, "sourcePawnKind");
		Scribe_References.Look(ref ownerPawn, "ownerPawn");
	}
}
