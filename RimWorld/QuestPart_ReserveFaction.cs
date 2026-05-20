using Verse;

namespace RimWorld;

public class QuestPart_ReserveFaction : QuestPart
{
	public Faction faction;

	public override bool QuestPartReserves(Faction f)
	{
		if (f != null)
		{
			return f == faction;
		}
		return false;
	}

	public override void Notify_FactionRemoved(Faction f)
	{
		if (faction == f)
		{
			faction = null;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref faction, "faction");
	}
}
