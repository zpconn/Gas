using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.DirectX;
using Direct3D = Microsoft.DirectX.Direct3D;
using Gas.Graphics;
using Gas.Helpers;

namespace GamePrototype
{
    class ShadowManager
    {
        #region Variables
        private static Renderer renderer = null;
        private static Effect shadowEffect = null;

        private static List<Shadow> shadows = new List<Shadow>();

        private static Texture shadowTex = null;
        private static Texture blurHorizontalTex = null;
        private static Texture blurVerticalTex = null;
        private static Texture finalTex = null;

        private static Mesh fullscreenQuad = null;

        private static Config config = new Config( "Config.txt" );
        private static float blur = 3.0f;

        private static bool initialized = false;
        #endregion

        #region Properties
        public static float Blur
        {
            get
            {
                return blur;
            }
            set
            {
                blur = value;
            }
        }
        #endregion

        #region Helpers
        private class Quad
        {
            public Direct3D.CustomVertex.PositionColoredTextured[] Vertices =
                new Microsoft.DirectX.Direct3D.CustomVertex.PositionColoredTextured[ 4 ];
        }

        private class Shadow
        {
            public List<Quad> Quads = new List<Quad>();
        }
        #endregion

        #region Initialization
        public static void Initialize( Renderer renderer )
        {
            ShadowManager.renderer = renderer;

            blur = config.GetSetting<float>( "ShadowBlur" );

            shadowTex = new Texture( renderer, renderer.FullscreenSize.Width / 2,
                renderer.FullscreenSize.Height / 2, true );
            blurHorizontalTex = new Texture( renderer, renderer.FullscreenSize.Width / 2,
                renderer.FullscreenSize.Height / 2, true );
            blurVerticalTex = new Texture( renderer, renderer.FullscreenSize.Width / 2,
                renderer.FullscreenSize.Height / 2, true );
            finalTex = new Texture( renderer, renderer.FullscreenSize.Width,
                renderer.FullscreenSize.Height, true );

            fullscreenQuad = Mesh.Rectangle( renderer, Color.Black, renderer.FullscreenSize.Width,
                renderer.FullscreenSize.Height );

            shadowEffect = GlobalResourceCache.CreateEffectFromFile( renderer,
                @".\\FXFiles\\Shadow.fx" );

            initialized = true;
        }
        #endregion

        #region Shadow generation
        public static void GenerateShadow( Mesh polygonGeometry, Matrix worldMatrix, Vector2 lightPosWS,
            float zValue, Vector2 shadowCasterCenter )
        {
            Vector3 UVOffset = new Vector3( 0.0f, -0.5f, 0.0f );

            // Transform the light position into model space
            Vector2 lightPos = Vector2.TransformCoordinate( lightPosWS, Matrix.Invert( worldMatrix ) );

            List<Edge> contourEdges = new List<Edge>();

            for ( int edgeIndex = 0; edgeIndex < polygonGeometry.NumEdges; ++edgeIndex )
            {
                Edge edge = polygonGeometry.GetEdge( edgeIndex );
                Vector2 edgeCenter = ( edge.Vertex1Pos + edge.Vertex2Pos ) * 0.5f;
                Vector2 incidentLightDir = edgeCenter - lightPos;

                // If the edge faces away from the light source
                if ( Vector2.Dot( incidentLightDir, edge.Normal ) >= 0.0f )
                {
                    contourEdges.Add( edge );
                }
            }

            if ( contourEdges.Count < 1 || contourEdges.Count == polygonGeometry.NumEdges )
            {
                return;
            }

            const float ExtrudeMagnitude = 1280;

            Shadow shadow = new Shadow();

            Vector3 lightPosVec3 = new Vector3( lightPos.X, lightPos.Y, zValue );
            lightPosVec3.TransformCoordinate( worldMatrix );

            int quadIndex = 0;
            foreach ( Edge edge in contourEdges )
            {
                Vector3 vertex1 = new Vector3(
                    edge.Vertex1Pos.X, edge.Vertex1Pos.Y, zValue );
                Vector3 vertex2 = new Vector3(
                    edge.Vertex2Pos.X, edge.Vertex2Pos.Y, zValue );

                // Transform the position data from model space to world space
                vertex1.TransformCoordinate( worldMatrix );
                vertex2.TransformCoordinate( worldMatrix );

                Quad quad = new Quad();
                Color shadowColor = Color.FromArgb( 1, 0, 0, 0 );

                quad.Vertices[ 2 * quadIndex + 0 ].Position = vertex1 + UVOffset -
                    18.0f * Vector3.Normalize( vertex1 - lightPosVec3 );
                quad.Vertices[ 2 * quadIndex + 0 ].Color = shadowColor.ToArgb();

                quad.Vertices[ 2 * quadIndex + 1 ].Position = vertex1 + ExtrudeMagnitude * ( vertex1 - lightPosVec3 )
                     + UVOffset;
                quad.Vertices[ 2 * quadIndex + 1 ].Color = shadowColor.ToArgb();

                quad.Vertices[ 2 * quadIndex + 2 ].Position = vertex2 + UVOffset -
                    18.0f * Vector3.Normalize( vertex2 - lightPosVec3 );
                quad.Vertices[ 2 * quadIndex + 2 ].Color = shadowColor.ToArgb();

                quad.Vertices[ 2 * quadIndex + 3 ].Position = vertex2 + ExtrudeMagnitude * ( vertex2 - lightPosVec3 )
                    + UVOffset;
                quad.Vertices[ 2 * quadIndex + 3 ].Color = shadowColor.ToArgb();

                shadow.Quads.Add( quad );
            }

            shadows.Add( shadow );
        }
        #endregion

