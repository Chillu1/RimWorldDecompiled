using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class QuestPart_AssaultThings : QuestPart_MakeLord
{
	public List<Thing> things = new List<Thing>();

	protected override Lord MakeLord()
	{
		return LordMaker.MakeNewLord(faction, new LordJob_AssaultThings(faction, things), base.Map);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref things, "things", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			things.RemoveAll((Thing x) => x == null);
		}
	}
}
