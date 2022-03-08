using System;
using UnityEngine;

namespace CLARTE.Serialization
{
	/// <summary>
	/// Binary serializer. It provide a fast and memory efficient way to serialize data into binary representation.
	/// </summary>
	/// <remarks>This class is pure C# and is compatible with all platforms, including hololens.</remarks>
	public partial class Binary
	{
		#region Convert from bytes
		/// <summary>
		/// Deserialize a Vector2 value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out Vector2 value)
		{
			float x, y;

			uint read = FromBytes(buffer, start, out x);
			read += FromBytes(buffer, start + read, out y);

			value = new Vector2(x, y);

			return read;
		}

		/// <summary>
		/// Deserialize a Vector3 value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out Vector3 value)
		{
			float x, y, z;

			uint read = FromBytes(buffer, start, out x);
			read += FromBytes(buffer, start + read, out y);
			read += FromBytes(buffer, start + read, out z);

			value = new Vector3(x, y, z);

			return read;
		}

		/// <summary>
		/// Deserialize a Vector4 value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out Vector4 value)
		{
			float w, x, y, z;

			uint read = FromBytes(buffer, start, out x);
			read += FromBytes(buffer, start + read, out y);
			read += FromBytes(buffer, start + read, out z);
			read += FromBytes(buffer, start + read, out w);

			value = new Vector4(x, y, z, w);

			return read;
		}

		/// <summary>
		/// Deserialize a Quaternion value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out Quaternion value)
		{
			float w, x, y, z;

			uint read = FromBytes(buffer, start, out x);
			read += FromBytes(buffer, start + read, out y);
			read += FromBytes(buffer, start + read, out z);
			read += FromBytes(buffer, start + read, out w);

			value = new Quaternion(x, y, z, w);

			return read;
		}

		/// <summary>
		/// Deserialize a Matrix4x4 value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out Matrix4x4 value)
		{
			Vector4 col0, col1, col2, col3;

			uint read = FromBytes(buffer, start, out col0);
			read += FromBytes(buffer, start + read, out col1);
			read += FromBytes(buffer, start + read, out col2);
			read += FromBytes(buffer, start + read, out col3);

			value = new Matrix4x4(col0, col1, col2, col3);

			return read;
		}

		/// <summary>
		/// Deserialize a Color value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out Color value)
		{
			byte r, g, b, a;

			uint read = FromBytes(buffer, start, out r);
			read += FromBytes(buffer, start + read, out g);
			read += FromBytes(buffer, start + read, out b);
			read += FromBytes(buffer, start + read, out a);

			value = new Color32(r, g, b, a);

			return read;
		}

		/// <summary>
		/// Deserialize a Gradient value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out Gradient value)
		{
			Enum mode;
			GradientColorKey[] color;
			GradientAlphaKey[] alpha;

			value = new Gradient();

			uint read = FromBytes(buffer, start, out mode);
			read += FromBytes(buffer, start + read, out color);
			read += FromBytes(buffer, start + read, out alpha);

			value.mode = (GradientMode) mode;
			value.colorKeys = color;
			value.alphaKeys = alpha;

			return read;
		}

		/// <summary>
		/// Deserialize a GradientColorKey value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out GradientColorKey value)
		{
			Color c;
			float t;

			uint read = FromBytes(buffer, start, out c);
			read += FromBytes(buffer, start + read, out t);

			value = new GradientColorKey(c, t);

			return read;
		}

		/// <summary>
		/// Deserialize a GradientAlphaKey value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out GradientAlphaKey value)
		{
			float a, t;

			uint read = FromBytes(buffer, start, out a);
			read += FromBytes(buffer, start + read, out t);

			value = new GradientAlphaKey(a, t);

			return read;
		}

		/// <summary>
		/// Deserialize a Rect value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out Rect value)
		{
			Vector2 position, size;

			uint read = FromBytes(buffer, start, out position);
			read += FromBytes(buffer, start + read, out size);

			value = new Rect(position, size);

			return read;
		}

