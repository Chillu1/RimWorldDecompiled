namespace Verse;

public class PawnKindLifeStage
{
	[MustTranslate]
	public string label;

	[MustTranslate]
	public string labelPlural;

	[MustTranslate]
	public string labelMale;

	[MustTranslate]
	public string labelMalePlural;

	[MustTranslate]
	public string labelFemale;

	[MustTranslate]
	public string labelFemalePlural;

	[Unsaved(false)]
	[TranslationHandle(Priority = 200)]
	public string untranslatedLabel;

	[Unsaved(false)]
	[TranslationHandle(Priority = 100)]
	public string untranslatedLabelMale;

	[Unsaved(false)]
	[TranslationHandle]
	public string untranslatedLabelFemale;

	public GraphicData bodyGraphicData;

	public GraphicData femaleGraphicData;

	public GraphicData dessicatedBodyGraphicData;

	public GraphicData femaleDessicatedBodyGraphicData;

	public GraphicData corpseGraphicData;

	public GraphicData swimmingGraphicData;

	public GraphicData femaleSwimmingGraphicData;

	public GraphicData femaleCorpseGraphicData;

	public GraphicData silhouetteGraphicData;

	public GraphicData rottingGraphicData;

	public GraphicData femaleRottingGraphicData;

	public GraphicData stationaryGraphicData;

	public GraphicData femaleStationaryGraphicData;

	public AnimationDef flyingAnimationEast;

	public AnimationDef flyingAnimationNorth;

	public AnimationDef flyingAnimationSouth;

	public AnimationDef flyingAnimationEastFemale;

	public AnimationDef flyingAnimationNorthFemale;

	public AnimationDef flyingAnimationSouthFemale;

	public BodyPartToDrop butcherBodyPart;

	public void PostLoad()
	{
		untranslatedLabel = label;
		untranslatedLabelMale = labelMale;
		untranslatedLabelFemale = labelFemale;
	}

	public void ResolveReferences()
	{
		if (bodyGraphicData != null && bodyGraphicData.graphicClass == null)
		{
			bodyGraphicData.graphicClass = typeof(Graphic_Multi);
		}
		if (femaleGraphicData != null && femaleGraphicData.graphicClass == null)
		{
			femaleGraphicData.graphicClass = typeof(Graphic_Multi);
		}
		if (dessicatedBodyGraphicData != null && dessicatedBodyGraphicData.graphicClass == null)
		{
			dessicatedBodyGraphicData.graphicClass = typeof(Graphic_Multi);
		}
		if (femaleDessicatedBodyGraphicData != null && femaleDessicatedBodyGraphicData.graphicClass == null)
		{
			femaleDessicatedBodyGraphicData.graphicClass = typeof(Graphic_Multi);
		}
		if (corpseGraphicData != null && corpseGraphicData.graphicClass == null)
		{
			corpseGraphicData.graphicClass = typeof(Graphic_Multi);
		}
		if (femaleCorpseGraphicData != null && femaleCorpseGraphicData.graphicClass == null)
		{
			femaleCorpseGraphicData.graphicClass = typeof(Graphic_Multi);
		}
		if (silhouetteGraphicData != null && silhouetteGraphicData.graphicClass == null)
		{
			silhouetteGraphicData.graphicClass = typeof(Graphic_Single);
		}
		if (stationaryGraphicData != null && stationaryGraphicData.graphicClass == null)
		{
			stationaryGraphicData.graphicClass = typeof(Graphic_Multi);
		}
		if (femaleStationaryGraphicData != null && femaleStationaryGraphicData.graphicClass == null)
		{
			femaleStationaryGraphicData.graphicClass = typeof(Graphic_Multi);
		}
	}
}
