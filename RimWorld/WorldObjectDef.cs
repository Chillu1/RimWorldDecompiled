using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class WorldObjectDef : Def
	{
		public Type worldObjectClass = typeof(WorldObject);

		public bool canHaveFaction = true;

		public bool saved = true;

		public bool canBePlayerHome;

		public List<WorldObjectCompProperties> comps = new List<WorldObjectCompProperties>();

		public bool allowCaravanIncidentsWhichGenerateMap;

		public bool isTempIncidentMapOwner;

		public List<IncidentTargetTagDef> IncidentTargetTags;

		public bool selectable = true;

		public bool neverMultiSelect;

		public MapGeneratorDef mapGenerator;

		public List<Type> inspectorTabs;

		[Unsaved(false)]
		public List<InspectTabBase> inspectorTabsResolved;

		public bool useDynamicDrawer;

		public bool expandingIcon;

		[NoTranslate]
		public string expandingIconTexture;

		public float expandingIconPriority;

		[NoTranslate]
		public string texture;

		[Unsaved(false)]
		private Material material;

		[Unsaved(false)]
		private Texture2D expandingIconTextureInt;

		public bool expandMore;

		public bool rotateGraphicWhenTraveling;

		public Color? expandingIconColor;

		public float expandingIconDrawSize = 1f;

		public bool blockExitGridUntilBattleIsWon;

		public Material Material
		{
			get
			{
				if (texture.NullOrEmpty())
				{
					return null;
				}
				if (material == null)
				{
					material = MaterialPool.MatFrom(texture, ShaderDatabase.WorldOverlayTransparentLit, WorldMaterials.WorldObjectRenderQueue);
				}
				return material;
			}
		}

		public Texture2D ExpandingIconTexture
		{
			get
			{
				if (expandingIconTextureInt == null)
				{
					if (expandingIconTexture.NullOrEmpty())
					{
						return null;
					}
					expandingIconTextureInt = ContentFinder<Texture2D>.Get(expandingIconTexture);
				}
				return expandingIconTextureInt;
			}
		}

		public override void PostLoad()
		{
			base.PostLoad();
			if (inspectorTabs == null)
			{
				return;
			}
			for (int i = 0; i < inspectorTabs.Count; i++)
			{
				if (inspectorTabsResolved == null)
				{
					inspectorTabsResolved = new List<InspectTabBase>();
				}
				try
				{
					inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(inspectorTabs[i]));
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat("Could not instantiate inspector tab of type ", inspectorTabs[i], ": ", ex));
				}
			}
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].ResolveReferences(this);
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			for (int i = 0; i < comps.Count; i++)
			{
				foreach (string item2 in comps[i].ConfigErrors(this))
				{
					yield return item2;
				}
			}
			if (expandMore && !expandingIcon)
			{
				yield return "has expandMore but doesn't have any expanding icon";
			}
		}
	}
}
