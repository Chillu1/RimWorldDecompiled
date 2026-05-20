using System;
using Verse;

namespace RimWorld;

public static class PawnGenerationContextUtility
{
	public static string ToStringHuman(this PawnGenerationContext context)
	{
		return context switch
		{
			PawnGenerationContext.All => "PawnGenerationContext_All".Translate(), 
			PawnGenerationContext.PlayerStarter => "PawnGenerationContext_PlayerStarter".Translate(), 
			PawnGenerationContext.NonPlayer => "PawnGenerationContext_NonPlayer".Translate(), 
			_ => throw new NotImplementedException(), 
		};
	}

	public static bool Includes(this PawnGenerationContext includer, PawnGenerationContext other)
	{
		if (includer == PawnGenerationContext.All)
		{
			return true;
		}
		return includer == other;
	}

	public static PawnGenerationContext GetRandom()
	{
		Array values = Enum.GetValues(typeof(PawnGenerationContext));
		return (PawnGenerationContext)values.GetValue(Rand.Range(0, values.Length));
	}

	public static bool OverlapsWith(this PawnGenerationContext a, PawnGenerationContext b)
	{
		if (a == PawnGenerationContext.All || b == PawnGenerationContext.All)
		{
			return true;
		}
		if (a == b)
		{
			return true;
		}
		return false;
	}
}
