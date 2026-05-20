using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_PlanetLayer : ScenPart
	{
		public PlanetLayerDef layer;

		public PlanetLayerSettingsDef settingsDef;

		public bool hide;

		public string tag;

		public List<LayerConnection> connections = new List<LayerConnection>();

		private float lastHeight;

		private static readonly List<string> tmpTags = new List<string>();

		private string[] vBuffer;

		private string radiusBuffer;

		private string extraCameraAltitudeBuffer;

		private string backgroundWorldCameraOffsetBuffer;

		private string backgroundWorldCameraParallaxBuffer;

		private string subdivisionsBuffer;

		private string viewAngleBuffer;

		private string numericBuffer;

		public PlanetLayerSettings Settings => settingsDef.settings;

		protected virtual bool CanEdit => true;

		public override bool Valid()
		{
			if (base.Valid())
			{
				return settingsDef != null;
			}
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref layer, "layer");
			Scribe_Defs.Look(ref settingsDef, "settingsDef");
			Scribe_Values.Look(ref hide, "hide", defaultValue: false);
			Scribe_Values.Look(ref tag, "tag");
			Scribe_Collections.Look(ref connections, "connections", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (layer == null)
				{
					Log.Error("ScenPart has null layer after loading.");
				}
				if (settingsDef == null)
				{
					Log.Error("ScenPart has null playet layer settings def.");
				}
			}
		}

		public override int GetHashCode()
		{
			HashCode hashCode = default(HashCode);
			hashCode.Add(base.GetHashCode());
			hashCode.Add((layer != null) ? layer.GetHashCode() : 0);
			hashCode.Add((settingsDef != null) ? settingsDef.GetHashCode() : 0);
			hashCode.Add(tag);
			foreach (LayerConnection connection in connections)
			{
				hashCode.Add(connection.GetHashCode());
			}
			return hashCode.ToHashCode();
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (string.IsNullOrEmpty(tag))
			{
				yield return "Layer tag empty";
			}
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			if (!hide)
			{
				ClearInvalidConnections();
				Rect row = listing.GetScenPartRect(this, lastHeight).TopPartPixels(ScenPart.RowHeight);
				float yMin = row.yMin;
				DoTypeRow(ref row);
				DoTagRow(ref row);
				DoSettings(ref row);
				DoConnections(ref row);
				lastHeight = row.yMax - yMin + 4f;
			}
		}

		private bool IsTagValid()
		{
			if (string.IsNullOrEmpty(tag))
			{
				return false;
			}
			foreach (string allTag in GetAllTags())
			{
				if (allTag == tag)
				{
					return false;
				}
			}
			return true;
		}

		public override bool TryMerge(ScenPart other)
		{
			return false;
		}

		public override bool CanCoexistWith(ScenPart other)
		{
			if (other is ScenPart_PlanetLayer scenPart_PlanetLayer && scenPart_PlanetLayer.tag == tag)
			{
				return false;
			}
			return true;
		}

		private List<string> GetAllTags()
		{
			tmpTags.Clear();
			foreach (ScenPart allPart in Find.Scenario.AllParts)
			{
				if (allPart != this && allPart is ScenPart_PlanetLayer scenPart_PlanetLayer && !string.IsNullOrEmpty(scenPart_PlanetLayer.tag))
				{
					tmpTags.Add(scenPart_PlanetLayer.tag);
				}
			}
			return tmpTags;
		}

		private void DoLabel(Rect row, string label)
		{
			Rect rect = row;
			rect.x -= rect.width + 5f;
			using (new TextBlock(TextAnchor.MiddleRight))
			{
				Widgets.Label(rect, label);
			}
		}

		private void DoTypeRow(ref Rect row)
		{
			TextBlock textBlock = new TextBlock(CanEdit ? Color.white : Color.gray);
			try
			{
				if (Widgets.ButtonText(row, layer?.LabelCap ?? "ScenPart_SelectQuestDef".Translate(), drawBackground: true, doMouseoverSound: true, CanEdit))
				{
					FloatMenuUtility.MakeMenu(DefDatabase<PlanetLayerDef>.AllDefs, (PlanetLayerDef id) => id.LabelCap, (PlanetLayerDef selected) => delegate
					{
						layer = selected;
					});
				}
				row.y += ScenPart.RowHeight + 4f;
			}
			finally
			{
				((IDisposable)textBlock/*cast due to .constrained prefix*/).Dispose();
			}
		}

		private void DoTagRow(ref Rect row)
		{
			string text = tag;
			tag = Widgets.TextArea(row, tag);
			if (!IsTagValid())
			{
				using (new TextBlock(ColorLibrary.RedReadable))
				{
					DoLabel(row, "ScenPart_PlanetLayerTag_Error".Translate());
				}
			}
			else
			{
				DoLabel(row, "ScenPart_PlanetLayerTag".Translate());
			}
			if (text != tag && !string.IsNullOrEmpty(tag))
			{
				foreach (ScenPart allPart in Find.Scenario.AllParts)
				{
					if (allPart == this || !(allPart is ScenPart_PlanetLayer scenPart_PlanetLayer))
					{
						continue;
					}
					foreach (LayerConnection connection in scenPart_PlanetLayer.connections)
					{
						if (connection.tag == text)
						{
							connection.tag = tag;
						}
					}
				}
			}
			row.y += ScenPart.RowHeight + 4f;
		}

		private void DoSettings(ref Rect row)
		{
			Seperator(ref row);
			DoLabel(row, "ScenPart_PlanetLayerSettings".Translate());
			TaggedString taggedString = settingsDef?.LabelCap ?? "ScenPart_PlanetLayerSettings_SelectDef".Translate();
			if (Widgets.ButtonText(row, taggedString))
			{
				FloatMenuUtility.MakeMenu(DefDatabase<PlanetLayerSettingsDef>.AllDefs, (PlanetLayerSettingsDef x) => x.LabelCap, (PlanetLayerSettingsDef x) => delegate
				{
					settingsDef = x;
				});
			}
			row.y += ScenPart.RowHeight + 4f;
		}

		private void DoConnections(ref Rect row)
		{
			Seperator(ref row);
			DoLabel(row, "ScenPart_PlanetLayerConnections".Translate());
			for (int num = connections.Count - 1; num >= 0; num--)
			{
				if (num % 2 == 1)
				{
					Widgets.DrawLightHighlight(row);
				}
				LayerConnection other = connections[num];
				Rect rect = row.LeftPart(0.25f);
				Rect rect2 = row.LeftPart(0.25f);
				Rect rect3 = row.RightPart(0.5f);
				rect2.x = rect.xMax;
				rect.xMax -= 4f;
				rect2.xMax += 4f;
				rect2.xMax -= 4f;
				rect3.xMin += 4f;
				if (Widgets.ButtonText(rect, other.tag))
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					int captured = num;
					list.Add(new FloatMenuOption("Remove", delegate
					{
						connections.RemoveAt(captured);
					}));
					foreach (string value in GetAllTags())
					{
						if (!connections.Any((LayerConnection x) => x.tag == value))
						{
							list.Add(new FloatMenuOption(value, delegate
							{
								other.tag = value;
							}));
						}
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}
				if (Widgets.ButtonText(label: (other.zoomMode == LayerConnection.ZoomMode.None) ? ((string)"ScenPart_PlanetLayerConnections_Zoom".Translate()) : ((other.zoomMode != LayerConnection.ZoomMode.ZoomIn) ? ((string)"ScenPart_PlanetLayerConnections_ZoomOut".Translate()) : ((string)"ScenPart_PlanetLayerConnections_ZoomIn".Translate())), rect: rect2))
				{
					List<FloatMenuOption> list2 = new List<FloatMenuOption>();
					int localIndex = num;
					if (other.zoomMode != LayerConnection.ZoomMode.None)
					{
						list2.Add(new FloatMenuOption("ScenPart_PlanetLayerConnections_Zoom".Translate(), delegate
						{
							other.zoomMode = LayerConnection.ZoomMode.None;
						}));
					}
					if (other.zoomMode != LayerConnection.ZoomMode.ZoomIn)
					{
						list2.Add(new FloatMenuOption("ScenPart_PlanetLayerConnections_ZoomIn".Translate(), delegate
						{
							other.zoomMode = LayerConnection.ZoomMode.ZoomIn;
							for (int i = 0; i < connections.Count; i++)
							{
								if (localIndex != i && connections[i].zoomMode == LayerConnection.ZoomMode.ZoomIn)
								{
									connections[i].zoomMode = LayerConnection.ZoomMode.None;
								}
							}
						}));
					}
					if (other.zoomMode != LayerConnection.ZoomMode.ZoomOut)
					{
						list2.Add(new FloatMenuOption("ScenPart_PlanetLayerConnections_ZoomOut".Translate(), delegate
						{
							other.zoomMode = LayerConnection.ZoomMode.ZoomOut;
							for (int i = 0; i < connections.Count; i++)
							{
								if (localIndex != i && connections[i].zoomMode == LayerConnection.ZoomMode.ZoomOut)
								{
									connections[i].zoomMode = LayerConnection.ZoomMode.None;
								}
							}
						}));
					}
					Find.WindowStack.Add(new FloatMenu(list2));
				}
				Rect rect4 = rect3.LeftHalf();
				rect4.xMax -= 4f;
				using (new TextBlock(TextAnchor.MiddleRight))
				{
					Widgets.Label(rect4, "ScenPart_PlanetLayerConnections_Fuel".Translate());
				}
				Widgets.TextFieldNumeric(rect3.RightHalf(), ref other.fuelCost, ref numericBuffer);
				row.y += ScenPart.RowHeight + 4f;
			}
			List<string> unusedTags = GetUnusedTags();
			TextBlock textBlock = new TextBlock(unusedTags.Any() ? Color.white : Color.gray);
			try
			{
				if (!Widgets.ButtonText(row, "ScenPart_PlanetLayerConnections_Add".Translate(), drawBackground: true, doMouseoverSound: true, unusedTags.Any()))
				{
					return;
				}
				FloatMenuUtility.MakeMenu(unusedTags, (string id) => id, (string nTag) => delegate
				{
					connections.Add(new LayerConnection
					{
						tag = nTag
					});
				});
			}
			finally
			{
				((IDisposable)textBlock/*cast due to .constrained prefix*/).Dispose();
			}
		}

		private void Seperator(ref Rect row)
		{
			Widgets.DrawLineHorizontal(row.x + 4f, row.y + row.height / 4f, row.width - 8f, Color.gray);
			row.y += 17f;
		}

		private List<string> GetUnusedTags()
		{
			tmpTags.Clear();
			foreach (ScenPart allPart in Find.Scenario.AllParts)
			{
				if (allPart != this)
				{
					ScenPart_PlanetLayer other = allPart as ScenPart_PlanetLayer;
					if (other != null && !string.IsNullOrEmpty(other.tag) && !connections.Any((LayerConnection t) => t.tag == other.tag))
					{
						tmpTags.Add(other.tag);
					}
				}
			}
			return tmpTags;
		}

		private void ClearInvalidConnections()
		{
			for (int num = connections.Count - 1; num >= 0; num--)
			{
				LayerConnection layerConnection = connections[num];
				bool flag = false;
				foreach (ScenPart allPart in Find.Scenario.AllParts)
				{
					if (this != allPart && allPart is ScenPart_PlanetLayer scenPart_PlanetLayer && scenPart_PlanetLayer.tag == layerConnection.tag)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					connections.RemoveAt(num);
				}
			}
		}
	}
}
