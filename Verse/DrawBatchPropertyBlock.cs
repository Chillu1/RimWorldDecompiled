using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class DrawBatchPropertyBlock
	{
		private enum PropertyType
		{
			Float,
			Color,
			Vector
		}

		private struct Property
		{
			public int propertyId;

			public PropertyType type;

			public float floatVal;

			public Vector4 vectorVal;

			public void Write(MaterialPropertyBlock propertyBlock)
			{
				switch (type)
				{
				case PropertyType.Float:
					propertyBlock.SetFloat(propertyId, floatVal);
					break;
				case PropertyType.Color:
					propertyBlock.SetColor(propertyId, vectorVal);
					break;
				case PropertyType.Vector:
					propertyBlock.SetVector(propertyId, vectorVal);
					break;
				}
			}
		}

		private List<Property> properties = new List<Property>();

		public string leakDebugString;

		public void Clear()
		{
			properties.Clear();
		}

		public void SetFloat(string name, float val)
		{
			SetFloat(Shader.PropertyToID(name), val);
		}

		public void SetFloat(int propertyId, float val)
		{
			properties.Add(new Property
			{
				propertyId = propertyId,
				type = PropertyType.Float,
				floatVal = val
			});
		}

		public void SetColor(string name, Color val)
		{
			SetColor(Shader.PropertyToID(name), val);
		}

		public void SetColor(int propertyId, Color val)
		{
			properties.Add(new Property
			{
				propertyId = propertyId,
				type = PropertyType.Color,
				vectorVal = val
			});
		}

		public void SetVector(string name, Vector4 val)
		{
			SetVector(Shader.PropertyToID(name), val);
		}

		public void SetVector(int propertyId, Vector4 val)
		{
			properties.Add(new Property
			{
				propertyId = propertyId,
				type = PropertyType.Vector,
				vectorVal = val
			});
		}

		public void Write(MaterialPropertyBlock propertyBlock)
		{
			foreach (Property property in properties)
			{
				property.Write(propertyBlock);
			}
		}
	}
}
