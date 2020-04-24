using RimWorld;

namespace Verse.AI.Group
{
	public class Trigger_ImportantTraderCaravanPeopleLost : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.PawnLost && (signal.condition == PawnLostCondition.IncappedOrKilled || signal.condition == PawnLostCondition.MadePrisoner))
			{
				if (signal.Pawn.GetTraderCaravanRole() == TraderCaravanRole.Trader || signal.Pawn.RaceProps.packAnimal)
				{
					return true;
				}
				if (lord.numPawnsLostViolently > 0 && (float)lord.numPawnsLostViolently / (float)lord.numPawnsEverGained >= 0.5f)
				{
					return true;
				}
			}
			return false;
		}
	}
}
