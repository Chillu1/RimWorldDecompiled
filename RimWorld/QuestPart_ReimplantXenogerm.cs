using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class QuestPart_ReimplantXenogerm : QuestPart_MakeLord
{
	public IntVec3 gatherSpot;

	public string inSignalReimplanted;

	public int waitDurationTicks;

	protected override Lord MakeLord()
	{
		if (!ModLister.CheckBiotech("reimplanting reward"))
		{
			return null;
		}
		return LordMaker.MakeNewLord(faction, new LordJob_ReimplantXenogerm(gatherSpot, waitDurationTicks, inSignalReimplanted), base.Map);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref gatherSpot, "gatherSpot");
		Scribe_Values.Look(ref inSignalReimplanted, "inSignalReimplanted");
		Scribe_Values.Look(ref waitDurationTicks, "waitDurationTicks", 15000);
	}
}
