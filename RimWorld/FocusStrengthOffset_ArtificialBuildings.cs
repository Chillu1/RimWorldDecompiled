using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class FocusStrengthOffset_ArtificialBuildings : FocusStrengthOffset_Curve
{
	public float radius = 10f;

	protected override string ExplanationKey => "StatsReport_NearbyArtificialStructures";

	public override bool CanApply(Thing parent, Pawn user = null)
	{
		return parent.Spawned;
	}

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
				IEnumerable<string> source = forCell.Select((Thing c) => GenLabel.ThingLabel(c, 1, includeHp: false)).Distinct();
				TaggedString taggedString = "MeditationFocusImpacted".Translate() + ": " + source.Take(3).ToCommaList().CapitalizeFirst();
				if (source.Count() > 3)
				{
					taggedString += " " + "Etc".Translate();
				}
				return taggedString;
			}
		}
		return "";
	}
}
