using System;
using Verse;

namespace RimWorld;

public class IntermittentSteamSprayer
{
	private readonly Thing parent;

	private int ticksUntilSpray;

	private int sprayTicksLeft;

	public Action startSprayCallback;

	public Action endSprayCallback;

	public int MinTicksBetweenSprays { get; set; } = 500;

	public int MaxTicksBetweenSprays { get; set; } = 2000;

	public int MinSprayDuration { get; set; } = 200;

	public int MaxSprayDuration { get; set; } = 500;

	public int PushHeatInterval { get; set; } = 20;

	public float SprayThickness { get; set; } = 0.6f;

	public IntermittentSteamSprayer(Thing parent)
	{
		this.parent = parent;
		ticksUntilSpray = MinTicksBetweenSprays;
	}

	public void SteamSprayerTick()
	{
		if (sprayTicksLeft > 0)
		{
			sprayTicksLeft--;
			if (Rand.Value < SprayThickness)
			{
				FleckMaker.ThrowAirPuffUp(parent.TrueCenter(), parent.Map);
			}
			if (parent.IsHashIntervalTick(PushHeatInterval))
			{
				GenTemperature.PushHeat(parent, 40f);
			}
			if (sprayTicksLeft <= 0)
			{
				if (endSprayCallback != null)
				{
					endSprayCallback();
				}
				ticksUntilSpray = Rand.RangeInclusive(MinTicksBetweenSprays, MaxTicksBetweenSprays);
			}
			return;
		}
		ticksUntilSpray--;
		if (ticksUntilSpray <= 0)
		{
			if (startSprayCallback != null)
			{
				startSprayCallback();
			}
			sprayTicksLeft = Rand.RangeInclusive(MinSprayDuration, MaxSprayDuration);
		}
	}
}
