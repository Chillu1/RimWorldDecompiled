using System.Text;
using Verse;

namespace RimWorld
{
	public class Building_Vent : Building_TempControl
	{
		private CompFlickable flickableComp;

		public override Graphic Graphic => flickableComp.CurrentGraphic;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			flickableComp = GetComp<CompFlickable>();
		}

		public override void TickRare()
		{
			if (FlickUtility.WantsToBeOn(this))
			{
				GenTemperature.EqualizeTemperaturesThroughBuilding(this, 14f, twoWay: true);
			}
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (!FlickUtility.WantsToBeOn(this))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("VentClosed".Translate());
			}
			return stringBuilder.ToString();
		}
	}
}
