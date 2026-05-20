using System;
using Verse;

namespace RimWorld;

public abstract class RitualPosition : IExposable
{
	public bool highlight;

	public abstract PawnStagePosition GetCell(IntVec3 spot, Pawn p, LordJob_Ritual ritual);

	public virtual bool CanUse(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
	{
		return true;
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref highlight, "highlight", defaultValue: false);
	}

	protected virtual IntVec3 GetFallbackSpot(CellRect rect, IntVec3 spot, Pawn p, LordJob_Ritual ritual, Func<IntVec3, bool> Validator)
	{
		foreach (IntVec3 adjacentCell in rect.AdjacentCells)
		{
			if (Validator(adjacentCell))
			{
				return adjacentCell;
			}
		}
		CellFinder.TryFindRandomCellNear(spot, p.Map, 3, Validator.Invoke, out var result);
		return result;
	}
}
