namespace Verse
{
	public class RoomStatScoreStage
	{
		public float minScore = float.MinValue;

		public string label;

		[Unsaved(false)]
		[TranslationHandle]
		public string untranslatedLabel;

		public void PostLoad()
		{
			untranslatedLabel = label;
		}
	}
}
