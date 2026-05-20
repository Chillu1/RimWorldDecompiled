using System.Collections.Generic;

namespace Verse;

public class DeathActionProperties_Divide : DeathActionProperties
{
	public List<PawnKindDef> dividePawnKindOptions = new List<PawnKindDef>();

	public int dividePawnCount;

	public List<PawnKindDef> dividePawnKindAdditionalForced = new List<PawnKindDef>();

	public IntRange divideBloodFilthCountRange;

	public DeathActionProperties_Divide()
	{
		workerClass = typeof(DeathActionWorker_Divide);
	}

	public override IEnumerable<string> ConfigErrors()
	{
		if (dividePawnCount <= 0 && dividePawnKindAdditionalForced.NullOrEmpty())
		{
			yield return "deathActionWorkerClass is DeathActionWorker_Divide or subclass, but dividePawnCount <= 0.";
		}
		if (dividePawnKindOptions.NullOrEmpty() && dividePawnKindAdditionalForced.NullOrEmpty())
		{
			yield return "deathActionWorkerClass is DeathActionWorker_Divide or subclass, but dividePawnKindOptions is null or empty.";
		}
	}
}
