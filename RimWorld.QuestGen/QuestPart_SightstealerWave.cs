using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld.QuestGen;

public class QuestPart_SightstealerWave : QuestPart_MakeLord
{
	public string waveTag;

	public QuestPart_SightstealerWave()
	{
	}

	public QuestPart_SightstealerWave(string inSignal, MapParent mapParent, IEnumerable<Pawn> sightstealers, string waveTag)
	{
		base.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
		base.mapParent = mapParent;
		pawns.AddRange(sightstealers);
		this.waveTag = waveTag;
	}

	protected override Lord MakeLord()
	{
		Lord lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_SightstealerAssault(), mapParent.Map);
		QuestUtility.AddQuestTag(lord, waveTag);
		return lord;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref waveTag, "waveTag");
	}
}
