using System.Text;
using Verse;

namespace RimWorld
{
	public class Building_BurningPowerCell : Building
	{
		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("PuncturedWillExplode".Translate());
			stringBuilder.AppendInNewLine(base.GetInspectString());
			return stringBuilder.ToString();
		}
	}
}
