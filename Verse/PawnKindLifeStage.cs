namespace Verse
{
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
		}
	}
}
