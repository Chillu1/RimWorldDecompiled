using RimWorld;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public abstract class ModRequirement
	{
		public string packageId;

		public string displayName;

		public static Texture2D NotResolved = ContentFinder<Texture2D>.Get("UI/Icons/ModRequirements/NotResolved");

		public static Texture2D NotInstalled = ContentFinder<Texture2D>.Get("UI/Icons/ModRequirements/NotInstalled");

		public static Texture2D Installed = ContentFinder<Texture2D>.Get("UI/Icons/ModRequirements/Installed");

		public static Texture2D Resolved = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOn");

		public abstract bool IsSatisfied
		{
			get;
		}

		public abstract string RequirementTypeLabel
		{
			get;
		}

		public virtual string Tooltip => "ModPackageId".Translate() + ": " + packageId;

		public virtual Texture2D StatusIcon
		{
			get
			{
				if (!IsSatisfied)
				{
					return NotResolved;
				}
				return Resolved;
			}
		}

		public abstract void OnClicked(Page_ModsConfig window);
	}
}
