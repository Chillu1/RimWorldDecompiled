using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GenerateShuttle : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<IEnumerable<Pawn>> requiredPawns;

		public SlateRef<IEnumerable<ThingDefCount>> requiredItems;

		public SlateRef<int> requireColonistCount;

		public SlateRef<bool> acceptColonists;

		public SlateRef<bool> onlyAcceptColonists;

		public SlateRef<bool> onlyAcceptHealthy;

		public SlateRef<bool> leaveImmediatelyWhenSatisfied;

		public SlateRef<bool> dropEverythingIfUnsatisfied;

		public SlateRef<bool> dropEverythingOnArrival;

		public SlateRef<Faction> owningFaction;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			Thing thing = ThingMaker.MakeThing(ThingDefOf.Shuttle);
			if (owningFaction.GetValue(slate) != null)
			{
				thing.SetFaction(owningFaction.GetValue(slate));
			}
			CompShuttle compShuttle = thing.TryGetComp<CompShuttle>();
			if (requiredPawns.GetValue(slate) != null)
			{
				compShuttle.requiredPawns.AddRange(requiredPawns.GetValue(slate));
			}
			if (requiredItems.GetValue(slate) != null)
			{
				compShuttle.requiredItems.AddRange(requiredItems.GetValue(slate));
			}
			compShuttle.acceptColonists = acceptColonists.GetValue(slate);
			compShuttle.onlyAcceptColonists = onlyAcceptColonists.GetValue(slate);
			compShuttle.onlyAcceptHealthy = onlyAcceptHealthy.GetValue(slate);
			compShuttle.requiredColonistCount = requireColonistCount.GetValue(slate);
			compShuttle.dropEverythingIfUnsatisfied = dropEverythingIfUnsatisfied.GetValue(slate);
			compShuttle.leaveImmediatelyWhenSatisfied = leaveImmediatelyWhenSatisfied.GetValue(slate);
			compShuttle.dropEverythingOnArrival = dropEverythingOnArrival.GetValue(slate);
			QuestGen.slate.Set(storeAs.GetValue(slate), thing);
		}
	}
}
