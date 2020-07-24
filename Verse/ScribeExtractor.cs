using RimWorld;
using RimWorld.Planet;
using System;
using System.Xml;

namespace Verse
{
	public static class ScribeExtractor
	{
		public static T ValueFromNode<T>(XmlNode subNode, T defaultValue)
		{
			if (subNode == null)
			{
				return defaultValue;
			}
			XmlAttribute xmlAttribute = subNode.Attributes["IsNull"];
			if (xmlAttribute != null && xmlAttribute.Value.ToLower() == "true")
			{
				return default(T);
			}
			try
			{
				try
				{
					return ParseHelper.FromString<T>(subNode.InnerText);
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat("Exception parsing node ", subNode.OuterXml, " into a ", typeof(T), ":\n", ex.ToString()));
				}
				return default(T);
			}
			catch (Exception arg)
			{
				Log.Error("Exception loading XML: " + arg);
				return defaultValue;
			}
		}

		public static T DefFromNode<T>(XmlNode subNode) where T : Def, new()
		{
			if (subNode == null || subNode.InnerText == null || subNode.InnerText == "null")
			{
				return null;
			}
			string text = BackCompatibility.BackCompatibleDefName(typeof(T), subNode.InnerText, forDefInjections: false, subNode);
			T namedSilentFail = DefDatabase<T>.GetNamedSilentFail(text);
			if (namedSilentFail == null && !BackCompatibility.WasDefRemoved(subNode.InnerText, typeof(T)))
			{
				if (text == subNode.InnerText)
				{
					Log.Error(string.Concat("Could not load reference to ", typeof(T), " named ", subNode.InnerText));
				}
				else
				{
					Log.Error(string.Concat("Could not load reference to ", typeof(T), " named ", subNode.InnerText, " after compatibility-conversion to ", text));
				}
				BackCompatibility.PostCouldntLoadDef(subNode.InnerText);
			}
			return namedSilentFail;
		}

		public static T DefFromNodeUnsafe<T>(XmlNode subNode)
		{
			return (T)GenGeneric.InvokeStaticGenericMethod(typeof(ScribeExtractor), typeof(T), "DefFromNode", subNode);
		}

		public static T SaveableFromNode<T>(XmlNode subNode, object[] ctorArgs)
		{
			if (Scribe.mode != LoadSaveMode.LoadingVars)
			{
				Log.Error("Called SaveableFromNode(), but mode is " + Scribe.mode);
				return default(T);
			}
			if (subNode == null)
			{
				return default(T);
			}
			XmlAttribute xmlAttribute = subNode.Attributes["IsNull"];
			if (xmlAttribute != null && xmlAttribute.Value.ToLower() == "true")
			{
				return default(T);
			}
			try
			{
				XmlAttribute xmlAttribute2 = subNode.Attributes["Class"];
				string text = (xmlAttribute2 != null) ? xmlAttribute2.Value : typeof(T).FullName;
				Type type = BackCompatibility.GetBackCompatibleType(typeof(T), text, subNode);
				if (type == null)
				{
					Type bestFallbackType = GetBestFallbackType<T>(subNode);
					Log.Error(string.Concat("Could not find class ", text, " while resolving node ", subNode.Name, ". Trying to use ", bestFallbackType, " instead. Full node: ", subNode.OuterXml));
					type = bestFallbackType;
				}
				if (type.IsAbstract)
				{
					throw new ArgumentException("Can't load abstract class " + type);
				}
				IExposable exposable = (IExposable)Activator.CreateInstance(type, ctorArgs);
				bool flag = typeof(T).IsValueType || typeof(Name).IsAssignableFrom(typeof(T));
				if (!flag)
				{
					Scribe.loader.crossRefs.RegisterForCrossRefResolve(exposable);
				}
				XmlNode curXmlParent = Scribe.loader.curXmlParent;
				IExposable curParent = Scribe.loader.curParent;
				string curPathRelToParent = Scribe.loader.curPathRelToParent;
				Scribe.loader.curXmlParent = subNode;
				Scribe.loader.curParent = exposable;
				Scribe.loader.curPathRelToParent = null;
				try
				{
					exposable.ExposeData();
				}
				finally
				{
					Scribe.loader.curXmlParent = curXmlParent;
					Scribe.loader.curParent = curParent;
					Scribe.loader.curPathRelToParent = curPathRelToParent;
				}
				if (!flag)
				{
					Scribe.loader.initer.RegisterForPostLoadInit(exposable);
				}
				return (T)exposable;
			}
			catch (Exception ex)
			{
				T result = default(T);
				Log.Error(string.Concat("SaveableFromNode exception: ", ex, "\nSubnode:\n", subNode.OuterXml));
				return result;
			}
		}

