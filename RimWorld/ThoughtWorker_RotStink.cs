using Verse;

namespace RimWorld;

public class ThoughtWorker_RotStink : ThoughtWorker
{
	private const byte MinorDensity = 127;

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!p.Spawned)
		{
			return ThoughtState.Inactive;
		}
		byte b = p.Position.GasDensity(p.Map, GasType.RotStink);
		if (b == 0)
		{
			return ThoughtState.Inactive;
		}
		if (!GasUtility.IsAffectedByExposure(p))
		{
			return ThoughtState.Inactive;
		}
		if (b <= 127)
		{
			return ThoughtState.ActiveAtStage(0);
		}
		return ThoughtState.ActiveAtStage(1);
	}
}
