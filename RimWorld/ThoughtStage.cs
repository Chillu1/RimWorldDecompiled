using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtStage
	{
		[MustTranslate]
		public string label;

		[MustTranslate]
		public string labelSocial;

		[MustTranslate]
		public string description;

		public float baseMoodEffect;

		public float baseOpinionOffset;

		public bool visible = true;

		[Unsaved(false)]
		private string cachedLabelCap;

		[Unsaved(false)]
		private string cachedLabelSocialCap;

		[Unsaved(false)]
		[TranslationHandle(Priority = 100)]
		public string untranslatedLabel;

		[Unsaved(false)]
		[TranslationHandle]
		public string untranslatedLabelSocial;

		public string LabelCap
		{
			get
			{
				if (cachedLabelCap == null)
				{
					cachedLabelCap = label.CapitalizeFirst();
				}
				return cachedLabelCap;
			}
		}

		public string LabelSocialCap
		{
			get
			{
				if (cachedLabelSocialCap == null)
				{
					cachedLabelSocialCap = labelSocial.CapitalizeFirst();
				}
				return cachedLabelSocialCap;
			}
		}

		public void PostLoad()
		{
			untranslatedLabel = label;
			untranslatedLabelSocial = labelSocial;
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (!labelSocial.NullOrEmpty() && labelSocial == label)
			{
				yield return "labelSocial is the same as label. labelSocial is unnecessary in this case";
			}
			if (baseMoodEffect != 0f && description.NullOrEmpty())
			{
				yield return "affects mood but doesn't have a description";
			}
		}
	}
}