		private static Type GetBestFallbackType<T>(XmlNode node)
		{
			if (typeof(Thing).IsAssignableFrom(typeof(T)))
			{
				ThingDef thingDef = TryFindDef<ThingDef>(node, "def");
				if (thingDef != null)
				{
					return thingDef.thingClass;
				}
			}
			else if (typeof(Hediff).IsAssignableFrom(typeof(T)))
			{
				HediffDef hediffDef = TryFindDef<HediffDef>(node, "def");
				if (hediffDef != null)
				{
					return hediffDef.hediffClass;
				}
			}
			else if (typeof(Ability).IsAssignableFrom(typeof(T)))
			{
				AbilityDef abilityDef = TryFindDef<AbilityDef>(node, "def");
				if (abilityDef != null)
				{
					return abilityDef.abilityClass;
				}
			}
			else if (typeof(Thought).IsAssignableFrom(typeof(T)))
			{
				ThoughtDef thoughtDef = TryFindDef<ThoughtDef>(node, "def");
				if (thoughtDef != null)
				{
					return thoughtDef.thoughtClass;
				}
			}
			return typeof(T);
		}

		private static TDef TryFindDef<TDef>(XmlNode node, string defNodeName) where TDef : Def, new()
		{
			XmlElement xmlElement = node[defNodeName];
			if (xmlElement == null)
			{
				return null;
			}
			return DefDatabase<TDef>.GetNamedSilentFail(BackCompatibility.BackCompatibleDefName(typeof(TDef), xmlElement.InnerText));
		}

