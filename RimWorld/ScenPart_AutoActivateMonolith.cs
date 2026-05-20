using Verse;

namespace RimWorld;

public class ScenPart_AutoActivateMonolith : ScenPart
{
	public int delayTicks;

	private string delayTicksBuf;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref delayTicks, "delayTicks", 0);
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		float val = (float)delayTicks / 60000f;
		Widgets.TextFieldNumericLabeled(listing.GetScenPartRect(this, ScenPart.RowHeight), "delayDays".Translate().CapitalizeFirst(), ref val, ref delayTicksBuf);
		delayTicks = (int)(val * 60000f);
	}

	public override void PostGameStart()
	{
		if (ModsConfig.AnomalyActive && Find.Anomaly.GenerateMonolith && Find.Anomaly.monolith != null)
		{
			Find.Anomaly.monolith.AutoActivate(Find.TickManager.TicksGame + delayTicks);
		}
	}
}
