using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerCompProperties
	{
		[TranslationHandle]
		public Type compClass;

		public float minDaysPassed;

		public List<IncidentTargetTagDef> allowedTargetTags;

		public List<IncidentTargetTagDef> disallowedTargetTags;

		public float minIncChancePopulationIntentFactor = 0.05f;

		public List<string> enableIfAnyModActive;

		public List<string> disableIfAnyModActive;

		public bool Enabled
		{
			get
			{
				if (!enableIfAnyModActive.NullOrEmpty())
				{
					for (int i = 0; i < enableIfAnyModActive.Count; i++)
					{
						if (ModsConfig.IsActive(enableIfAnyModActive[i]))
						{
							return true;
						}
					}
					return false;
				}
				if (!disableIfAnyModActive.NullOrEmpty())
				{
					for (int j = 0; j < disableIfAnyModActive.Count; j++)
					{
						if (ModsConfig.IsActive(disableIfAnyModActive[j]))
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public StorytellerCompProperties()
		{
		}

		public StorytellerCompProperties(Type compClass)
		{
			this.compClass = compClass;
		}

		public virtual IEnumerable<string> ConfigErrors(StorytellerDef parentDef)
		{
			if (compClass == null)
			{
				yield return "a StorytellerCompProperties has null compClass.";
			}
			if (!enableIfAnyModActive.NullOrEmpty() && !disableIfAnyModActive.NullOrEmpty())
			{
				yield return "enableIfAnyModActive and disableIfAnyModActive can't be used simultaneously";
			}
		}

		public virtual void ResolveReferences(StorytellerDef parentDef)
		{
		}
	}
}