		public static LocalTargetInfo LocalTargetInfoFromNode(XmlNode node, string label, LocalTargetInfo defaultValue)
		{
			LoadIDsWantedBank loadIDs = Scribe.loader.crossRefs.loadIDs;
			if (node != null && Scribe.EnterNode(label))
			{
				try
				{
					string innerText = node.InnerText;
					if (innerText.Length != 0 && innerText[0] == '(')
					{
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), "thing");
						return new LocalTargetInfo(IntVec3.FromString(innerText));
					}
					loadIDs.RegisterLoadIDReadFromXml(innerText, typeof(Thing), "thing");
					return LocalTargetInfo.Invalid;
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), label + "/thing");
			return defaultValue;
		}

		public static TargetInfo TargetInfoFromNode(XmlNode node, string label, TargetInfo defaultValue)
		{
			LoadIDsWantedBank loadIDs = Scribe.loader.crossRefs.loadIDs;
			if (node != null && Scribe.EnterNode(label))
			{
				try
				{
					string innerText = node.InnerText;
					if (innerText.Length != 0 && innerText[0] == '(')
					{
						ExtractCellAndMapPairFromTargetInfo(innerText, out string cell, out string map);
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), "thing");
						loadIDs.RegisterLoadIDReadFromXml(map, typeof(Map), "map");
						return new TargetInfo(IntVec3.FromString(cell), null, allowNullMap: true);
					}
					loadIDs.RegisterLoadIDReadFromXml(innerText, typeof(Thing), "thing");
					loadIDs.RegisterLoadIDReadFromXml(null, typeof(Map), "map");
					return TargetInfo.Invalid;
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), label + "/thing");
			loadIDs.RegisterLoadIDReadFromXml(null, typeof(Map), label + "/map");
			return defaultValue;
		}

		public static GlobalTargetInfo GlobalTargetInfoFromNode(XmlNode node, string label, GlobalTargetInfo defaultValue)
		{
			LoadIDsWantedBank loadIDs = Scribe.loader.crossRefs.loadIDs;
			if (node != null && Scribe.EnterNode(label))
			{
				try
				{
					string innerText = node.InnerText;
					if (innerText.Length != 0 && innerText[0] == '(')
					{
						ExtractCellAndMapPairFromTargetInfo(innerText, out string cell, out string map);
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), "thing");
						loadIDs.RegisterLoadIDReadFromXml(map, typeof(Map), "map");
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(WorldObject), "worldObject");
						return new GlobalTargetInfo(IntVec3.FromString(cell), null, allowNullMap: true);
					}
					if (int.TryParse(innerText, out int result))
					{
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), "thing");
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(Map), "map");
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(WorldObject), "worldObject");
						return new GlobalTargetInfo(result);
					}
					if (innerText.Length != 0 && innerText[0] == '@')
					{
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), "thing");
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(Map), "map");
						loadIDs.RegisterLoadIDReadFromXml(innerText.Substring(1), typeof(WorldObject), "worldObject");
						return GlobalTargetInfo.Invalid;
					}
					loadIDs.RegisterLoadIDReadFromXml(innerText, typeof(Thing), "thing");
					loadIDs.RegisterLoadIDReadFromXml(null, typeof(Map), "map");
					loadIDs.RegisterLoadIDReadFromXml(null, typeof(WorldObject), "worldObject");
					return GlobalTargetInfo.Invalid;
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), label + "/thing");
			loadIDs.RegisterLoadIDReadFromXml(null, typeof(Map), label + "/map");
			loadIDs.RegisterLoadIDReadFromXml(null, typeof(WorldObject), label + "/worldObject");
			return defaultValue;
		}

		public static LocalTargetInfo ResolveLocalTargetInfo(LocalTargetInfo loaded, string label)
		{
			if (Scribe.EnterNode(label))
			{
				try
				{
					Thing thing = Scribe.loader.crossRefs.TakeResolvedRef<Thing>("thing");
					IntVec3 cell = loaded.Cell;
					if (thing != null)
					{
						return new LocalTargetInfo(thing);
					}
					return new LocalTargetInfo(cell);
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			return loaded;
		}

		public static TargetInfo ResolveTargetInfo(TargetInfo loaded, string label)
		{
			if (Scribe.EnterNode(label))
			{
				try
				{
					Thing thing = Scribe.loader.crossRefs.TakeResolvedRef<Thing>("thing");
					Map map = Scribe.loader.crossRefs.TakeResolvedRef<Map>("map");
					IntVec3 cell = loaded.Cell;
					if (thing != null)
					{
						return new TargetInfo(thing);
					}
					if (cell.IsValid && map != null)
					{
						return new TargetInfo(cell, map);
					}
					return TargetInfo.Invalid;
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			return loaded;
		}

		public static GlobalTargetInfo ResolveGlobalTargetInfo(GlobalTargetInfo loaded, string label)
		{
			if (Scribe.EnterNode(label))
			{
				try
				{
					Thing thing = Scribe.loader.crossRefs.TakeResolvedRef<Thing>("thing");
					Map map = Scribe.loader.crossRefs.TakeResolvedRef<Map>("map");
					WorldObject worldObject = Scribe.loader.crossRefs.TakeResolvedRef<WorldObject>("worldObject");
					IntVec3 cell = loaded.Cell;
					int tile = loaded.Tile;
					if (thing != null)
					{
						return new GlobalTargetInfo(thing);
					}
					if (worldObject != null)
					{
						return new GlobalTargetInfo(worldObject);
					}
					if (cell.IsValid)
					{
						if (map != null)
						{
							return new GlobalTargetInfo(cell, map);
						}
						return GlobalTargetInfo.Invalid;
					}
					if (tile >= 0)
					{
						return new GlobalTargetInfo(tile);
					}
					return GlobalTargetInfo.Invalid;
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			return loaded;
		}

		public static BodyPartRecord BodyPartFromNode(XmlNode node, string label, BodyPartRecord defaultValue)
		{
			if (node != null && Scribe.EnterNode(label))
			{
				try
				{
					XmlAttribute xmlAttribute = node.Attributes["IsNull"];
					if (xmlAttribute != null && xmlAttribute.Value.ToLower() == "true")
					{
						return null;
					}
					BodyDef bodyDef = DefFromNode<BodyDef>(Scribe.loader.curXmlParent["body"]);
					if (bodyDef == null)
					{
						return null;
					}
					XmlElement xmlElement = Scribe.loader.curXmlParent["index"];
					int index = (xmlElement != null) ? int.Parse(xmlElement.InnerText) : (-1);
					index = BackCompatibility.GetBackCompatibleBodyPartIndex(bodyDef, index);
					return bodyDef.GetPartAtIndex(index);
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			return defaultValue;
		}

		private static void ExtractCellAndMapPairFromTargetInfo(string str, out string cell, out string map)
		{
			int num = str.IndexOf(')');
			cell = str.Substring(0, num + 1);
			int num2 = str.IndexOf(',', num + 1);
			map = str.Substring(num2 + 1);
			map = map.TrimStart(' ');
		}
	}
}
