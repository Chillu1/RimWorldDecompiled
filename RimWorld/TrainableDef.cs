using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class TrainableDef : Def
	{
		public float difficulty = -1f;

		public float minBodySize;

		public List<TrainableDef> prerequisites;

		[NoTranslate]
		public List<string> tags = new List<string>();

		public bool defaultTrainable;

		public TrainabilityDef requiredTrainability;

		public int steps = 1;

		public float listPriority;

		[NoTranslate]
		public string icon;

		[Unsaved(false)]
		public int indent;

		[Unsaved(false)]
		private Texture2D iconTex;

		public Texture2D Icon
		{
			get
			{
				if (iconTex == null)
				{
					iconTex = ContentFinder<Texture2D>.Get(icon);
				}
				return iconTex;
			}
		}

		public bool MatchesTag(string tag)
		{
			if (tag == defName)
			{
				return true;
			}
			for (int i = 0; i < tags.Count; i++)
			{
				if (tags[i] == tag)
				{
					return true;
				}
			}
			return false;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (difficulty < 0f)
			{
				yield return "difficulty not set";
			}
		}
	}
}