        #region Shadow rendering
        public static void RenderAllShadows()
        {
            if ( !initialized )
            {
                Log.Write( "ShadowManager.RenderAllShadows() called before calling ShadowManager.Initialize()." +
                    "Shadows will not be rendered." );
                return;
            }

            Matrix oldWorld = renderer.WorldMatrix;
            Matrix oldView = renderer.ViewMatrix;

            renderer.WorldMatrix = Matrix.Identity;

            renderer.SaveRenderTarget();

            SetEffectParameters();

            DoShadowGeometryPass();

            renderer.ViewMatrix = Matrix.Identity;
            shadowEffect.SetValue( "worldViewProj", renderer.WorldViewProjectionMatrix );
            shadowEffect.CommitChanges();

            DoBlurHorizontalPass();
            DoBlurVerticalPass();
            DoLightingPass();

            shadowEffect.SetValue( "viewProj", renderer.ViewMatrix * renderer.ProjectionMatrix );
            DoFinalizePass();

            renderer.WorldMatrix = oldWorld;
            renderer.ViewMatrix = oldView;

            shadows.Clear();
        }

        private static void SetEffectParameters()
        {
            shadowEffect.SetValue( "blur", blur );
            shadowEffect.SetValue( "world", renderer.WorldMatrix );
            shadowEffect.SetValue( "worldViewProj", renderer.WorldViewProjectionMatrix );

            Vector4[] lightPoss = new Vector4[ 4 ];
            Vector4[] lightColor = new Vector4[ 4 ];
            float[] range = new float[ 4 ];

            for ( int i = 0; i < renderer.Lights.Count && i < 4; ++i )
            {
                lightPoss[ i ] = new Vector4( renderer.Lights[ i ].Position.X,
                    renderer.Lights[ i ].Position.Y, 1.0f, 1.0f );
                lightColor[ i ] = new Vector4( ( float )renderer.Lights[ i ].Color.R / 255.0f,
                    ( float )renderer.Lights[ i ].Color.G / 255.0f,
                    ( float )renderer.Lights[ i ].Color.B / 255.0f,
                    ( float )renderer.Lights[ i ].Color.A / 255.0f );
                range[ i ] = renderer.Lights[ i ].Range;
            }

            shadowEffect.SetValue( "lightPos", lightPoss );
            shadowEffect.SetValue( "lightColor", lightColor );
            shadowEffect.SetValue( "range", range );
            shadowEffect.SetValue( "numActiveLights", renderer.Lights.Count );
        }

        private static void DoShadowGeometryPass()
        {
            shadowTex.SetAsRenderTarget();
            renderer.Clear( Direct3D.ClearFlags.Target, Color.FromArgb( 0, 0, 0, 0 ), 1.0f, 0 );

            renderer.Begin( shadowEffect );
            renderer.SetPass( 0 );
            renderer.Device.VertexFormat = Direct3D.CustomVertex.PositionColoredTextured.Format;

            foreach ( Shadow shadow in shadows )
            {
                foreach ( Quad quad in shadow.Quads )
                {
                    renderer.Device.DrawUserPrimitives( Direct3D.PrimitiveType.TriangleStrip, 2, quad.Vertices );
                }
            }
        }

        private static void DoBlurHorizontalPass()
        {
            blurHorizontalTex.SetAsRenderTarget();
            renderer.Clear( Direct3D.ClearFlags.Target, Color.FromArgb( 0, 0, 0, 0 ), 1.0f, 0 );

            shadowEffect.SetValue( "shadowTex", shadowTex );
            renderer.SetPass( 1 );
            fullscreenQuad.Render();
        }

        private static void DoBlurVerticalPass()
        {
            blurVerticalTex.SetAsRenderTarget();
            renderer.Clear( Direct3D.ClearFlags.Target, Color.FromArgb( 0, 0, 0, 0 ), 1.0f, 0 );

            shadowEffect.SetValue( "shadowTex", blurHorizontalTex );
            renderer.SetPass( 2 );
            fullscreenQuad.Render();
        }

        private static void DoLightingPass()
        {
            finalTex.SetAsRenderTarget();
            renderer.Clear( Direct3D.ClearFlags.Target, Color.FromArgb( 0, 0, 0, 0 ), 1.0f, 0 );

            shadowEffect.SetValue( "shadowTex", blurVerticalTex );
            renderer.SetPass( 3 );
            fullscreenQuad.Render();
        }

        private static void DoFinalizePass()
        {
            renderer.RestoreRenderTarget();

            shadowEffect.SetValue( "shadowTex", finalTex );
            renderer.SetPass( 4 );
            fullscreenQuad.Render();

            renderer.End();
        }
        #endregion
    }
}
