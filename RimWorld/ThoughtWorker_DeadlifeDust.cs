using Verse;

namespace RimWorld;

public class ThoughtWorker_DeadlifeDust : ThoughtWorker
{
	private const byte MinorDensity = 51;

	private const byte MediumDensity = 127;

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return ThoughtState.Inactive;
		}
		if (!p.Spawned)
		{
			return ThoughtState.Inactive;
		}
		byte b = p.Position.GasDensity(p.Map, GasType.DeadlifeDust);
		if (b == 0)
		{
			return ThoughtState.Inactive;
		}
		if (b <= 51)
		{
			return ThoughtState.ActiveAtStage(0);
		}
		if (b <= 127)
		{
			return ThoughtState.ActiveAtStage(1);
		}
		return ThoughtState.ActiveAtStage(2);
	}
}
