using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class SongDef : Def
	{
		[NoTranslate]
		public string clipPath;

		public float volume = 1f;

		public bool playOnMap = true;

		public float commonality = 1f;

		public bool tense;

		public TimeOfDay allowedTimeOfDay = TimeOfDay.Any;

		public List<Season> allowedSeasons;

		public RoyalTitleDef minRoyalTitle;

		[Unsaved(false)]
		public AudioClip clip;

		public override void PostLoad()
		{
			base.PostLoad();
			if (defName == "UnnamedDef")
			{
				string[] array = clipPath.Split('/', '\\');
				defName = array[array.Length - 1];
			}
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				clip = ContentFinder<AudioClip>.Get(clipPath);
			});
		}
	}
}
