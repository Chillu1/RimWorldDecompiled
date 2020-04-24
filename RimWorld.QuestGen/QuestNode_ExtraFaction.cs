using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_ExtraFaction : QuestNode
	{
		public SlateRef<Thing> factionOf;

		public SlateRef<Faction> faction;

		public SlateRef<IEnumerable<Pawn>> pawns;

		public SlateRef<ExtraFactionType> factionType;

		public SlateRef<bool> areHelpers;

		protected override void RunInt()
		{
			Faction value = faction.GetValue(QuestGen.slate);
			if (value == null)
			{
				Thing value2 = factionOf.GetValue(QuestGen.slate);
				if (value2 != null)
				{
					value = value2.Faction;
				}
				if (value == null)
				{
					return;
				}
			}
			QuestGen.quest.AddPart(new QuestPart_ExtraFaction
			{
				affectedPawns = pawns.GetValue(QuestGen.slate).ToList(),
				extraFaction = new ExtraFaction(value, factionType.GetValue(QuestGen.slate)),
				areHelpers = areHelpers.GetValue(QuestGen.slate)
			});
		}

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}
	}
}
