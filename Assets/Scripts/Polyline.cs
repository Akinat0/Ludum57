using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Polyline : MonoBehaviour
{

    [SerializeField] float thickness = 0.1f;
    [SerializeField] Material mat;
    
    public List<Vector3> points;
    public List<Color> Colors;
    
    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    void Awake()
    {
        mesh = new Mesh();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = mat;

        // Rebuild();
    }
    
    public void Rebuild()
    {
        if(mesh == null)
            return;
        
        Vector3[] vertices  = new Vector3[points.Count * 2];
        int[]     triangles = new int    [points.Count * 6];
        Color[]   colors    = new Color  [points.Count * 2];
        Vector3[] normals   = new Vector3[points.Count * 2];
        Vector2[] uvs       = new Vector2[points.Count * 2];

        for (int i = 0; i < points.Count; i++)
        {
            
            Vector3 pos = points[i];
            pos = transform.InverseTransformPoint(pos);

            Vector3 dir = (i == (points.Count() - 1)) ? Vector3.zero : (points[i + 1] - points[i]).normalized;

            Vector3 tangent = dir; //new Vector3(dir.y, -dir.x);
            Vector3 upVector = new Vector3(0, 0, -1);
            Vector3 normalVector = Vector3.Cross(tangent, upVector);

            
            
            
            vertices[i * 2 + 0] = pos - normalVector * thickness;
            vertices[i * 2 + 1] = pos + normalVector * thickness;
            colors[i * 2 + 0] = Colors[i];
            colors[i * 2 + 1] = Colors[i];
            normals[i * 2 + 0] = Vector3.up;
            normals[i * 2 + 1] = Vector3.up;
            uvs[i * 2 + 0] = Vector2.zero;//new Vector2(phase, invert ? 1 : 0);
            uvs[i * 2 + 1] = Vector2.zero;;//new Vector2(phase, invert ? 0 : 1);

            if (i < points.Count - 1)
            {
                triangles[i * 6 + 0] = i * 2 + 0;
                triangles[i * 6 + 1] = i * 2 + 1;
                triangles[i * 6 + 2] = i * 2 + 2;

                triangles[i * 6 + 3] = i * 2 + 1;
                triangles[i * 6 + 4] = i * 2 + 3;
                triangles[i * 6 + 5] = i * 2 + 2;
            }
            // else
            // {
            //     
            //     triangles[i * 6 + 0] = i * 2 + 0;
            //     triangles[i * 6 + 1] = i * 2 + 1;
            //     triangles[i * 6 + 2] = 0;
            //
            //     triangles[i * 6 + 3] = i * 2 + 1;
            //     triangles[i * 6 + 4] = 1;
            //     triangles[i * 6 + 5] = 0;
            // }
        

            Debug.DrawLine(pos, pos + upVector * 3, Color.blue, 5, false);
            Debug.DrawLine(pos, pos + tangent * 3, Color.red, 5, false);
            Debug.DrawLine(pos, pos + normalVector * 3, Color.green, 5, false);
        }
        
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.normals = normals;
        mesh.uv = uvs;


    
    }

    
}