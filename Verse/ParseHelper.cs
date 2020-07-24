using RimWorld;
using RimWorld.QuestGen;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Verse
{
	public static class ParseHelper
	{
		public static class Parsers<T>
		{
			public static Func<string, T> parser;

			public static readonly string profilerLabel = "ParseHelper.FromString<" + typeof(T).FullName + ">()";

			public static void Register(Func<string, T> method)
			{
				parser = method;
				parsers.Add(typeof(T), (string str) => method(str));
			}
		}

		private static Dictionary<Type, Func<string, object>> parsers;

		private static readonly char[] colorTrimStartParameters;

		private static readonly char[] colorTrimEndParameters;

		public static string ParseString(string str)
		{
			return str.Replace("\\n", "\n");
		}

		public static int ParseIntPermissive(string str)
		{
			if (!int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
			{
				result = (int)float.Parse(str, CultureInfo.InvariantCulture);
				Log.Warning("Parsed " + str + " as int.");
			}
			return result;
		}

		public static Vector3 FromStringVector3(string Str)
		{
			Str = Str.TrimStart('(');
			Str = Str.TrimEnd(')');
			string[] array = Str.Split(',');
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			float x = Convert.ToSingle(array[0], invariantCulture);
			float y = Convert.ToSingle(array[1], invariantCulture);
			float z = Convert.ToSingle(array[2], invariantCulture);
			return new Vector3(x, y, z);
		}

		public static Vector2 FromStringVector2(string Str)
		{
			Str = Str.TrimStart('(');
			Str = Str.TrimEnd(')');
			string[] array = Str.Split(',');
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			float x;
			float y;
			if (array.Length == 1)
			{
				x = (y = Convert.ToSingle(array[0], invariantCulture));
			}
			else
			{
				if (array.Length != 2)
				{
					throw new InvalidOperationException();
				}
				x = Convert.ToSingle(array[0], invariantCulture);
				y = Convert.ToSingle(array[1], invariantCulture);
			}
			return new Vector2(x, y);
		}

		public static Vector4 FromStringVector4Adaptive(string Str)
		{
			Str = Str.TrimStart('(');
			Str = Str.TrimEnd(')');
			string[] array = Str.Split(',');
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			float x = 0f;
			float y = 0f;
			float z = 0f;
			float w = 0f;
			if (array.Length >= 1)
			{
				x = Convert.ToSingle(array[0], invariantCulture);
			}
			if (array.Length >= 2)
			{
				y = Convert.ToSingle(array[1], invariantCulture);
			}
			if (array.Length >= 3)
			{
				z = Convert.ToSingle(array[2], invariantCulture);
			}
			if (array.Length >= 4)
			{
				w = Convert.ToSingle(array[3], invariantCulture);
			}
			if (array.Length >= 5)
			{
				Log.ErrorOnce($"Too many elements in vector {Str}", 16139142);
			}
			return new Vector4(x, y, z, w);
		}

		public static Rect FromStringRect(string str)
		{
			str = str.TrimStart('(');
			str = str.TrimEnd(')');
			string[] array = str.Split(',');
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			float x = Convert.ToSingle(array[0], invariantCulture);
			float y = Convert.ToSingle(array[1], invariantCulture);
			float width = Convert.ToSingle(array[2], invariantCulture);
			float height = Convert.ToSingle(array[3], invariantCulture);
			return new Rect(x, y, width, height);
		}

		public static float ParseFloat(string str)
		{
			return float.Parse(str, CultureInfo.InvariantCulture);
		}

		public static bool ParseBool(string str)
		{
			return bool.Parse(str);
		}

		public static long ParseLong(string str)
		{
			return long.Parse(str, CultureInfo.InvariantCulture);
		}

		public static double ParseDouble(string str)
		{
			return double.Parse(str, CultureInfo.InvariantCulture);
		}

		public static sbyte ParseSByte(string str)
		{
			return sbyte.Parse(str, CultureInfo.InvariantCulture);
		}

		public static Type ParseType(string str)
		{
			if (str == "null" || str == "Null")
			{
				return null;
			}
			Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(str);
			if (typeInAnyAssembly == null)
			{
				Log.Error("Could not find a type named " + str);
			}
			return typeInAnyAssembly;
		}

		public static Action ParseAction(string str)
		{
			string[] array = str.Split('.');
			string methodName = array[array.Length - 1];
			string typeName = (array.Length != 3) ? array[0] : (array[0] + "." + array[1]);
			MethodInfo method = GenTypes.GetTypeInAnyAssembly(typeName).GetMethods().First((MethodInfo m) => m.Name == methodName);
			return (Action)Delegate.CreateDelegate(typeof(Action), method);
		}

		public static Color ParseColor(string str)
		{
			str = str.TrimStart(colorTrimStartParameters);
			str = str.TrimEnd(colorTrimEndParameters);
			string[] array = str.Split(',');
			float num = ParseFloat(array[0]);
			float num2 = ParseFloat(array[1]);
			float num3 = ParseFloat(array[2]);
			bool num4 = num > 1f || num3 > 1f || num2 > 1f;
			float num5 = (!num4) ? 1 : 255;
			if (array.Length == 4)
			{
				num5 = FromString<float>(array[3]);
			}
			Color result = default(Color);
			if (!num4)
			{
				result.r = num;
				result.g = num2;
				result.b = num3;
				result.a = num5;
				return result;
			}
			result = GenColor.FromBytes(Mathf.RoundToInt(num), Mathf.RoundToInt(num2), Mathf.RoundToInt(num3), Mathf.RoundToInt(num5));
			return result;
		}

		public static PublishedFileId_t ParsePublishedFileId(string str)
		{
			return new PublishedFileId_t(ulong.Parse(str));
		}

		public static IntVec2 ParseIntVec2(string str)
		{
			return IntVec2.FromString(str);
		}

		public static IntVec3 ParseIntVec3(string str)
		{
			return IntVec3.FromString(str);
		}

		public static Rot4 ParseRot4(string str)
		{
			return Rot4.FromString(str);
		}

		public static CellRect ParseCellRect(string str)
		{
			return CellRect.FromString(str);
		}

		public static CurvePoint ParseCurvePoint(string str)
		{
			return CurvePoint.FromString(str);
		}

		public static NameTriple ParseNameTriple(string str)
		{
			NameTriple nameTriple = NameTriple.FromString(str);
			nameTriple.ResolveMissingPieces();
			return nameTriple;
		}

		public static FloatRange ParseFloatRange(string str)
		{
			return FloatRange.FromString(str);
		}

		public static IntRange ParseIntRange(string str)
		{
			return IntRange.FromString(str);
		}

		public static QualityRange ParseQualityRange(string str)
		{
			return QualityRange.FromString(str);
		}

		public static ColorInt ParseColorInt(string str)
		{
			str = str.TrimStart(colorTrimStartParameters);
			str = str.TrimEnd(colorTrimEndParameters);
			string[] array = str.Split(',');
			ColorInt result = new ColorInt(255, 255, 255, 255);
			result.r = ParseIntPermissive(array[0]);
			result.g = ParseIntPermissive(array[1]);
			result.b = ParseIntPermissive(array[2]);
			if (array.Length == 4)
			{
				result.a = ParseIntPermissive(array[3]);
			}
			else
			{
				result.a = 255;
			}
			return result;
		}

		public static TaggedString ParseTaggedString(string str)
		{
			return str;
		}

		static ParseHelper()
		{
			parsers = new Dictionary<Type, Func<string, object>>();
			colorTrimStartParameters = new char[5]
			{
				'(',
				'R',
				'G',
				'B',
				'A'
			};
			colorTrimEndParameters = new char[1]
			{
				')'
			};
			Parsers<string>.Register(ParseString);
			Parsers<int>.Register(ParseIntPermissive);
			Parsers<Vector3>.Register(FromStringVector3);
			Parsers<Vector2>.Register(FromStringVector2);
			Parsers<Vector4>.Register(FromStringVector4Adaptive);
			Parsers<Rect>.Register(FromStringRect);
			Parsers<float>.Register(ParseFloat);
			Parsers<bool>.Register(ParseBool);
			Parsers<long>.Register(ParseLong);
			Parsers<double>.Register(ParseDouble);
			Parsers<sbyte>.Register(ParseSByte);
			Parsers<Type>.Register(ParseType);
			Parsers<Action>.Register(ParseAction);
			Parsers<Color>.Register(ParseColor);
			Parsers<PublishedFileId_t>.Register(ParsePublishedFileId);
			Parsers<IntVec2>.Register(ParseIntVec2);
			Parsers<IntVec3>.Register(ParseIntVec3);
			Parsers<Rot4>.Register(ParseRot4);
			Parsers<CellRect>.Register(ParseCellRect);
			Parsers<CurvePoint>.Register(ParseCurvePoint);
			Parsers<NameTriple>.Register(ParseNameTriple);
			Parsers<FloatRange>.Register(ParseFloatRange);
			Parsers<IntRange>.Register(ParseIntRange);
			Parsers<QualityRange>.Register(ParseQualityRange);
			Parsers<ColorInt>.Register(ParseColorInt);
			Parsers<TaggedString>.Register(ParseTaggedString);
		}

		public static T FromString<T>(string str)
		{
			Func<string, T> parser = Parsers<T>.parser;
			if (parser != null)
			{
				return parser(str);
			}
			return (T)FromString(str, typeof(T));
		}

		public static object FromString(string str, Type itemType)
		{
			try
			{
				itemType = (Nullable.GetUnderlyingType(itemType) ?? itemType);
				if (itemType.IsEnum)
				{
					try
					{
						object obj = BackCompatibility.BackCompatibleEnum(itemType, str);
						if (obj != null)
						{
							return obj;
						}
						return Enum.Parse(itemType, str);
					}
					catch (ArgumentException innerException)
					{
						throw new ArgumentException(string.Concat(string.Concat("'", str, "' is not a valid value for ", itemType, ". Valid values are: \n"), GenText.StringFromEnumerable(Enum.GetValues(itemType))), innerException);
					}
				}
				if (parsers.TryGetValue(itemType, out Func<string, object> value))
				{
					return value(str);
				}
				if (typeof(ISlateRef).IsAssignableFrom(itemType))
				{
					ISlateRef obj2 = (ISlateRef)Activator.CreateInstance(itemType);
					obj2.SlateRef = str;
					return obj2;
				}
				throw new ArgumentException("Trying to parse to unknown data type " + itemType.Name + ". Content is '" + str + "'.");
			}
			catch (Exception innerException2)
			{
				throw new ArgumentException(string.Concat("Exception parsing ", itemType, " from \"", str, "\""), innerException2);
			}
		}

		public static bool HandlesType(Type type)
		{
			type = (Nullable.GetUnderlyingType(type) ?? type);
			if (!type.IsPrimitive && !type.IsEnum && !parsers.ContainsKey(type))
			{
				return typeof(ISlateRef).IsAssignableFrom(type);
			}
			return true;
		}

		public static bool CanParse(Type type, string str)
		{
			if (!HandlesType(type))
			{
				return false;
			}
			try
			{
				FromString(str, type);
			}
			catch (ArgumentException)
			{
				return false;
			}
			catch (FormatException)
			{
				return false;
			}
			return true;
		}
	}
}
