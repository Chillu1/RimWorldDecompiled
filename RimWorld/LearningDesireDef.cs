using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class LearningDesireDef : Def
	{
		public float selectionWeight = 1f;

		public float xpPerTick;

		[NoTranslate]
		public string iconPath;

		public Type workerClass = typeof(LearningGiver);

		public JobDef jobDef;

		private Texture2D icon;

		private LearningGiver workerInt;

		public Texture2D Icon
		{
			get
			{
				if (icon == null)
				{
					icon = ContentFinder<Texture2D>.Get(iconPath);
				}
				return icon;
			}
		}

		public LearningGiver Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (LearningGiver)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (jobDef == null)
			{
				yield return "Learning desires require a job to be configured.";
			}
		}
	}
}
