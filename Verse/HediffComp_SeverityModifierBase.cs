using System.Text;

namespace Verse;

public abstract class HediffComp_SeverityModifierBase : HediffComp
{
	protected const int SeverityUpdateInterval = 200;

	public abstract float SeverityChangePerDay();

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (base.Pawn.IsHashIntervalTick(200, delta))
		{
			float num = SeverityChangePerDay();
			num *= 0.0033333334f;
			severityAdjustment += num;
		}
	}

	public override string CompDebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.CompDebugString());
		if (!base.Pawn.Dead)
		{
			stringBuilder.AppendLine("severity/day: " + SeverityChangePerDay().ToString("F3"));
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}
}
