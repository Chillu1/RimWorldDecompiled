using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class FocusStrengthOffset_ArtificialBuildings : FocusStrengthOffset_Curve
	{
		public float radius = 10f;

		protected override string ExplanationKey => "StatsReport_NearbyArtificialStructures";

		protected override float SourceValue(Thing parent)
		{
			return parent.Spawned ? parent.Map.listerArtificialBuildingsForMeditation.GetForCell(parent.Position, radius).Count : 0;
		}

		public override void PostDrawExtraSelectionOverlays(Thing parent, Pawn user = null)
		{
			base.PostDrawExtraSelectionOverlays(parent, user);
			MeditationUtility.DrawArtificialBuildingOverlay(parent.Position, parent.def, parent.Map, radius);
		}

		public override string InspectStringExtra(Thing parent, Pawn user = null)
		{
			if (parent.Spawned)
			{
				List<Thing> forCell = parent.Map.listerArtificialBuildingsForMeditation.GetForCell(parent.Position, radius);
				if (forCell.Count > 0)
				{
					TaggedString taggedString = "MeditationFocusImpacted".Translate() + ": " + (from c in forCell.Take(3)
						select c.LabelShort).ToCommaList().CapitalizeFirst();
					if (forCell.Count > 3)
					{
						taggedString += " " + "Etc".Translate();
					}
					return taggedString;
				}
			}
			return "";
		}
	}
}
