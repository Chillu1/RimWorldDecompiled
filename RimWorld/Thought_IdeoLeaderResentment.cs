using Verse;

namespace RimWorld
{
	public class Thought_IdeoLeaderResentment : Thought_Situational
	{
		public Pawn Leader => pawn.Faction.leader;

		public override string LabelCap => "IdeoLeaderDifferentIdeoThoughtLabel".Translate(Leader.Ideo.memberName);

		public override string Description => base.CurStage.description.Formatted(Leader.Ideo.memberName);
	}
}
