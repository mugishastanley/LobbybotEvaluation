using System;
using UnityEngine;

namespace CLARTE.Rendering.Extensions {

	public class UnknownTextureFormatException: Exception {
		public UnknownTextureFormatException(string message) : base(message) { }
	};

	public static class Texture2DExtensions {

		public static int GetTextureFormatDepth(TextureFormat format) {
			switch (format) {
				case TextureFormat.RGBAFloat:
				case TextureFormat.ARGB4444:
				case TextureFormat.RGBA4444:
					return 16;
				case TextureFormat.ARGB32:
				case TextureFormat.RGBA32:
				case TextureFormat.BGRA32:
				case TextureFormat.RFloat:
					return 4;
				case TextureFormat.RGB24:
					return 3;
				case TextureFormat.R8:
				case TextureFormat.Alpha8:
					return 1;
			}
			throw new UnknownTextureFormatException("Please complete depth value for TextureFormat: " + format.ToString());
		}

		public static int GetTextureFormatDepth(this Texture2D tex) {
			return Texture2DExtensions.GetTextureFormatDepth(tex.format);
		}
	}
}
