using UnityEngine;

namespace Verse;

public class Hediff_RapidRegeneration : Hediff
{
	private float hpRemaining = 100f;

	private float hpCapacity = 100f;

	public override string SeverityLabel => string.Format("{0:0} / {1:0}{2}", hpRemaining, hpCapacity, "HP".Translate());

	public override bool ShouldRemove => hpRemaining <= 0f;

	public void SetHpCapacity(float amount)
	{
		hpRemaining = (hpCapacity = amount);
	}

	public override void Notify_Regenerated(float hp)
	{
		hpRemaining = Mathf.Max(hpRemaining - hp, 0f);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref hpRemaining, "hpRemaining", 0f);
		Scribe_Values.Look(ref hpCapacity, "hpCapacity", 0f);
	}
}
