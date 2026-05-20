using System.Linq;
using Verse;

namespace RimWorld.Planet;

public class SitePartWorker_InsectLair : SitePartWorker
{
	public override void PostMapGenerate(Map map)
	{
		Thing thing = map.listerThings.ThingsOfDef(ThingDefOf.InsectLairEntrance).FirstOrDefault();
		if (thing != null)
		{
			Find.LetterStack.ReceiveLetter("CaveEntranceLetter".Translate(), "CaveEntranceLetterText".Translate(), LetterDefOf.NeutralEvent, thing);
		}
	}
}
