using RimWorld;
using System.Text;

namespace Verse.AI.Group
{
	public struct TriggerSignal
	{
		public TriggerSignalType type;

		public string memo;

		public Thing thing;

		public DamageInfo dinfo;

		public PawnLostCondition condition;

		public Faction faction;

		public FactionRelationKind? previousRelationKind;

		public ClamorDef clamorType;

		public Pawn Pawn => thing as Pawn;

		public static TriggerSignal ForTick => new TriggerSignal(TriggerSignalType.Tick);

		public TriggerSignal(TriggerSignalType type)
		{
			this.type = type;
			memo = null;
			thing = null;
			dinfo = default(DamageInfo);
			condition = PawnLostCondition.Undefined;
			faction = null;
			clamorType = null;
			previousRelationKind = null;
		}

		public static TriggerSignal ForMemo(string memo)
		{
			TriggerSignal result = new TriggerSignal(TriggerSignalType.Memo);
			result.memo = memo;
			return result;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("(");
			stringBuilder.Append(type.ToString());
			if (memo != null)
			{
				stringBuilder.Append(", memo=" + memo);
			}
			if (Pawn != null)
			{
				stringBuilder.Append(", pawn=" + Pawn);
			}
			if (dinfo.Def != null)
			{
				stringBuilder.Append(", dinfo=" + dinfo);
			}
			if (condition != 0)
			{
				stringBuilder.Append(", condition=" + condition);
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}
	}
}
