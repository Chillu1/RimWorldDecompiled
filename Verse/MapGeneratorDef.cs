using System;
using System.Collections.Generic;

namespace Verse;

public class MapGeneratorDef : Def
{
	public bool isUnderground;

	public bool forceCaves;

	public List<GenStepDef> genSteps;

	public PocketMapProperties pocketMapProperties;

	public List<Type> customMapComponents = new List<Type>();

	public bool ignoreAreaRevealedLetter;

	public RoofDef roofDef;

	public bool disableShadows;

	public bool disableCallAid;

	public bool disableMapClippers;

	public bool renderWorld;

	public bool defeatRequiresCantReachUnfogged = true;

	public TerrainDef defaultUnderGridTerrain;

	public bool validScenarioMap;

	[NoTranslate]
	public string mapClipperTexturePath;

	public ShaderTypeDef mapClipperShader;
}
