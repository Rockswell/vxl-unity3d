﻿using UnityEngine;
using System.Collections;

using Cubiquity.Impl;

namespace Cubiquity
{
	[ExecuteInEditMode]
	/// Causes the colored cubes volume to have a collision mesh and allows it to participate in collisions.
	/**
	 * See the base VolumeCollider class for further details.
	 */
	public class ColoredCubesVolumeCollider : VolumeCollider
	{
		unsafe public override Mesh BuildMeshFromNodeHandle(uint nodeHandle)
		{
            Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f); // Required for the CubicVertex decoding process.

            // Get the data from Cubiquity.
            uint noOfVertices; ColoredCubesVertex* vertices = null; uint noOfIndices; ushort* indices = null;
            CubiquityDLL.GetMesh(nodeHandle, out noOfVertices, &vertices, out noOfIndices, &indices);

            // Cubiquity uses 16-bit index arrays to save space, and it appears Unity does the same (at least, there is
            // a limit of 65535 vertices per mesh). However, the Mesh.triangles property is of the signed 32-bit int[]
            // type rather than the unsigned 16-bit ushort[] type. Perhaps this is so they can switch to 32-bit index
            // buffers in the future? At any rate, it means we have to perform a conversion.
            int[] indicesAsInt = new int[noOfIndices];
            for (int ct = 0; ct < noOfIndices; ct++)
            {
                indicesAsInt[ct] = *indices;
                indices++;
            }

            // Create the arrays which we'll copy the data to.
            Vector3[] positions = new Vector3[noOfVertices];

            // Move the data from our Cubiquity-owned memory to managed memory. We also
            // need to decode the data as Cubiquity stores it in a compressed form.
            for (int ct = 0; ct < noOfVertices; ct++)
            {
                // Get and decode the position
                positions[ct].Set(vertices->x, vertices->y, vertices->z);
                positions[ct] -= offset;

                // Now do the next vertex.
                vertices++;
            }

            // Create rendering mesh
            Mesh mesh = new Mesh();
            mesh.hideFlags = HideFlags.DontSave;

            // Assign vertex data to the mesh.
            mesh.vertices = positions;

            // Assign index data to the meshes.
            mesh.triangles = indicesAsInt;

            return mesh;
		}
	}
}