		/// <summary>
		/// Deserialize a RectOffset value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out RectOffset value)
		{
			int left, right, top, bottom;

			uint read = FromBytes(buffer, start, out left);
			read += FromBytes(buffer, start + read, out right);
			read += FromBytes(buffer, start + read, out top);
			read += FromBytes(buffer, start + read, out bottom);

			value = new RectOffset(left, right, top, bottom);

			return read;
		}

		/// <summary>
		/// Deserialize a LayerMask value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out LayerMask value)
		{
			int val;

			uint read = FromBytes(buffer, start, out val);

			value = val;

			return read;
		}

		/// <summary>
		/// Deserialize a AnimationCurve value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out AnimationCurve value)
		{
			Enum pre, post;
			Keyframe[] frames;

			uint read = FromBytes(buffer, start, out pre);
			read += FromBytes(buffer, start + read, out post);
			read += FromBytes(buffer, start + read, out frames);

			value = new AnimationCurve(frames);
			value.preWrapMode = (WrapMode) pre;
			value.postWrapMode = (WrapMode) post;

			return read;
		}

		/// <summary>
		/// Deserialize a Keyframe value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out Keyframe value)
		{
			float t, v, in_t, out_t, in_w, out_w;

			uint read = FromBytes(buffer, start, out t);
			read += FromBytes(buffer, start + read, out v);
			read += FromBytes(buffer, start + read, out in_t);
			read += FromBytes(buffer, start + read, out out_t);
			read += FromBytes(buffer, start + read, out in_w);
			read += FromBytes(buffer, start + read, out out_w);

			value = new Keyframe(t, v, in_t, out_t, in_w, out_w);

			return read;
		}

		/// <summary>
		/// Deserialize a GUIStyle value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out GUIStyle value)
		{
			GUIStyleState normal, onNormal, hover, onHover, active, onActive, focused, onFocused;
			RectOffset border, margin, padding, overflow;
			Vector2 contentOffset;
			Font font;
			Enum fontStyle, clipping, alignment, imagePosition;
			bool stretchWidth, stretchHeight, wordWrap, richText;
			float fixedWidth, fixedHeight;
			int fontSize;
			string name;

			uint read = FromBytes(buffer, start, out name);
			read += FromBytes(buffer, start + read, out normal);
			read += FromBytes(buffer, start + read, out onNormal);
			read += FromBytes(buffer, start + read, out hover);
			read += FromBytes(buffer, start + read, out onHover);
			read += FromBytes(buffer, start + read, out active);
			read += FromBytes(buffer, start + read, out onActive);
			read += FromBytes(buffer, start + read, out focused);
			read += FromBytes(buffer, start + read, out onFocused);
			read += FromBytes(buffer, start + read, out border);
			read += FromBytes(buffer, start + read, out margin);
			read += FromBytes(buffer, start + read, out padding);
			read += FromBytes(buffer, start + read, out overflow);
			read += FromBytes(buffer, start + read, out font);
			read += FromBytes(buffer, start + read, out fontSize);
			read += FromBytes(buffer, start + read, out fontStyle);
			read += FromBytes(buffer, start + read, out alignment);
			read += FromBytes(buffer, start + read, out wordWrap);
			read += FromBytes(buffer, start + read, out richText);
			read += FromBytes(buffer, start + read, out clipping);
			read += FromBytes(buffer, start + read, out imagePosition);
			read += FromBytes(buffer, start + read, out contentOffset);
			read += FromBytes(buffer, start + read, out fixedWidth);
			read += FromBytes(buffer, start + read, out fixedHeight);
			read += FromBytes(buffer, start + read, out stretchWidth);
			read += FromBytes(buffer, start + read, out stretchHeight);

			value = new GUIStyle();

			value.name = name;
			value.normal = normal;
			value.onNormal = onNormal;
			value.hover = hover;
			value.onHover = onHover;
			value.active = active;
			value.onActive = onActive;
			value.focused = focused;
			value.onFocused = onFocused;
			value.border = border;
			value.margin = margin;
			value.padding = padding;
			value.overflow = overflow;
			value.font = font;
			value.fontSize = fontSize;
			value.fontStyle = (FontStyle) fontStyle;
			value.alignment = (TextAnchor) alignment;
			value.wordWrap = wordWrap;
			value.richText = richText;
			value.clipping = (TextClipping) clipping;
			value.imagePosition = (ImagePosition) imagePosition;
			value.contentOffset = contentOffset;
			value.fixedWidth = fixedWidth;
			value.fixedHeight = fixedHeight;
			value.stretchWidth = stretchWidth;
			value.stretchHeight = stretchHeight;

			return read;
		}

