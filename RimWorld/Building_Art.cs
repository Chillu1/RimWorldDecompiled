using Verse;

namespace RimWorld;

public class Building_Art : Building
{
	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (!string.IsNullOrEmpty(text))
		{
			text += "\n";
		}
		return text + $"{StatDefOf.Beauty.LabelCap}: {StatDefOf.Beauty.ValueToString(this.GetStatValue(StatDefOf.Beauty))}";
	}
}
