using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class MinifiedThing : ThingWithComps, IThingHolder
	{
		public const float MaxMinifiedGraphicSize = 1.1f;

		public const float CrateToGraphicScale = 1.16f;

		private ThingOwner innerContainer;

		protected Graphic cachedGraphic;

		private Graphic crateFrontGraphic;

		public Thing InnerThing
		{
			get
			{
				if (innerContainer.Count == 0)
				{
					return null;
				}
				return innerContainer[0];
			}
			set
			{
				if (value == InnerThing)
				{
					return;
				}
				if (value == null)
				{
					innerContainer.Clear();
					return;
				}
				if (innerContainer.Count != 0)
				{
					Log.Warning("Assigned 2 things to the same MinifiedThing " + this.ToStringSafe() + " (first=" + innerContainer[0].ToStringSafe() + " second=" + value.ToStringSafe() + ")");
					innerContainer.ClearAndDestroyContents();
				}
				innerContainer.TryAdd(value);
			}
		}

		public override Graphic Graphic
		{
			get
			{
				if (cachedGraphic == null)
				{
					cachedGraphic = InnerThing.Graphic.ExtractInnerGraphicFor(InnerThing);
					if ((float)InnerThing.def.size.x > 1.1f || (float)InnerThing.def.size.z > 1.1f)
					{
						Vector2 minifiedDrawSize = GetMinifiedDrawSize(InnerThing.def.size.ToVector2(), 1.1f);
						Vector2 newDrawSize = new Vector2(minifiedDrawSize.x / (float)InnerThing.def.size.x * cachedGraphic.drawSize.x, minifiedDrawSize.y / (float)InnerThing.def.size.z * cachedGraphic.drawSize.y);
						cachedGraphic = cachedGraphic.GetCopy(newDrawSize, null);
					}
					if (Math.Abs(InnerThing.def.minifiedDrawScale - 1f) > float.Epsilon)
					{
						cachedGraphic = cachedGraphic.GetCopy(new Vector2(InnerThing.def.minifiedDrawScale * cachedGraphic.drawSize.x, InnerThing.def.minifiedDrawScale * cachedGraphic.drawSize.y), null);
					}
				}
				return cachedGraphic;
			}
		}

		public override string LabelNoCount => InnerThing.LabelNoCount;

		public override string DescriptionDetailed => InnerThing.DescriptionDetailed;

		public override string DescriptionFlavor => InnerThing.DescriptionFlavor;

		public override ModContentPack ContentSource => InnerThing.ContentSource;

		private Graphic CrateFrontGraphic
		{
			get
			{
				if (crateFrontGraphic == null)
				{
					crateFrontGraphic = LoadCrateFrontGraphic();
				}
				return crateFrontGraphic;
			}
		}

		public static void TryInsertIntoAtlas()
		{
			GlobalTextureAtlasManager.TryInsertStatic(TextureAtlasGroup.Item, ContentFinder<Texture2D>.Get("Things/Item/Minified/CrateFront"));
			GlobalTextureAtlasManager.TryInsertStatic(TextureAtlasGroup.Item, ContentFinder<Texture2D>.Get("Things/Item/Minified/BurlapBag"));
		}

		public MinifiedThing()
		{
			innerContainer = new ThingOwner<Thing>(this, oneStackOnly: true);
		}

		protected virtual Graphic LoadCrateFrontGraphic()
		{
			return GraphicDatabase.Get<Graphic_Single>("Things/Item/Minified/CrateFront", ShaderDatabase.Cutout, GetMinifiedDrawSize(InnerThing.def.size.ToVector2(), 1.1f) * 1.16f, Color.white);
		}

		protected override void Tick()
		{
			if (InnerThing == null)
			{
				Log.Error("MinifiedThing with null InnerThing. Destroying.");
				Destroy();
			}
			else
			{
				base.Tick();
			}
		}

		protected override void TickInterval(int delta)
		{
			if (InnerThing == null && !base.Destroyed)
			{
				Log.Error("MinifiedThing with null InnerThing. Destroying.");
				Destroy();
			}
			else
			{
				base.TickInterval(delta);
			}
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public override Thing SplitOff(int count)
		{
			MinifiedThing minifiedThing = (MinifiedThing)base.SplitOff(count);
			if (minifiedThing != this)
			{
				minifiedThing.InnerThing = ThingMaker.MakeThing(InnerThing.def, InnerThing.Stuff);
				if (InnerThing is ThingWithComps thingWithComps)
				{
					for (int i = 0; i < thingWithComps.AllComps.Count; i++)
					{
						thingWithComps.AllComps[i].PostSplitOff(minifiedThing.InnerThing);
					}
				}
			}
			return minifiedThing;
		}

		public override bool CanStackWith(Thing other)
		{
			if (!(other is MinifiedThing minifiedThing))
			{
				return false;
			}
			if (base.CanStackWith(other))
			{
				return InnerThing.CanStackWith(minifiedThing.InnerThing);
			}
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			Blueprint_Install blueprint_Install = InstallBlueprintUtility.ExistingBlueprintFor(this);
			if (blueprint_Install != null)
			{
				GenDraw.DrawLineBetween(this.TrueCenter(), blueprint_Install.TrueCenter());
			}
		}

		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			if (InnerThing.def.minifiedManualDraw)
			{
				InnerThing.DrawNowAt(drawLoc, flip);
				CrateFrontGraphic.DrawFromDef(drawLoc + Altitudes.AltIncVect * 0.1f, Rot4.North, null);
				return;
			}
			CrateFrontGraphic.DrawFromDef(drawLoc + Altitudes.AltIncVect * 0.1f, Rot4.North, null);
			Rot4 rot = ((Graphic is Graphic_Single) ? Rot4.North : Rot4.South);
			if (InnerThing.def.overrideMinifiedRot != Rot4.Invalid)
			{
				rot = InnerThing.def.overrideMinifiedRot;
			}
			Vector3 vector = InnerThing.def.minifiedDrawOffset - Graphic.DrawOffset(rot);
			Graphic.Draw(drawLoc + vector, rot, this);
		}

		public override void Print(SectionLayer layer)
		{
			Vector3 drawPos = DrawPos;
			Material material = CrateFrontGraphic.MatSingle;
			Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Item, flipUv: false, vertexColors: false, out material, out var uvs, out var vertexColor);
			Printer_Plane.PrintPlane(layer, drawPos + Altitudes.AltIncVect * 0.1f, CrateFrontGraphic.drawSize, material, 0f, flipUv: false, uvs);
			Rot4 rot = Rot4.South;
			if (Graphic is Graphic_Single)
			{
				rot = Rot4.North;
			}
			if (InnerThing.def.overrideMinifiedRot != Rot4.Invalid)
			{
				rot = InnerThing.def.overrideMinifiedRot;
			}
			Material material2 = Graphic.MatAt(rot, this);
			Graphic.TryGetTextureAtlasReplacementInfo(material2, InnerThing.def.category.ToAtlasGroup(), flipUv: false, vertexColors: false, out material2, out uvs, out vertexColor);
			Printer_Plane.PrintPlane(layer, drawPos + InnerThing.def.minifiedDrawOffset, Graphic.drawSize, material2, 0f, flipUv: false, uvs);
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			bool spawned = base.Spawned;
			Map map = base.Map;
			InnerThing?.Notify_MinifiedThingAboutToBeDestroyed(mode);
			base.Destroy(mode);
			if (InnerThing == null)
			{
				return;
			}
			InstallBlueprintUtility.CancelBlueprintsFor(this);
			if (spawned)
			{
				switch (mode)
				{
				case DestroyMode.Deconstruct:
					SoundDefOf.Building_Deconstructed.PlayOneShot(new TargetInfo(base.Position, map));
					GenLeaving.DoLeavingsFor(InnerThing, map, mode, this.OccupiedRect());
					break;
				case DestroyMode.KillFinalize:
					GenLeaving.DoLeavingsFor(InnerThing, map, mode, this.OccupiedRect());
					break;
				}
			}
			innerContainer.ClearAndDestroyContents(mode);
		}

		public override void PreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
			base.PreTraded(action, playerNegotiator, trader);
			InstallBlueprintUtility.CancelBlueprintsFor(this);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			yield return InstallationDesignatorDatabase.DesignatorFor(def);
		}

		public override string GetInspectString()
		{
			string text = "NotInstalled".Translate();
			string inspectString = InnerThing.GetInspectString();
			if (!inspectString.NullOrEmpty())
			{
				text += "\n";
				text += inspectString;
			}
			return text;
		}

		protected Vector2 GetMinifiedDrawSize(Vector2 drawSize, float maxSideLength)
		{
			float num = maxSideLength / Mathf.Max(drawSize.x, drawSize.y);
			if (num >= 1f)
			{
				return drawSize;
			}
			return drawSize * num;
		}
	}
}
