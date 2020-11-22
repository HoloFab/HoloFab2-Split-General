using System;
using System.Collections.Generic;

#if !(UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || WINDOWS_UWP)
using Rhino.Geometry;
using System.Drawing;
#else
using UnityEngine;
#endif

using HoloFab.CustomData;

namespace HoloFab {
	// Tools for processing meshes.
	public static class MeshUtilities {
		//////////////////////////////////////////////////////////////////////////////
		#region RhinoOnly
		#if !(UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || WINDOWS_UWP)
		// Encode a Mesh.
		public static MeshData EncodeMesh(Mesh _mesh) {
			MeshData meshData = new MeshData();
            
			for (int i = 0; i < _mesh.Vertices.Count; i++) {
				meshData.vertices.Add(EncodeUtilities.EncodeLocation(_mesh.Vertices[i]));
			}
            
			for (int i = 0; i < _mesh.Faces.Count; i++) {
				if (!_mesh.Faces[i].IsQuad) {
					meshData.faces.Add(new int[] { 0, _mesh.Faces[i].A, _mesh.Faces[i].B, _mesh.Faces[i].C });
				} else {
					meshData.faces.Add(new int[] { 1, _mesh.Faces[i].A, _mesh.Faces[i].B, _mesh.Faces[i].C, _mesh.Faces[i].D });
				}
			}
            
			return meshData;
		}
		// Encode a Mesh with a Color.
		public static MeshData EncodeMesh(Mesh _mesh, Color _color) {
			MeshData meshData = EncodeMesh(_mesh);
			meshData.colors = new List<int[]>() {EncodeUtilities.EncodeColor(_color)};
			return meshData;
		}
		// Decode a Mesh.
		public static Mesh DecodeMesh(MeshData data) {
			Mesh mesh = new Mesh();
			foreach (float[] vertex in data.vertices)
				mesh.Vertices.Add(new Point3d(vertex[0], vertex[1], vertex[2]));
			foreach (int[] face in data.faces)
				mesh.Faces.AddFace(face[0], face[1], face[2]);
			mesh.FaceNormals.ComputeFaceNormals();
			mesh.Normals.ComputeNormals();
			mesh.Compact();
			return mesh;
		}
		#endif
		#endregion
		//////////////////////////////////////////////////////////////////////////////
		#region UnityOnly
		#if (UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || WINDOWS_UWP)
		// A function to decode extracted data into Unity Mesh object.
		public static Mesh DecodeMesh(List<Vector3> currentVertices, List<int> currentFaces, List<Color> currentColors=null) {
			Mesh mesh = new Mesh();// { name = name };
			mesh.SetVertices(currentVertices);
			//mesh.SetNormals(normals);
			mesh.SetTriangles(currentFaces, 0);
			mesh.SetColors(currentColors);
			mesh.RecalculateNormals();
			return mesh;
		}
		#endif
		#endregion
	}
}