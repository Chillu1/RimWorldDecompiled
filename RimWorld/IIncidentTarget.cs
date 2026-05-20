using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public interface IIncidentTarget : ILoadReferenceable
{
	PlanetTile Tile { get; }

	StoryState StoryState { get; }

	GameConditionManager GameConditionManager { get; }

	float PlayerWealthForStoryteller { get; }

	IEnumerable<Pawn> PlayerPawnsForStoryteller { get; }

	FloatRange IncidentPointsRandomFactorRange { get; }

	int ConstantRandSeed { get; }

	IEnumerable<IncidentTargetTagDef> IncidentTargetTags();
}
