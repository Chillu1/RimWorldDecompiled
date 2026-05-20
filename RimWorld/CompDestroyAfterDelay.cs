using UnityEngine;
using Verse;

namespace RimWorld;

public class CompDestroyAfterDelay : ThingComp
{
	public int spawnTick;

	public int DestructionTick => spawnTick + Props.delayTicks;

	public int TicksLeft => DestructionTick - Find.TickManager.TicksGame;

	public CompProperties_DestroyAfterDelay Props => (CompProperties_DestroyAfterDelay)props;

	public override void CompTick()
	{
		base.CompTick();
		if (TicksLeft <= 0 && !parent.Destroyed)
		{
			parent.Destroy(Props.destroyMode);
		}
	}

	public override string CompInspectStringExtra()
	{
		if (Props.countdownLabel.NullOrEmpty())
		{
			return "";
		}
		int numTicks = Mathf.Max(0, spawnTick + Props.delayTicks - Find.TickManager.TicksGame);
		return Props.countdownLabel + ": " + numTicks.ToStringSecondsFromTicks();
	}

	public override string TransformLabel(string label)
	{
		if (Props.displayCountdownOnLabel)
		{
			return base.TransformLabel(label) + " (" + TicksLeft.TicksToSeconds().ToString("F0") + "s)";
		}
		return base.TransformLabel(label);
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		if (!respawningAfterLoad && !parent.BeingTransportedOnGravship)
		{
			spawnTick = Find.TickManager.TicksGame;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref spawnTick, "spawnTick", 0);
	}
}
