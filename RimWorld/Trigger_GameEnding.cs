using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class Trigger_GameEnding : Trigger
{
	private int triggerAfterTicks;

	private bool requireActivePawns = true;

	private int gameEndingSinceTick = -1;

	public Trigger_GameEnding(int triggerAfterTicks = 2500)
	{
		this.triggerAfterTicks = triggerAfterTicks;
	}

	public override bool ActivateOn(Lord lord, TriggerSignal signal)
	{
		if (signal.type == TriggerSignalType.Tick)
		{
			if (requireActivePawns && !lord.AnyActivePawn)
			{
				return false;
			}
			if (Find.GameEnder.gameEnding)
			{
				if (gameEndingSinceTick == -1)
				{
					gameEndingSinceTick = Find.TickManager.TicksGame;
				}
			}
			else
			{
				gameEndingSinceTick = -1;
			}
			if (gameEndingSinceTick > 0 && Find.TickManager.TicksGame - gameEndingSinceTick > triggerAfterTicks)
			{
				return true;
			}
		}
		return false;
	}
}
