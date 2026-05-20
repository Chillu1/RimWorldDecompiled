using Verse;

namespace RimWorld;

public class CompOxygenPusher : ThingComp
{
	private const float IntervalToPerSecond = 4.1666665f;

	private CompPowerTrader intPowerTrader;

	private CompProperties_OxygenPusher Props => (CompProperties_OxygenPusher)props;

	public CompPowerTrader PowerTrader => intPowerTrader ?? (intPowerTrader = parent.GetComp<CompPowerTrader>());

	public override void CompTickRare()
	{
		if (!Props.requiresPower || !PowerTrader.Off)
		{
			Room room = parent.GetRoom();
			float num = 100f / (float)room.CellCount * Props.airPerSecondPerHundredCells * 4.1666665f;
			room.Vacuum = room.UnsanitizedVacuum - num;
		}
	}
}
