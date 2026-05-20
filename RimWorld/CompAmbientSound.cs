using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompAmbientSound : ThingComp
{
	private Sustainer sustainer;

	private CompProperties_AmbientSound Props => (CompProperties_AmbientSound)props;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		LongEventHandler.ExecuteWhenFinished(StartSustanier);
	}

	public override void CompTick()
	{
		sustainer?.Maintain();
	}

	public override void ReceiveCompSignal(string signal)
	{
		if (Props.disableOnHacked && signal == "Hacked")
		{
			EndSustainer();
		}
		if (Props.disabledOnUnpowered && signal == "PowerTurnedOff")
		{
			EndSustainer();
		}
		if (Props.disabledOnUnpowered && signal == "PowerTurnedOn")
		{
			StartSustanier();
		}
		if (Props.disableOnInteracted && signal == "Interacted")
		{
			EndSustainer();
		}
	}

	public void StartSustanier()
	{
		if (sustainer == null)
		{
			sustainer = Props.sound.TrySpawnSustainer(SoundInfo.InMap(parent));
		}
	}

	public void EndSustainer()
	{
		sustainer?.End();
		sustainer = null;
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		base.PostDeSpawn(map, mode);
		sustainer?.End();
		sustainer = null;
	}
}
