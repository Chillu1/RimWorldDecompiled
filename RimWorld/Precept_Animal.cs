namespace RimWorld
{
	public class Precept_Animal : Precept_ThingDef
	{
		public override string UIInfoFirstLine
		{
			get
			{
				if (base.ThingDef == null)
				{
					return base.UIInfoFirstLine;
				}
				return base.ThingDef.LabelCap;
			}
		}

		public override string TipLabel => def.issue.LabelCap + ": " + base.ThingDef.LabelCap;
	}
}
