using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

[AddComponentMenu("Storage/Store Materials")]
[ExecuteInEditMode]
[DontStore]
public class StoreMaterials : MonoBehaviour
{
	static StoreMaterials()
	{
		DelegateSupport.RegisterFunctionType<Texture2D, int>();
		DelegateSupport.RegisterFunctionType<StoreMaterials, List<StoreMaterials.MaterialProperty>>();
		DelegateSupport.RegisterFunctionType<StoreMaterials.MaterialProperty, StoreMaterials.MaterialProperty.PropertyType>();
		DelegateSupport.RegisterFunctionType<StoreMaterials.MaterialProperty, string>();
		DelegateSupport.RegisterFunctionType<StoreMaterials.StoredValue, StoreMaterials.MaterialProperty>();
		DelegateSupport.RegisterFunctionType<StoreMaterials.StoredValue, object>();
	}

	public StoreMaterials()
	{
		if (this.MaterialProperties.Count == 0)
		{
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.color,
				name = "_Color"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.texture,
				name = "_MainTex"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.texture,
				name = "_Cube"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.real,
				name = "_SelfIllumStrength"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.texture,
				name = "_ReflectionTex"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.texture,
				name = "_Normals"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.texture,
				name = "_Normal"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.unknown,
				name = "_SelfIllumination"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.texture,
				name = "_NoiseTex"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.color,
				name = "_TintColor"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.unknown,
				name = "_Illum"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.unknown,
				name = "_EmissionLM"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.unknown,
				name = "_InterlacePattern"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.unknown,
				name = "_Intensity"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.unknown,
				name = "_Distort"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.unknown,
				name = "_ShimmerDistort"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.color,
				name = "_FogColor"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.color,
				name = "_SpecColor"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.real,
				name = "_Shininess"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.color,
				name = "_ReflectColor"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.texture,
				name = "_BumpMap"
			});
			this.MaterialProperties.Add(new StoreMaterials.MaterialProperty
			{
				type = StoreMaterials.MaterialProperty.PropertyType.unknown,
				name = "_Parallax"
			});
		}
	}

	public List<StoreMaterials.StoredValue> GetValues(Material m)
	{
		List<StoreMaterials.MaterialProperty> shaderProperties = this.GetShaderProperties(m);
		List<StoreMaterials.StoredValue> list = new List<StoreMaterials.StoredValue>();
		foreach (StoreMaterials.MaterialProperty materialProperty in shaderProperties)
		{
			StoreMaterials.StoredValue storedValue = new StoreMaterials.StoredValue
			{
				Property = materialProperty
			};
			list.Add(storedValue);
			switch (materialProperty.type)
			{
			case StoreMaterials.MaterialProperty.PropertyType.color:
				storedValue.Value = m.GetColor(materialProperty.name);
				break;
			case StoreMaterials.MaterialProperty.PropertyType.vector:
				storedValue.Value = m.GetVector(materialProperty.name);
				break;
			case StoreMaterials.MaterialProperty.PropertyType.texture:
				storedValue.Value = m.GetTexture(materialProperty.name);
				break;
			case StoreMaterials.MaterialProperty.PropertyType.textureOffset:
				storedValue.Value = m.GetTextureOffset(materialProperty.name);
				break;
			case StoreMaterials.MaterialProperty.PropertyType.textureScale:
				storedValue.Value = m.GetTextureScale(materialProperty.name);
				break;
			case StoreMaterials.MaterialProperty.PropertyType.matrix:
				storedValue.Value = m.GetMatrix(materialProperty.name);
				break;
			case StoreMaterials.MaterialProperty.PropertyType.real:
				storedValue.Value = m.GetFloat(materialProperty.name);
				break;
			}
		}
		return list;
	}

	public void SetValues(Material m, IEnumerable<StoreMaterials.StoredValue> values)
	{
		foreach (StoreMaterials.StoredValue storedValue in values)
		{
			switch (storedValue.Property.type)
			{
			case StoreMaterials.MaterialProperty.PropertyType.color:
				m.SetColor(storedValue.Property.name, (Color)storedValue.Value);
				break;
			case StoreMaterials.MaterialProperty.PropertyType.vector:
				m.SetVector(storedValue.Property.name, (Vector4)storedValue.Value);
				break;
			case StoreMaterials.MaterialProperty.PropertyType.texture:
				m.SetTexture(storedValue.Property.name, (Texture)storedValue.Value);
				break;
			case StoreMaterials.MaterialProperty.PropertyType.textureOffset:
				m.SetTextureOffset(storedValue.Property.name, (Vector2)storedValue.Value);
				break;
			case StoreMaterials.MaterialProperty.PropertyType.textureScale:
				m.SetTextureScale(storedValue.Property.name, (Vector2)storedValue.Value);
				break;
			case StoreMaterials.MaterialProperty.PropertyType.matrix:
				m.SetMatrix(storedValue.Property.name, (Matrix4x4)storedValue.Value);
				break;
			case StoreMaterials.MaterialProperty.PropertyType.real:
				m.SetFloat(storedValue.Property.name, (float)storedValue.Value);
				break;
			}
		}
	}

	public List<StoreMaterials.MaterialProperty> GetShaderProperties(Material material)
	{
		if (StoreMaterials.cache.ContainsKey(material.shader.name))
		{
			return StoreMaterials.cache[material.shader.name];
		}
		List<StoreMaterials.MaterialProperty> list = new List<StoreMaterials.MaterialProperty>();
		foreach (StoreMaterials.MaterialProperty materialProperty in this.MaterialProperties)
		{
			if (material.HasProperty(materialProperty.name))
			{
				if (materialProperty.type == StoreMaterials.MaterialProperty.PropertyType.unknown)
				{
					try
					{
						Color color = material.GetColor(materialProperty.name);
						if (color != this.transparent)
						{
							list.Add(new StoreMaterials.MaterialProperty
							{
								name = materialProperty.name,
								type = StoreMaterials.MaterialProperty.PropertyType.color
							});
						}
					}
					catch
					{
					}
					try
					{
						float @float = material.GetFloat(materialProperty.name);
						if (@float != 0f)
						{
							list.Add(new StoreMaterials.MaterialProperty
							{
								name = materialProperty.name,
								type = StoreMaterials.MaterialProperty.PropertyType.real
							});
						}
					}
					catch
					{
					}
					try
					{
						Texture texture = material.GetTexture(materialProperty.name);
						if (texture != null)
						{
							list.Add(new StoreMaterials.MaterialProperty
							{
								name = materialProperty.name,
								type = StoreMaterials.MaterialProperty.PropertyType.texture
							});
						}
					}
					catch
					{
					}
					try
					{
						Vector4 vector = material.GetVector(materialProperty.name);
						if (vector != Vector4.zero)
						{
							list.Add(new StoreMaterials.MaterialProperty
							{
								name = materialProperty.name,
								type = StoreMaterials.MaterialProperty.PropertyType.vector
							});
						}
					}
					catch
					{
					}
					try
					{
						Matrix4x4 matrix = material.GetMatrix(materialProperty.name);
						if (matrix != Matrix4x4.identity)
						{
							list.Add(new StoreMaterials.MaterialProperty
							{
								name = materialProperty.name,
								type = StoreMaterials.MaterialProperty.PropertyType.matrix
							});
						}
					}
					catch
					{
					}
					try
					{
						Vector2 textureOffset = material.GetTextureOffset(materialProperty.name);
						if (textureOffset != Vector2.zero)
						{
							list.Add(new StoreMaterials.MaterialProperty
							{
								name = materialProperty.name,
								type = StoreMaterials.MaterialProperty.PropertyType.textureOffset
							});
						}
					}
					catch
					{
					}
					try
					{
						Vector2 textureScale = material.GetTextureScale(materialProperty.name);
						if (textureScale != Vector2.zero)
						{
							list.Add(new StoreMaterials.MaterialProperty
							{
								name = materialProperty.name,
								type = StoreMaterials.MaterialProperty.PropertyType.textureScale
							});
						}
					}
					catch
					{
					}
				}
				else
				{
					list.Add(materialProperty);
				}
			}
		}
		StoreMaterials.cache[material.shader.name] = list;
		return list;
	}

	private void Awake()
	{
		this.OnEnable();
	}

	private void OnEnable()
	{
		if (base.GetComponent<Renderer>() != null)
		{
			this.MaterialProperties = (from m in base.GetComponent<Renderer>().sharedMaterials
			where m
			select m).SelectMany((Material m) => this.GetShaderProperties(m)).Discrete((StoreMaterials.MaterialProperty m) => m.name).ToList<StoreMaterials.MaterialProperty>();
		}
	}

	public List<StoreMaterials.MaterialProperty> MaterialProperties = new List<StoreMaterials.MaterialProperty>();

	private static Index<string, List<StoreMaterials.MaterialProperty>> cache = new Index<string, List<StoreMaterials.MaterialProperty>>();

	private Color transparent = new Color(0f, 0f, 0f, 0f);

	[Serializable]
	public class MaterialProperty
	{
		public string name;

		public StoreMaterials.MaterialProperty.PropertyType type;

		public enum PropertyType
		{
			unknown = -1,
			color,
			vector,
			texture,
			textureOffset,
			textureScale,
			matrix,
			real
		}
	}

	public class StoredValue
	{
		public StoreMaterials.MaterialProperty Property;

		public object Value;
	}
}
