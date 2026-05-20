using System.Text;
using RimWorld;

namespace Verse.AI.Group;

public struct TriggerSignal
{
	public TriggerSignalType type;

	public string memo;

	public Signal signal;

	public Thing thing;

	public Thing otherThing;

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
		signal = default(Signal);
		thing = null;
		otherThing = null;
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

	public static TriggerSignal ForSignal(Signal signal)
	{
		TriggerSignal result = new TriggerSignal(TriggerSignalType.Signal);
		result.signal = signal;
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
			DamageInfo damageInfo = dinfo;
			stringBuilder.Append(", dinfo=" + damageInfo.ToString());
		}
		if (condition != PawnLostCondition.Undefined)
		{
			stringBuilder.Append(", condition=" + condition);
		}
		if (this.signal.tag != null)
		{
			Signal signal = this.signal;
			stringBuilder.Append(", signal=" + signal.ToString());
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}
}
