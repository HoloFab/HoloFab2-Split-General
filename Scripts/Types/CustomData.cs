using System;
using System.Collections.Generic;

namespace HoloFab {
	// Structure to hold Custom data types holding data to be sent.
	namespace CustomData {
		public enum SourceType { TCP, UDP };
		public enum SourceCommunicationType { Sender, Receiver, SenderReceiver };
        
		// Custom Mesh item encoding.
		[Serializable]
		public class MeshData {
			public virtual List<float[]> vertices { get; set; }
			public virtual List<int[]> faces { get; set; }
			//public virtual List<float[]> normals { get; set; }
			public virtual List<int[]> colors { get; set; }
            
			public MeshData() {
				this.vertices = new List<float[]>();
				this.faces = new List<int[]>();
				this.colors = new List<int[]>();
			}
		}
        
		// Custom Tag item encoding.
		[Serializable]
		public struct LabelData {
			public List<string> text;
			public List<float[]> textLocation;
			public List<float> textSize;
			public List<int[]> textColor;
            
			public LabelData(List<string> _text, List<float[]> _textLocation, List<float> _textSize, List<int[]> _textColor) {
				this.text = _text;
				this.textLocation = _textLocation;
				this.textSize = _textSize;
				this.textColor = _textColor;
			}
		}
        
		// Cutom UI state encoding.
		[Serializable]
		public class UIData {
			public List<bool> bools;
			public List<int> ints;
			public List<float> floats;
            
			public UIData() {
				this.bools = new List<bool>();
				this.ints = new List<int>();
				this.floats = new List<float>();
			}
			public UIData(List<bool> _bools, List<int> _ints, List<float> _floats) : this() {
				if (_bools.Count > 0) this.bools = _bools;
				if (_ints.Count > 0) this.ints = _ints;
				if (_floats.Count > 0) this.floats = _floats;
			}
		}
		// Custom Marked Point Data encoding
		[Serializable]
		public class MarkedPointData {
            public List<float[]> points;
            public List<float[]> normals;
            
            public MarkedPointData() {
                this.points = new List<float[]>();
                this.normals = new List<float[]>();
            }
            public MarkedPointData(List<float[]> _points, List<float[]> _normals) : this() {
                if (_points.Count > 0) this.points = _points;
                if (_normals.Count > 0) this.normals = _normals;
            }
        }
	}
}