using CLARTE.Threads.DataFlow.Unity;
using UnityEngine;

namespace CLARTE.Video {

	public class FrameToMaterialConsumer: DataConsumer<Frame> {
		public Material FrameDropMaterial;

		protected Texture2D frameDropTexture;
		protected bool available = false;
		protected Frame frame;

		protected override void Update() {
			base.Update();
			if (available) {
				if (frameDropTexture == null) {
					frameDropTexture = CreateTexture();
					FrameDropMaterial.mainTexture = frameDropTexture;
				}
				LoadTextureData();
				available = false;
			}
		}

		protected virtual Texture2D CreateTexture() {
			return new Texture2D(frame.Width, frame.Height, frame.Format, false);
		}

		protected virtual void LoadTextureData() {
			frameDropTexture.LoadRawTextureData(frame.Data);
			frameDropTexture.Apply();
		}

		protected override void ConsumeData(Frame data) {
			if (!available) {
				frame = data;
				available = true;
			}
		}

	}
}
