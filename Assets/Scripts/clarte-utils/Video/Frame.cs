using System;
using UnityEngine;
using CLARTE.Rendering.Extensions;

namespace CLARTE.Video {
	public class InvalidDataFrame: Exception {
		public InvalidDataFrame() : base() { }
		public InvalidDataFrame(string message) : base(message) { }
		public InvalidDataFrame(string message, Exception innerException) : base(message, innerException) { }
	}

	public class Frame : ICloneable {

		public int Width { get; protected set; }
		public int Height { get; protected set; }
		public TextureFormat Format { get; protected set; }
		public int Depth { get; private set; }
		public byte[] Data { get; protected set; }

		public Frame(int width, int height, TextureFormat format) {
			Width = width;
			Height = height;
			Format = format;
			Depth = Texture2DExtensions.GetTextureFormatDepth(Format);
			Data = new byte[Width * Height * Depth];
		}

		public virtual object Clone() {
			Frame frame = new Frame(Width, Height, Format);
			Buffer.BlockCopy(Data, 0, frame.Data, 0, Data.Length);
			return frame;
		}
	}
}