		/// <summary>
		/// Deserialize a GUIStyleState value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out GUIStyleState value)
		{
			Texture2D background;
			Color text_color;

			uint read = FromBytes(buffer, start, out background);
			read += FromBytes(buffer, start + read, out text_color);

			value = new GUIStyleState();

			value.background = background;
			value.textColor = text_color;

			return read;
		}

		/// <summary>
		/// Deserialize a Font value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out Font value)
		{
			Enum hide_flags;
			string name;
			string[] names;
			int size;

			uint read = FromBytes(buffer, start, out name);
			read += FromBytes(buffer, start + read, out hide_flags);
			read += FromBytes(buffer, start + read, out names);
			read += FromBytes(buffer, start + read, out size);

			value = Font.CreateDynamicFontFromOSFont(names, size);

			value.name = name;
			value.hideFlags = (HideFlags) hide_flags;

			return read;
		}

		/// <summary>
		/// Deserialize a Texture2D value.
		/// </summary>
		/// <param name="buffer">The buffer containing the serialized data.</param>
		/// <param name="start">The start index in the buffer of the serialized value.</param>
		/// <param name="value">The deserialized value.</param>
		/// <returns>The number of deserialized bytes.</returns>
		public uint FromBytes(Buffer buffer, uint start, out Texture2D value)
		{
			Enum hide_flags, format, dimension, wrap_mode_u, wrap_mode_v, wrap_mode_w, filter_mode;
			string name;
			int width, height, aniso_level;
			bool has_mipmap;
			float mipmap_bias;
			byte[] data;

			uint read = FromBytes(buffer, start, out name);
			read += FromBytes(buffer, start + read, out hide_flags);
			read += FromBytes(buffer, start + read, out width);
			read += FromBytes(buffer, start + read, out height);
			read += FromBytes(buffer, start + read, out format);
			read += FromBytes(buffer, start + read, out dimension);
			read += FromBytes(buffer, start + read, out has_mipmap);
			read += FromBytes(buffer, start + read, out mipmap_bias);
			read += FromBytes(buffer, start + read, out wrap_mode_u);
			read += FromBytes(buffer, start + read, out wrap_mode_v);
			read += FromBytes(buffer, start + read, out wrap_mode_w);
			read += FromBytes(buffer, start + read, out filter_mode);
			read += FromBytes(buffer, start + read, out aniso_level);
			read += FromBytes(buffer, start + read, out data);

			value = new Texture2D(width, height, (TextureFormat) format, has_mipmap);

			value.name = name;
			value.hideFlags = (HideFlags) hide_flags;
			value.dimension = (UnityEngine.Rendering.TextureDimension) dimension;
			value.mipMapBias = mipmap_bias;
			value.wrapModeU = (TextureWrapMode) wrap_mode_u;
			value.wrapModeV = (TextureWrapMode) wrap_mode_v;
			value.wrapModeW = (TextureWrapMode) wrap_mode_w;
			value.filterMode = (FilterMode) filter_mode;
			value.anisoLevel = aniso_level;

			value.LoadRawTextureData(data);
			value.Apply();

			return read;
		}
		#endregion

		#region Convert to bytes
		/// <summary>
		/// Serialize a Vector2 value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Vector2 value)
		{
			uint written = ToBytes(ref buffer, start, value.x);
			written += ToBytes(ref buffer, start + written, value.y);

			return written;
		}

