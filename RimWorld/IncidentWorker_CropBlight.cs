using System.Linq;
using Verse;

namespace RimWorld;

public class IncidentWorker_CropBlight : IncidentWorker
{
	private const float Radius = 11f;

	private static readonly SimpleCurve BlightChancePerRadius = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(8f, 1f),
		new CurvePoint(11f, 0.3f)
	};

	private static readonly SimpleCurve RadiusFactorPerPointsCurve = new SimpleCurve
	{
		new CurvePoint(100f, 0.6f),
		new CurvePoint(500f, 1f),
		new CurvePoint(2000f, 2f)
	};

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		Plant plant;
		return TryFindRandomBlightablePlant((Map)parms.target, out plant);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		float num = RadiusFactorPerPointsCurve.Evaluate(parms.points);
		if (!TryFindRandomBlightablePlant(map, out var plant))
		{
			return false;
		}
		Room room = plant.GetRoom();
		int i = 0;
		for (int num2 = GenRadial.NumCellsInRadius(11f * num); i < num2; i++)
		{
			IntVec3 intVec = plant.Position + GenRadial.RadialPattern[i];
			if (intVec.InBounds(map) && intVec.GetRoom(map) == room)
			{
				Plant firstBlightableNowPlant = BlightUtility.GetFirstBlightableNowPlant(intVec, map);
				if (firstBlightableNowPlant != null && firstBlightableNowPlant.def == plant.def && Rand.Chance(BlightChance(firstBlightableNowPlant.Position, plant.Position, num)))
				{
					firstBlightableNowPlant.CropBlighted();
				}
			}
		}
		SendStandardLetter("LetterLabelCropBlight".Translate(new NamedArgument(plant.def, "PLANTDEF")), "LetterCropBlight".Translate(new NamedArgument(plant.def, "PLANTDEF")), LetterDefOf.NegativeEvent, parms, new TargetInfo(plant.Position, map));
		return true;
	}

	private bool TryFindRandomBlightablePlant(Map map, out Plant plant)
	{
		Thing result2;
		bool result = (from x in map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
			where ((Plant)x).BlightableNow
			select x).TryRandomElement(out result2);
		plant = (Plant)result2;
		return result;
	}

	private float BlightChance(IntVec3 c, IntVec3 root, float radiusFactor)
	{
		float x = c.DistanceTo(root) / radiusFactor;
		return BlightChancePerRadius.Evaluate(x);
	}
}
