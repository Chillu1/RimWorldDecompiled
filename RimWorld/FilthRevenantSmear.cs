using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class FilthRevenantSmear : Filth
{
	private const int Radius = 4;

	public Pawn revenant;

	private void SendLetter(Pawn triggerer)
	{
		Find.LetterStack.ReceiveLetter("LetterLabelRevenantSmearDiscovered".Translate(), "LetterRevenantSmearDiscovered".Translate(triggerer.Named("PAWN")), LetterDefOf.ThreatSmall, this);
		revenant.TryGetComp<CompRevenant>().revenantSmearNotified = true;
	}

	protected override void Tick()
	{
		if (revenant?.mindState == null || revenant.TryGetComp<CompRevenant>().revenantSmearNotified || revenant.mindState.lastBecameVisibleTick > 0 || revenant.mindState.lastForcedVisibleTick > 0 || !this.IsHashIntervalTick(60))
		{
			return;
		}
		int num = GenRadial.NumCellsInRadius(4f);
		for (int i = 0; i < num; i++)
		{
			IntVec3 c = base.Position + GenRadial.RadialPattern[i];
			if (!c.InBounds(base.Map))
			{
				continue;
			}
			List<Thing> thingList = c.GetThingList(base.Map);
			for (int j = 0; j < thingList.Count; j++)
			{
				if (thingList[j] is Pawn { IsColonistPlayerControlled: not false } pawn)
				{
					SendLetter(pawn);
					return;
				}
			}
		}
	}
}