		/// <summary>
		/// Serialize a Vector3 value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Vector3 value)
		{
			uint written = ToBytes(ref buffer, start, value.x);
			written += ToBytes(ref buffer, start + written, value.y);
			written += ToBytes(ref buffer, start + written, value.z);

			return written;
		}

		/// <summary>
		/// Serialize a Vector4 value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Vector4 value)
		{
			uint written = ToBytes(ref buffer, start, value.x);
			written += ToBytes(ref buffer, start + written, value.y);
			written += ToBytes(ref buffer, start + written, value.z);
			written += ToBytes(ref buffer, start + written, value.w);

			return written;
		}

		/// <summary>
		/// Serialize a Quaternion value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Quaternion value)
		{
			uint written = ToBytes(ref buffer, start, value.x);
			written += ToBytes(ref buffer, start + written, value.y);
			written += ToBytes(ref buffer, start + written, value.z);
			written += ToBytes(ref buffer, start + written, value.w);

			return written;
		}

		/// <summary>
		/// Serialize a Matrix4x4 value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Matrix4x4 value)
		{
			uint written = ToBytes(ref buffer, start, value.GetColumn(0));
			written += ToBytes(ref buffer, start + written, value.GetColumn(1));
			written += ToBytes(ref buffer, start + written, value.GetColumn(2));
			written += ToBytes(ref buffer, start + written, value.GetColumn(3));

			return written;
		}

		/// <summary>
		/// Serialize a Color value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Color value)
		{
			Color32 val = value;

			uint written = ToBytes(ref buffer, start, val.r);
			written += ToBytes(ref buffer, start + written, val.g);
			written += ToBytes(ref buffer, start + written, val.b);
			written += ToBytes(ref buffer, start + written, val.a);

			return written;
		}

		/// <summary>
		/// Serialize a Gradient value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Gradient value)
		{
			uint written = ToBytes(ref buffer, start, value.mode);
			written += ToBytes(ref buffer, start + written, value.colorKeys);
			written += ToBytes(ref buffer, start + written, value.alphaKeys);

			return written;
		}

		/// <summary>
		/// Serialize a GradientColorKey value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, GradientColorKey value)
		{
			uint written = ToBytes(ref buffer, start, value.color);
			written += ToBytes(ref buffer, start + written, value.time);

			return written;
		}

		/// <summary>
		/// Serialize a GradientAlphaKey value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, GradientAlphaKey value)
		{
			uint written = ToBytes(ref buffer, start, value.alpha);
			written += ToBytes(ref buffer, start + written, value.time);

			return written;
		}

		/// <summary>
		/// Serialize a Rect value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Rect value)
		{
			uint written = ToBytes(ref buffer, start, value.position);
			written += ToBytes(ref buffer, start + written, value.size);

			return written;
		}

		/// <summary>
		/// Serialize a RectOffset value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, RectOffset value)
		{
			uint written = ToBytes(ref buffer, start, value.left);
			written += ToBytes(ref buffer, start + written, value.right);
			written += ToBytes(ref buffer, start + written, value.top);
			written += ToBytes(ref buffer, start + written, value.bottom);

			return written;
		}

		/// <summary>
		/// Serialize a LayerMask value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, LayerMask value)
		{
			return ToBytes(ref buffer, start, value.value); ;
		}

		/// <summary>
		/// Serialize a AnimationCurve value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, AnimationCurve value)
		{
			uint written = ToBytes(ref buffer, start, value.preWrapMode);
			written += ToBytes(ref buffer, start + written, value.postWrapMode);
			written += ToBytes(ref buffer, start + written, value.keys);

			return written;
		}

		/// <summary>
		/// Serialize a Keyframe value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Keyframe value)
		{
			uint written = ToBytes(ref buffer, start, value.time);
			written += ToBytes(ref buffer, start + written, value.value);
			written += ToBytes(ref buffer, start + written, value.inTangent);
			written += ToBytes(ref buffer, start + written, value.outTangent);
			written += ToBytes(ref buffer, start + written, value.inWeight);
			written += ToBytes(ref buffer, start + written, value.outWeight);

			return written;
		}

