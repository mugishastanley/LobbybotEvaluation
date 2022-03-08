using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLARTE.Serialization
{
	/// <summary>
	/// Binary serializer. It provide a fast and memory efficient way to serialize data into binary representation.
	/// </summary>
	/// <remarks>This class is pure C# and is compatible with all platforms, including hololens.</remarks>
	public partial class Binary
	{
		#region Members
		private static readonly Dictionary<Type, SupportedTypes> mapping = new Dictionary<Type, SupportedTypes>()
		{
			{typeof(object), SupportedTypes.OBJECT},
			{typeof(bool), SupportedTypes.BOOL},
			{typeof(byte), SupportedTypes.BYTE},
			{typeof(sbyte), SupportedTypes.SBYTE},
			{typeof(char), SupportedTypes.CHAR},
			{typeof(short), SupportedTypes.SHORT},
			{typeof(ushort), SupportedTypes.USHORT},
			{typeof(int), SupportedTypes.INT},
			{typeof(uint), SupportedTypes.UINT},
			{typeof(long), SupportedTypes.LONG},
			{typeof(ulong), SupportedTypes.ULONG},
			{typeof(float), SupportedTypes.FLOAT},
			{typeof(double), SupportedTypes.DOUBLE},
			{typeof(decimal), SupportedTypes.DECIMAL},
			{typeof(string), SupportedTypes.STRING},
			{typeof(Type), SupportedTypes.TYPE},
			{typeof(Vector2), SupportedTypes.UNITY_VECTOR2},
			{typeof(Vector3), SupportedTypes.UNITY_VECTOR3},
			{typeof(Vector4), SupportedTypes.UNITY_VECTOR4},
			{typeof(Quaternion), SupportedTypes.UNITY_QUATERNION},
			{typeof(Matrix4x4), SupportedTypes.UNITY_MATRIX4X4},
			{typeof(Color), SupportedTypes.UNITY_COLOR},
			{typeof(Gradient), SupportedTypes.UNITY_GRADIENT},
			{typeof(GradientColorKey), SupportedTypes.UNITY_GRADIENT_COLOR_KEY},
			{typeof(GradientAlphaKey), SupportedTypes.UNITY_GRADIENT_ALPHA_KEY},
			{typeof(Rect), SupportedTypes.UNITY_RECT},
			{typeof(RectOffset), SupportedTypes.UNITY_RECT_OFFSET},
			{typeof(LayerMask), SupportedTypes.UNITY_LAYER_MASK},
			{typeof(AnimationCurve), SupportedTypes.UNITY_ANIMATION_CURVE},
			{typeof(Keyframe), SupportedTypes.UNITY_KEY_FRAME},
			{typeof(GUIStyle), SupportedTypes.UNITY_GUI_STYLE},
			{typeof(GUIStyleState), SupportedTypes.UNITY_GUI_STYLE_STATE},
			{typeof(Font), SupportedTypes.UNITY_FONT},
			{typeof(Texture2D), SupportedTypes.UNITY_TEXTURE_2D},
		};
		#endregion

		#region Object dynamic serialization
		/// <summary>
		/// Deserialize objects where type is not known at compilation time.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized object.</param>
		/// <param name="value">The deserialized object.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out object value)
		{
			byte type;

			CheckDeserializationParameters(buffer, start);

			uint read = FromBytes(buffer, start, out type);

			if((SupportedTypes) type != SupportedTypes.NONE)
			{
				read += FromBytesWrapper(buffer, start + read, out value, (SupportedTypes) type);
			}
			else
			{
				value = null;
			}

			return read;
		}

		/// <summary>
		/// Serialize objects where type is not known at compilation time.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The serialized object.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, object value)
		{
			CheckSerializationParameters(buffer, start);

			SupportedTypes type = GetSupportedType(value.GetType());

			if(type == SupportedTypes.OBJECT)
			{
				throw new ArgumentException("Can not serialize object of unspecialized type.", "value");
			}

			uint written = ToBytes(ref buffer, start, (byte) type);

			if(type != SupportedTypes.NONE)
			{
				written += ToBytesWrapper(ref buffer, start + written, value, type);
			}

			return written;
		}
		#endregion

		#region Generics to type overloads casts
		public static SupportedTypes GetSupportedType(Type type)
		{
			SupportedTypes result;

			if(!mapping.TryGetValue(type, out result))
			{
				if(typeof(IBinaryTypeMapped).IsAssignableFrom(type))
				{
					result = SupportedTypes.BINARY_TYPE_MAPPED;
				}
				else if(typeof(IBinarySerializable).IsAssignableFrom(type))
				{
					result = SupportedTypes.BINARY_SERIALIZABLE;
				}
				else if(type.IsEnum)
				{
					result = SupportedTypes.ENUM;
				}
				else if(type.IsArray)
				{
					result = SupportedTypes.ARRAY;

					// Check inner type support
					GetSupportedType(type.GetElementType());
				}
				else if(CheckGenericDefinition(type, typeof(List<>)))
				{
					result = SupportedTypes.LIST;

					// Check inner type support
					CheckGenericParametersTypes(type, 1);
				}
				else if(CheckGenericDefinition(type, typeof(Dictionary<,>)))
				{
					result = SupportedTypes.DICTIONARY;

					// Check inner type support
					CheckGenericParametersTypes(type, 2);
				}
				else
				{
					throw new ArgumentException(string.Format("The type '{0}' is not supported.", type));
				}
			}

			return result;
		}

		protected uint FromBytesWrapper(Buffer buffer, uint start, out object value, SupportedTypes type)
		{
			uint read;

			switch(type)
			{
				case SupportedTypes.BOOL:
					{
						bool o;

						read = FromBytes(buffer, start, out o);

						value = o;
					}

					break;
				case SupportedTypes.BYTE:
					{
						byte b;

						read = FromBytes(buffer, start, out b);

						value = b;
					}

					break;
				case SupportedTypes.SBYTE:
					{
						sbyte sb;

						read = FromBytes(buffer, start, out sb);

						value = sb;
					}

					break;
				case SupportedTypes.CHAR:
					{
						char c;

						read = FromBytes(buffer, start, out c);

						value = c;
					}

					break;
				case SupportedTypes.SHORT:
					{
						short s;

						read = FromBytes(buffer, start, out s);

						value = s;
					}

					break;
				case SupportedTypes.USHORT:
					{
						ushort us;

						read = FromBytes(buffer, start, out us);

						value = us;
					}

					break;
				case SupportedTypes.INT:
					{
						int i;

						read = FromBytes(buffer, start, out i);

						value = i;
					}

					break;
				case SupportedTypes.UINT:
					{
						uint ui;

						read = FromBytes(buffer, start, out ui);

						value = ui;
					}

					break;
				case SupportedTypes.LONG:
					{
						long l;

						read = FromBytes(buffer, start, out l);

						value = l;
					}

					break;
				case SupportedTypes.ULONG:
					{
						ulong ul;

						read = FromBytes(buffer, start, out ul);

						value = ul;
					}

					break;
				case SupportedTypes.FLOAT:
					{
						float f;

						read = FromBytes(buffer, start, out f);

						value = f;
					}

					break;
				case SupportedTypes.DOUBLE:
					{
						double d;

						read = FromBytes(buffer, start, out d);

						value = d;
					}

					break;
				case SupportedTypes.DECIMAL:
					{
						decimal d;

						read = FromBytes(buffer, start, out d);

						value = d;
					}

					break;
				case SupportedTypes.STRING:
					{
						string str;

						read = FromBytes(buffer, start, out str);

						value = str;
					}

					break;
				case SupportedTypes.TYPE:
					{
						Type t;

						read = FromBytes(buffer, start, out t);

						value = t;
					}

					break;
				case SupportedTypes.OBJECT:
					read = FromBytes(buffer, start, out value);

					break;
				case SupportedTypes.ENUM:
					{
						Enum e;

						read = FromBytes(buffer, start, out e);

						value = e;
					}

					break;
				case SupportedTypes.ARRAY:
					{
						Array a;

						read = FromBytes(buffer, start, out a);

						value = a;
					}

					break;
				case SupportedTypes.LIST:
					{
						IList l;

						read = FromBytes(buffer, start, out l);

						value = l;
					}

					break;
				case SupportedTypes.DICTIONARY:
					{
						IDictionary dic;

						read = FromBytes(buffer, start, out dic);

						value = dic;
					}

					break;
				case SupportedTypes.BINARY_SERIALIZABLE:
					{
						IBinarySerializable v;

						read = FromBytes(buffer, start, out v);

						value = v;
					}

					break;
				case SupportedTypes.BINARY_TYPE_MAPPED:
					{
						IBinaryTypeMapped v;

						read = FromBytes(buffer, start, out v);

						value = v;
					}

					break;
				case SupportedTypes.UNITY_VECTOR2:
					{
						Vector2 v2;

						read = FromBytes(buffer, start, out v2);

						value = v2;
					}

					break;
				case SupportedTypes.UNITY_VECTOR3:
					{
						Vector3 v3;

						read = FromBytes(buffer, start, out v3);

						value = v3;
					}

					break;
				case SupportedTypes.UNITY_VECTOR4:
					{
						Vector4 v4;

						read = FromBytes(buffer, start, out v4);

						value = v4;
					}

					break;
				case SupportedTypes.UNITY_QUATERNION:
					{
						Quaternion q;

						read = FromBytes(buffer, start, out q);

						value = q;
					}

					break;
				case SupportedTypes.UNITY_MATRIX4X4:
					{
						Matrix4x4 m;

						read = FromBytes(buffer, start, out m);

						value = m;
					}

					break;
				case SupportedTypes.UNITY_COLOR:
					{
						Color c;

						read = FromBytes(buffer, start, out c);

						value = c;
					}

					break;
				case SupportedTypes.UNITY_GRADIENT:
					{
						Gradient g;

						read = FromBytes(buffer, start, out g);

						value = g;
					}

					break;
				case SupportedTypes.UNITY_GRADIENT_COLOR_KEY:
					{
						GradientColorKey k;

						read = FromBytes(buffer, start, out k);

						value = k;
					}

					break;
				case SupportedTypes.UNITY_GRADIENT_ALPHA_KEY:
					{
						Gradient k;

						read = FromBytes(buffer, start, out k);

						value = k;
					}

					break;
				case SupportedTypes.UNITY_RECT:
					{
						Rect r;

						read = FromBytes(buffer, start, out r);

						value = r;
					}

					break;
				case SupportedTypes.UNITY_RECT_OFFSET:
					{
						RectOffset r;

						read = FromBytes(buffer, start, out r);

						value = r;
					}

					break;
				case SupportedTypes.UNITY_LAYER_MASK:
					{
						LayerMask l;

						read = FromBytes(buffer, start, out l);

						value = l;
					}

					break;
				case SupportedTypes.UNITY_ANIMATION_CURVE:
					{
						AnimationCurve c;

						read = FromBytes(buffer, start, out c);

						value = c;
					}

					break;
				case SupportedTypes.UNITY_KEY_FRAME:
					{
						Keyframe k;

						read = FromBytes(buffer, start, out k);

						value = k;
					}

					break;
				case SupportedTypes.UNITY_GUI_STYLE:
					{
						GUIStyle s;

						read = FromBytes(buffer, start, out s);

						value = s;
					}

					break;
				case SupportedTypes.UNITY_GUI_STYLE_STATE:
					{
						GUIStyleState s;

						read = FromBytes(buffer, start, out s);

						value = s;
					}

					break;
				case SupportedTypes.UNITY_FONT:
					{
						Font f;

						read = FromBytes(buffer, start, out f);

						value = f;
					}

					break;
				case SupportedTypes.UNITY_TEXTURE_2D:
					{
						Texture2D t;

						read = FromBytes(buffer, start, out t);

						value = t;
					}

					break;
				default:
					throw new ArgumentException(string.Format("Unsupported Deserialization of type '{0}'.", type));
			}

			return read;
		}

		protected uint ToBytesWrapper(ref Buffer buffer, uint start, object value, SupportedTypes type)
		{
			uint written;

			switch(type)
			{
				case SupportedTypes.BOOL:
					written = ToBytes(ref buffer, start, (bool) value);
					break;
				case SupportedTypes.BYTE:
					written = ToBytes(ref buffer, start, (byte) value);
					break;
				case SupportedTypes.SBYTE:
					written = ToBytes(ref buffer, start, (sbyte) value);
					break;
				case SupportedTypes.CHAR:
					written = ToBytes(ref buffer, start, (char) value);
					break;
				case SupportedTypes.SHORT:
					written = ToBytes(ref buffer, start, (short) value);
					break;
				case SupportedTypes.USHORT:
					written = ToBytes(ref buffer, start, (ushort) value);
					break;
				case SupportedTypes.INT:
					written = ToBytes(ref buffer, start, (int) value);
					break;
				case SupportedTypes.UINT:
					written = ToBytes(ref buffer, start, (uint) value);
					break;
				case SupportedTypes.LONG:
					written = ToBytes(ref buffer, start, (long) value);
					break;
				case SupportedTypes.ULONG:
					written = ToBytes(ref buffer, start, (ulong) value);
					break;
				case SupportedTypes.FLOAT:
					written = ToBytes(ref buffer, start, (float) value);
					break;
				case SupportedTypes.DOUBLE:
					written = ToBytes(ref buffer, start, (double) value);
					break;
				case SupportedTypes.DECIMAL:
					written = ToBytes(ref buffer, start, (decimal) value);
					break;
				case SupportedTypes.STRING:
					written = ToBytes(ref buffer, start, (string) value);
					break;
				case SupportedTypes.TYPE:
					written = ToBytes(ref buffer, start, (Type) value);
					break;
				case SupportedTypes.OBJECT:
					written = ToBytes(ref buffer, start, value);
					break;
				case SupportedTypes.ENUM:
					written = ToBytes(ref buffer, start, (Enum) value);
					break;
				case SupportedTypes.ARRAY:
					written = ToBytes(ref buffer, start, (Array) value);
					break;
				case SupportedTypes.LIST:
					written = ToBytes(ref buffer, start, (IList) value);
					break;
				case SupportedTypes.DICTIONARY:
					written = ToBytes(ref buffer, start, (IDictionary) value);
					break;
				case SupportedTypes.BINARY_SERIALIZABLE:
					written = ToBytes(ref buffer, start, (IBinarySerializable) value);
					break;
				case SupportedTypes.BINARY_TYPE_MAPPED:
					written = ToBytes(ref buffer, start, (IBinaryTypeMapped) value);
					break;
				case SupportedTypes.UNITY_VECTOR2:
					written = ToBytes(ref buffer, start, (Vector2) value);
					break;
				case SupportedTypes.UNITY_VECTOR3:
					written = ToBytes(ref buffer, start, (Vector3) value);
					break;
				case SupportedTypes.UNITY_VECTOR4:
					written = ToBytes(ref buffer, start, (Vector4) value);
					break;
				case SupportedTypes.UNITY_QUATERNION:
					written = ToBytes(ref buffer, start, (Quaternion) value);
					break;
				case SupportedTypes.UNITY_MATRIX4X4:
					written = ToBytes(ref buffer, start, (Matrix4x4) value);
					break;
				case SupportedTypes.UNITY_COLOR:
					written = ToBytes(ref buffer, start, (Color) value);
					break;
				case SupportedTypes.UNITY_GRADIENT:
					written = ToBytes(ref buffer, start, (Gradient) value);
					break;
				case SupportedTypes.UNITY_GRADIENT_COLOR_KEY:
					written = ToBytes(ref buffer, start, (GradientColorKey) value);
					break;
				case SupportedTypes.UNITY_GRADIENT_ALPHA_KEY:
					written = ToBytes(ref buffer, start, (GradientAlphaKey) value);
					break;
				case SupportedTypes.UNITY_RECT:
					written = ToBytes(ref buffer, start, (Rect) value);
					break;
				case SupportedTypes.UNITY_RECT_OFFSET:
					written = ToBytes(ref buffer, start, (RectOffset) value);
					break;
				case SupportedTypes.UNITY_LAYER_MASK:
					written = ToBytes(ref buffer, start, (LayerMask) value);
					break;
				case SupportedTypes.UNITY_ANIMATION_CURVE:
					written = ToBytes(ref buffer, start, (AnimationCurve) value);
					break;
				case SupportedTypes.UNITY_KEY_FRAME:
					written = ToBytes(ref buffer, start, (Keyframe) value);
					break;
				case SupportedTypes.UNITY_GUI_STYLE:
					written = ToBytes(ref buffer, start, (GUIStyle) value);
					break;
				case SupportedTypes.UNITY_GUI_STYLE_STATE:
					written = ToBytes(ref buffer, start, (GUIStyleState) value);
					break;
				case SupportedTypes.UNITY_FONT:
					written = ToBytes(ref buffer, start, (Font) value);
					break;
				case SupportedTypes.UNITY_TEXTURE_2D:
					written = ToBytes(ref buffer, start, (Texture2D) value);
					break;
				default:
					throw new ArgumentException(string.Format("Unsupported serialization of type '{0}' with object of type '{1}'.", type, value.GetType()));
			}

			return written;
		}
		#endregion

		#region Parameters checks
		protected static bool CheckGenericDefinition(Type type, Type generic_definition)
		{
#if NETFX_CORE
			return type.GetTypeInfo().IsGenericType && type.GetTypeInfo().GetGenericTypeDefinition() == generic_definition;
#else
			return type.IsGenericType && type.GetGenericTypeDefinition() == generic_definition;
#endif
		}

		protected static void CheckGenericParametersTypes(Type type, uint nb_expected_types)
		{
			Type[] element_types = GetGenericParametersTypes(type);

			if(element_types == null || element_types.Length < nb_expected_types)
			{
				throw new ArgumentException(string.Format("The type '{0}' is not supported.", type));
			}

			for(uint i = 0; i < nb_expected_types; i++)
			{
				// Check if we get an exception or not
				GetSupportedType(element_types[i]);
			}
		}
		#endregion
	}
}
