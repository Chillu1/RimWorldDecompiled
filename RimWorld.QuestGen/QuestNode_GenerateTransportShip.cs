using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GenerateTransportShip : QuestNode
	{
		public SlateRef<TransportShipDef> def;

		public SlateRef<Thing> shipThing;

		public SlateRef<IEnumerable<Thing>> contents;

		[NoTranslate]
		public SlateRef<string> storeAs;

		[NoTranslate]
		public SlateRef<string> inSignal;

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			TransportShip transportShip = TransportShipMaker.MakeTransportShip(def.GetValue(slate), null, shipThing.GetValue(slate));
			if (!storeAs.GetValue(slate).NullOrEmpty())
			{
				slate.Set(storeAs.GetValue(slate), transportShip);
			}
			QuestPart_SetupTransportShip part = new QuestPart_SetupTransportShip
			{
				inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal")),
				transportShip = transportShip,
				items = ((contents != (SlateRef<IEnumerable<Thing>>)(IEnumerable<Thing>)null) ? (from c in contents.GetValue(slate)
					where !(c is Pawn)
					select c).ToList() : null),
				pawns = ((contents != (SlateRef<IEnumerable<Thing>>)(IEnumerable<Thing>)null) ? contents.GetValue(slate).OfType<Pawn>().ToList() : null)
			};
			QuestGen.quest.AddPart(part);
		}

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}
	}
}