		/// <summary>
		/// Serialize a GUIStyle value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, GUIStyle value)
		{
			uint written = ToBytes(ref buffer, start, value.name);
			written += ToBytes(ref buffer, start + written, value.normal);
			written += ToBytes(ref buffer, start + written, value.onNormal);
			written += ToBytes(ref buffer, start + written, value.hover);
			written += ToBytes(ref buffer, start + written, value.onHover);
			written += ToBytes(ref buffer, start + written, value.active);
			written += ToBytes(ref buffer, start + written, value.onActive);
			written += ToBytes(ref buffer, start + written, value.focused);
			written += ToBytes(ref buffer, start + written, value.onFocused);
			written += ToBytes(ref buffer, start + written, value.border);
			written += ToBytes(ref buffer, start + written, value.margin);
			written += ToBytes(ref buffer, start + written, value.padding);
			written += ToBytes(ref buffer, start + written, value.overflow);
			written += ToBytes(ref buffer, start + written, value.font);
			written += ToBytes(ref buffer, start + written, value.fontSize);
			written += ToBytes(ref buffer, start + written, value.fontStyle);
			written += ToBytes(ref buffer, start + written, value.alignment);
			written += ToBytes(ref buffer, start + written, value.wordWrap);
			written += ToBytes(ref buffer, start + written, value.richText);
			written += ToBytes(ref buffer, start + written, value.clipping);
			written += ToBytes(ref buffer, start + written, value.imagePosition);
			written += ToBytes(ref buffer, start + written, value.contentOffset);
			written += ToBytes(ref buffer, start + written, value.fixedWidth);
			written += ToBytes(ref buffer, start + written, value.fixedHeight);
			written += ToBytes(ref buffer, start + written, value.stretchWidth);
			written += ToBytes(ref buffer, start + written, value.stretchHeight);

			return written;
		}

		/// <summary>
		/// Serialize a GUIStyleState value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, GUIStyleState value)
		{
			uint written = ToBytes(ref buffer, start, value.background);
			written += ToBytes(ref buffer, start + written, value.textColor);

			return written;
		}

		/// <summary>
		/// Serialize a Font value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Font value)
		{
			uint written = ToBytes(ref buffer, start, value.name);
			written += ToBytes(ref buffer, start + written, value.hideFlags);
			written += ToBytes(ref buffer, start + written, value.fontNames);
			written += ToBytes(ref buffer, start + written, value.fontSize);

			return written;
		}

		/// <summary>
		/// Serialize a Texture2D value.
		/// </summary>
		/// <param name="buffer">The buffer where to serialize the data.</param>
		/// <param name="start">The start index in the buffer where to serialize the data.</param>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The number of serialized bytes.</returns>
		public uint ToBytes(ref Buffer buffer, uint start, Texture2D value)
		{
			uint written = ToBytes(ref buffer, start, value.name);
			written += ToBytes(ref buffer, start + written, value.hideFlags);
			written += ToBytes(ref buffer, start + written, value.width);
			written += ToBytes(ref buffer, start + written, value.height);
			written += ToBytes(ref buffer, start + written, value.format);
			written += ToBytes(ref buffer, start + written, value.dimension);
			written += ToBytes(ref buffer, start + written, value.streamingMipmaps);
			written += ToBytes(ref buffer, start + written, value.mipMapBias);
			written += ToBytes(ref buffer, start + written, value.wrapModeU);
			written += ToBytes(ref buffer, start + written, value.wrapModeV);
			written += ToBytes(ref buffer, start + written, value.wrapModeW);
			written += ToBytes(ref buffer, start + written, value.filterMode);
			written += ToBytes(ref buffer, start + written, value.anisoLevel);
			written += ToBytes(ref buffer, start + written, value.GetRawTextureData());

			return written;
		}
		#endregion
	}
}
