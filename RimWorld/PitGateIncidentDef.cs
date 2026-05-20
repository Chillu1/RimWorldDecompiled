using System;
using Verse;

namespace RimWorld;

public class PitGateIncidentDef : Def
{
	public Type workerClass = typeof(PitGateIncidentWorker);

	public float baseChance;

	public IntRange durationRangeTicks;

	public bool usesThreatPoints;

	public int disableEnteringTicks;

	[MustTranslate]
	public string disableEnteringReason;

	[MustTranslate]
	public string letterText;

	[MustTranslate]
	public string letterLabel;
}
