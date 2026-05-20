using Verse;

namespace RimWorld;

public class MapPortalProperties
{
	public MapGeneratorDef pocketMapGenerator;

	public ThingDef exitDef;

	public int pocketMapSize = 100;

	[MustTranslate]
	public string enteredLetterLabel;

	[MustTranslate]
	public string enteredLetterText;

	[MustTranslate]
	public LetterDef enteredLetterDef;

	public SoundDef traverseSound;
}
