using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class QuestPart_EscortPawn : QuestPart_MakeLord
{
	public Pawn escortee;

	public Thing shuttle;

	public string questTag;

	public string leavingDangerMessage;

	protected override Lord MakeLord()
	{
		LordJob_EscortPawn lordJob_EscortPawn = new LordJob_EscortPawn(escortee, shuttle);
		lordJob_EscortPawn.leavingDangerMessage = leavingDangerMessage;
		Lord lord = LordMaker.MakeNewLord(faction, lordJob_EscortPawn, base.Map);
		QuestUtility.AddQuestTag(ref lord.questTags, questTag);
		return lord;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref escortee, "escortee");
		Scribe_References.Look(ref shuttle, "shuttle");
		Scribe_Values.Look(ref questTag, "questTag");
		Scribe_Values.Look(ref leavingDangerMessage, "leavingDangerMessage");
	}
}
