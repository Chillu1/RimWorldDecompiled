using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class ThingCategoryDef : Def
	{
		public ThingCategoryDef parent;

		[NoTranslate]
		public string iconPath;

		public bool resourceReadoutRoot;

		[Unsaved(false)]
		public TreeNode_ThingCategory treeNode;

		[Unsaved(false)]
		public List<ThingCategoryDef> childCategories = new List<ThingCategoryDef>();

		[Unsaved(false)]
		public List<ThingDef> childThingDefs = new List<ThingDef>();

		[Unsaved(false)]
		private HashSet<ThingDef> allChildThingDefsCached;

		[Unsaved(false)]
		public List<SpecialThingFilterDef> childSpecialFilters = new List<SpecialThingFilterDef>();

		[Unsaved(false)]
		public Texture2D icon = BaseContent.BadTex;

		public IEnumerable<ThingCategoryDef> Parents
		{
			get
			{
				if (parent != null)
				{
					yield return parent;
					foreach (ThingCategoryDef parent2 in parent.Parents)
					{
						yield return parent2;
					}
				}
			}
		}

		public IEnumerable<ThingCategoryDef> ThisAndChildCategoryDefs
		{
			get
			{
				yield return this;
				foreach (ThingCategoryDef childCategory in childCategories)
				{
					foreach (ThingCategoryDef thisAndChildCategoryDef in childCategory.ThisAndChildCategoryDefs)
					{
						yield return thisAndChildCategoryDef;
					}
				}
			}
		}

		public IEnumerable<ThingDef> DescendantThingDefs
		{
			get
			{
				foreach (ThingCategoryDef thisAndChildCategoryDef in ThisAndChildCategoryDefs)
				{
					foreach (ThingDef childThingDef in thisAndChildCategoryDef.childThingDefs)
					{
						yield return childThingDef;
					}
				}
			}
		}

		public IEnumerable<SpecialThingFilterDef> DescendantSpecialThingFilterDefs
		{
			get
			{
				foreach (ThingCategoryDef thisAndChildCategoryDef in ThisAndChildCategoryDefs)
				{
					foreach (SpecialThingFilterDef childSpecialFilter in thisAndChildCategoryDef.childSpecialFilters)
					{
						yield return childSpecialFilter;
					}
				}
			}
		}

		public IEnumerable<SpecialThingFilterDef> ParentsSpecialThingFilterDefs
		{
			get
			{
				foreach (ThingCategoryDef parent2 in Parents)
				{
					foreach (SpecialThingFilterDef childSpecialFilter in parent2.childSpecialFilters)
					{
						yield return childSpecialFilter;
					}
				}
			}
		}

		public bool ContainedInThisOrDescendant(ThingDef thingDef)
		{
			return allChildThingDefsCached.Contains(thingDef);
		}

		public override void ResolveReferences()
		{
			allChildThingDefsCached = new HashSet<ThingDef>();
			foreach (ThingCategoryDef thisAndChildCategoryDef in ThisAndChildCategoryDefs)
			{
				foreach (ThingDef childThingDef in thisAndChildCategoryDef.childThingDefs)
				{
					allChildThingDefsCached.Add(childThingDef);
				}
			}
		}

		public override void PostLoad()
		{
			treeNode = new TreeNode_ThingCategory(this);
			if (!iconPath.NullOrEmpty())
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					icon = ContentFinder<Texture2D>.Get(iconPath);
				});
			}
		}

		public static ThingCategoryDef Named(string defName)
		{
			return DefDatabase<ThingCategoryDef>.GetNamed(defName);
		}

		public override int GetHashCode()
		{
			return defName.GetHashCode();
		}
	}
}
