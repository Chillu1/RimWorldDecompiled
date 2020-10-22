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

		public SlateRef<bool> permitShuttle;

		public SlateRef<bool> hideControls;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Shuttle is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 8811221);
				return;
			}
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
			compShuttle.permitShuttle = permitShuttle.GetValue(slate);
			compShuttle.hideControls = hideControls.GetValue(slate);
			QuestGen.slate.Set(storeAs.GetValue(slate), thing);
		}
	}
}
