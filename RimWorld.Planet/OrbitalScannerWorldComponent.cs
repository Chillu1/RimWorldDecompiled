using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class OrbitalScannerWorldComponent : WorldComponent
{
	private int lastFoundSignal = -1;

	private List<CompOrbitalScanner> workingScanners = new List<CompOrbitalScanner>();

	private const int FindSignalMTBTicks = 60000;

	private const int FindSignalCooldownTicks = 1080000;

	private bool OnCooldown
	{
		get
		{
			if (lastFoundSignal > 0)
			{
				return Find.TickManager.TicksGame < lastFoundSignal + 1080000;
			}
			return false;
		}
	}

	public OrbitalScannerWorldComponent(World world)
		: base(world)
	{
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref lastFoundSignal, "lastFoundSignal", 0);
	}

	public override void WorldComponentTick()
	{
		if (!OnCooldown && !workingScanners.Empty())
		{
			if (Rand.MTBEventOccurs(60000f, 1f, 1f))
			{
				workingScanners.RandomElement().ReceiveSignal();
				lastFoundSignal = Find.TickManager.TicksGame;
			}
			workingScanners.Clear();
		}
	}

	public void Notify_ScannerWorking(CompOrbitalScanner scanner)
	{
		if (!OnCooldown)
		{
			workingScanners.Add(scanner);
		}
	}
}
