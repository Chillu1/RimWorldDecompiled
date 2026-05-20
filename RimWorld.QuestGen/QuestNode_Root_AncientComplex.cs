using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public abstract class QuestNode_Root_AncientComplex : QuestNode
{
	private static List<LandmarkDef> allowedLandmarksCached;

	protected static readonly SimpleCurve ThreatPointsOverPointsCurve = new SimpleCurve
	{
		new CurvePoint(35f, 38.5f),
		new CurvePoint(400f, 165f),
		new CurvePoint(10000f, 4125f)
	};

	protected virtual SimpleCurve ComplexSizeOverPointsCurve => new SimpleCurve
	{
		new CurvePoint(0f, 30f),
		new CurvePoint(10000f, 50f)
	};

	protected virtual SimpleCurve TerminalsOverRoomCountCurve => new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(10f, 4f),
		new CurvePoint(20f, 6f),
		new CurvePoint(50f, 10f)
	};

	protected virtual LayoutDef LayoutDef => LayoutDefOf.AncientComplex;

	protected virtual SitePartDef SitePartDef => SitePartDefOf.AncientComplex;

	protected static List<LandmarkDef> AllowedLandmarks
	{
		get
		{
			if (ModsConfig.OdysseyActive && allowedLandmarksCached == null)
			{
				allowedLandmarksCached = new List<LandmarkDef>
				{
					LandmarkDefOf.Oasis,
					LandmarkDefOf.Lake,
					LandmarkDefOf.LakeWithIsland,
					LandmarkDefOf.LakeWithIslands,
					LandmarkDefOf.Pond,
					LandmarkDefOf.DryLake,
					LandmarkDefOf.ToxicLake,
					LandmarkDefOf.Wetland,
					LandmarkDefOf.HotSprings,
					LandmarkDefOf.CoastalIsland,
					LandmarkDefOf.Peninsula,
					LandmarkDefOf.Valley,
					LandmarkDefOf.Cavern,
					LandmarkDefOf.Chasm,
					LandmarkDefOf.IceDunes,
					LandmarkDefOf.Cliffs,
					LandmarkDefOf.Hollow,
					LandmarkDefOf.TerraformingScar,
					LandmarkDefOf.LavaFlow,
					LandmarkDefOf.Dunes,
					LandmarkDefOf.AncientSmokeVent,
					LandmarkDefOf.AncientToxVent
				};
			}
			return allowedLandmarksCached;
		}
	}

	public abstract LayoutStructureSketch QuestSetupComplex(Quest quest, float points);

	protected virtual LayoutStructureSketch GenerateStructureSketch(float points, bool generateTerminals = true)
	{
		int num = (int)ComplexSizeOverPointsCurve.Evaluate(points);
		StructureGenParams parms = new StructureGenParams
		{
			size = new IntVec2(num, num)
		};
		LayoutStructureSketch layoutStructureSketch = LayoutDef.Worker.GenerateStructureSketch(parms);
		if (generateTerminals)
		{
			int num2 = Mathf.FloorToInt(TerminalsOverRoomCountCurve.Evaluate(layoutStructureSketch.structureLayout.Rooms.Count));
			for (int i = 0; i < num2; i++)
			{
				layoutStructureSketch.thingsToSpawn.Add(ThingMaker.MakeThing(ThingDefOf.AncientTerminal));
			}
		}
		return layoutStructureSketch;
	}
}
