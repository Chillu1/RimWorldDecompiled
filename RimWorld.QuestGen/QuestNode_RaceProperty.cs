using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen
{
	public abstract class QuestNode_RaceProperty : QuestNode
	{
		[NoTranslate]
		public SlateRef<object> value;

		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			if (Matches(value.GetValue(slate)))
			{
				if (node != null)
				{
					return node.TestRun(slate);
				}
				return true;
			}
			if (elseNode != null)
			{
				return elseNode.TestRun(slate);
			}
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			if (Matches(value.GetValue(slate)))
			{
				if (node != null)
				{
					node.Run();
				}
			}
			else if (elseNode != null)
			{
				elseNode.Run();
			}
		}

		private bool Matches(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is PawnKindDef)
			{
				return Matches(((PawnKindDef)obj).RaceProps);
			}
			if (obj is ThingDef)
			{
				if (((ThingDef)obj).race != null)
				{
					return Matches(((ThingDef)obj).race);
				}
				return false;
			}
			if (obj is Pawn)
			{
				return Matches(((Pawn)obj).RaceProps);
			}
			if (obj is Faction)
			{
				if (((Faction)obj).def.basicMemberKind != null)
				{
					return Matches(((Faction)obj).def.basicMemberKind);
				}
				return false;
			}
			if (obj is IEnumerable<Pawn>)
			{
				if (((IEnumerable<Pawn>)obj).Any())
				{
					return ((IEnumerable<Pawn>)obj).All((Pawn x) => Matches(x.RaceProps));
				}
				return false;
			}
			if (obj is IEnumerable<Thing>)
			{
				if (((IEnumerable<Thing>)obj).Any())
				{
					return ((IEnumerable<Thing>)obj).All((Thing x) => x is Pawn && Matches(((Pawn)x).RaceProps));
				}
				return false;
			}
			if (obj is IEnumerable<object>)
			{
				if (((IEnumerable<object>)obj).Any())
				{
					return ((IEnumerable<object>)obj).All((object x) => x is Pawn && Matches(((Pawn)x).RaceProps));
				}
				return false;
			}
			if (obj is string && !((string)obj).NullOrEmpty())
			{
				return Matches(DefDatabase<PawnKindDef>.GetNamed((string)obj).RaceProps);
			}
			return false;
		}

		protected abstract bool Matches(RaceProperties raceProperties);
	}
}
