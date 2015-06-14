using System;
using UnityEngine;
using System.Collections.Generic;

public class Drawer: MonoBehaviour
{
  public Material lineMaterial;

  struct Line
  {
    public Vector2 from;
    public Vector2 to;
    public Color color;

    public Line( Vector2 from, Vector2 to, Color color )
    {
      this.from = from;
      this.to = to;
      this.color = color;
    }
  }

  static List<Line> lines = new List<Line>();

  static Material CreateLineMaterial()
  {
    var mat = new Material(
       @"Shader ""Lines/Colored Blended"" {
       SubShader { Pass {
           Blend SrcAlpha OneMinusSrcAlpha
           ZWrite Off Cull Off Fog { Mode Off }
           BindChannels {
             Bind ""vertex"", vertex Bind ""color"", color }
       }}}"
    );

    mat.hideFlags = HideFlags.HideAndDontSave;
    mat.shader.hideFlags = HideFlags.HideAndDontSave;

    return mat;
  }

  void Awake()
  {
    if( lineMaterial == null )
      lineMaterial = CreateLineMaterial();
  }

  void OnPostRender()
  {
//    lineMaterial.SetPass( 0 );


		foreach( var l in lines )
		{
			Debug.DrawRay(l.from, l.to);
		}
//    GL.Begin( GL.LINES );
//
//      foreach( var l in lines )
//      {
//        GL.Color( l.color );
//        GL.Vertex3( l.from.x, l.from.y, l.from.z );
//        GL.Vertex3( l.to.x, l.to.y, l.to.z  );
//      }
//
//    GL.End();
  }

  void FixedUpdate()
  {
    lines.Clear();
  }

  public static void DrawLine( Vector2 from, Vector2 to, Color color )
  {
//    lines.Add( new Line(from, to, color) );
  }

  public static void DrawRay( Vector2 from, Vector2 to, Color color )
  {
//		Debug.Log("DrawRay - from: " + from + ", to: " + (from + to));
		Debug.DrawLine(from, from + to);
//    lines.Add( new Line(from, from + to, color) );
  }
}



