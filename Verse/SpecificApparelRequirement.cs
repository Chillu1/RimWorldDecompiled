using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class SpecificApparelRequirement
	{
		public struct TagChance
		{
			public string tag;

			public float chance;
		}

		private string requiredTag;

		private List<TagChance> alternateTagChoices;

		private ThingDef stuff;

		private BodyPartGroupDef bodyPartGroup;

		private ApparelLayerDef apparelLayer;

		private Color color;

		public string RequiredTag => requiredTag;

		public List<TagChance> AlternateTagChoices => alternateTagChoices;

		public ThingDef Stuff => stuff;

		public BodyPartGroupDef BodyPartGroup => bodyPartGroup;

		public ApparelLayerDef ApparelLayer => apparelLayer;

		public Color Color => color;
	}
}
