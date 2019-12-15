using System;
using System.Collections.Generic;
using System.Numerics;

using MoonSharp.Interpreter;
using Museclone.Charting;
using theori;
using theori.Charting;
using theori.Charting.Playback;
using theori.Graphics;
using theori.Graphics.OpenGL;
using theori.Resources;

namespace Museclone.Graphics
{
    public sealed class Highway : Disposable
    {
        private readonly RandomAccessChartWindow m_window;
        private readonly ClientResourceManager m_resources;

        private EntityDrawable3DStaticResources m_entity3dResources;

        private Chart m_chart;
        public Chart Chart
        {
            get => m_chart;
            set
            {
                if (value == m_chart) return;

                m_chart = value;
                m_window.Chart = value;
            }
        }

        [MoonSharpHidden]
        public readonly BasicCamera Camera = new BasicCamera();

        [MoonSharpHidden]
        public (int X, int Y, int Size) Viewport { get; set; } = ((int)(Window.Width - Window.Height * 0.95f) / 2, 0, (int)(Window.Height * 0.95f));

        public time_t Position
        {
            get => m_window.Position;
            set => m_window.Position = value;
        }

        public time_t LookAhead
        {
            get => m_window.LookAhead;
            set => m_window.LookAhead = value;
        }

        public time_t LookBehind
        {
            get => m_window.LookBehind;
            set => m_window.LookBehind = value;
        }

        [MoonSharpHidden]
        public time_t StartTime => m_window.Position - m_window.LookBehind;
        [MoonSharpHidden]
        public time_t EndTime => m_window.Position + m_window.LookAhead;
        [MoonSharpHidden]
        public time_t TotalTime => m_window.LookBehind + m_window.LookAhead;

        public bool LanesHaveDepth { get; set; } = false;

        public float Length { get; set; } = 10;
        public float Depth { get; set; } = 0.15f;

        [MoonSharpHidden]
        public Transform WorldTransform { get; private set; }

        private Drawable3D? m_flatHighwayDrawable;
        
        private Drawable3D? m_tickMeasureDrawable, m_tickBeatDrawable;

        private readonly Drawable3D[] m_laneLines = new Drawable3D[5];
        private Drawable3D? m_highwayBowl;
        
        private readonly Dictionary<HybridLabel, Dictionary<Entity, EntityDrawable3D>> m_entityDrawables = new Dictionary<HybridLabel, Dictionary<Entity, EntityDrawable3D>>();

        public Highway(ClientResourceLocator locator, Chart chart)
        {
            m_chart = chart;
            m_resources = new ClientResourceManager(locator);

            m_window = new RandomAccessChartWindow(chart);
            Camera.SetPerspectiveFoV(120, 1, 0.01f, 60);

            m_window.EntityEnter += Window_EntityEnter;
            m_window.EntityExit += Window_EntityExit;
        }

        private Dictionary<Entity, EntityDrawable3D> GetDrawables(HybridLabel laneLabel)
        {
            if (!m_entityDrawables.TryGetValue(laneLabel, out var drawables))
                drawables = m_entityDrawables[laneLabel] = new Dictionary<Entity, EntityDrawable3D>();
            return drawables;
        }

        private void Window_EntityEnter(HybridLabel laneLabel, Entity entity)
        {
            var isPedal = laneLabel == 5;

            var dParams = new MaterialParams();
            dParams["Color"] = isPedal ? new Vector4(0.5f, 0.5f, 0.5f, 1) : new Vector4(1, 1, 1, 1);

            var drawables = GetDrawables(laneLabel);
#if false
            var drawable = GetDrawables(laneLabel)[entity] = new Drawable3D()
            {
                Texture = Texture.Empty,
                Material = m_resources.AquireMaterial("materials/basic"),
                Mesh = Mesh.CreatePlane(Vector3.UnitX, Vector3.UnitZ, isPedal ? 0.45f : 0.175f, 1, Anchor.BottomCenter),
                Params = dParams,
            };
            m_resources.Manage(drawable.Mesh);
#endif

            if (isPedal)
            {

            }
            else if (entity is ButtonEntity button)
            {
                if (button.IsInstant)
                    drawables[entity] = new ButtonChipRenderState3D(button, m_resources, m_entity3dResources);
                else drawables[entity] = new ButtonHoldRenderState3D(button, m_resources, m_entity3dResources);
            }

        }

        private void Window_EntityExit(HybridLabel laneLabel, Entity entity)
        {
            GetDrawables(laneLabel).Remove(entity);
        }

        public bool AsyncLoad()
        {
            m_resources.QueueTextureLoad("textures/game/buttonHead");
            m_resources.QueueTextureLoad("textures/game/buttonHold");

            if (!m_resources.LoadAll())
                return false;

            return true;
        }

        public bool AsyncFinalize()
        {
            if (!m_resources.FinalizeLoad())
                return false;

            m_entity3dResources = new EntityDrawable3DStaticResources();

            var flatHighwayParams = new MaterialParams();
            flatHighwayParams["Color"] = new Vector4(1, 1, 1, 0.05f);

            m_flatHighwayDrawable = new Drawable3D()
            {
                Texture = Texture.Empty,
                Material = m_resources.AquireMaterial("materials/basic"),
                Mesh = Mesh.CreatePlane(Vector3.UnitX, Vector3.UnitZ, 1, 1, Anchor.BottomCenter),
                Params = flatHighwayParams,
            };
            m_resources.Manage(m_flatHighwayDrawable.Mesh);

            var tickMeasureParams = new MaterialParams();
            tickMeasureParams["Color"] = new Vector4(1, 1, 0.5f, 0.7f);

            var tickBeatParams = new MaterialParams();
            tickBeatParams["Color"] = new Vector4(1, 1, 1, 0.25f);

            var tickMesh = new Mesh();
            tickMesh.SetIndices(0, 1, 2, 2, 1, 3,
                                2, 3, 4, 4, 3, 5,
                                4, 5, 6, 6, 5, 7,
                                6, 7, 8, 8, 7, 9);

            tickMesh.SetVertices(new[]
            {
                new VertexP3T2(new Vector3(-0.50f,  0,  0), Vector2.Zero),
                new VertexP3T2(new Vector3(-0.50f,  0, -1), Vector2.Zero),

                new VertexP3T2(new Vector3(-0.25f, -1,  0), Vector2.Zero),
                new VertexP3T2(new Vector3(-0.25f, -1, -1), Vector2.Zero),

                new VertexP3T2(new Vector3( 0.00f,  0,  0), Vector2.Zero),
                new VertexP3T2(new Vector3( 0.00f,  0, -1), Vector2.Zero),

                new VertexP3T2(new Vector3( 0.25f, -1,  0), Vector2.Zero),
                new VertexP3T2(new Vector3( 0.25f, -1, -1), Vector2.Zero),

                new VertexP3T2(new Vector3( 0.50f,  0,  0), Vector2.Zero),
                new VertexP3T2(new Vector3( 0.50f,  0, -1), Vector2.Zero),
            });
            m_resources.Manage(tickMesh);

            m_tickMeasureDrawable = new Drawable3D()
            {
                Texture = Texture.Empty,
                Material = m_resources.AquireMaterial("materials/basic"),
                Mesh = tickMesh,
                Params = tickMeasureParams,
            };

            m_tickBeatDrawable = new Drawable3D()
            {
                Texture = Texture.Empty,
                Material = m_resources.AquireMaterial("materials/basic"),
                Mesh = tickMesh,
                Params = tickBeatParams,
            };

            var defaultLaneLineParams = new MaterialParams();
            defaultLaneLineParams["Color"] = new Vector4(1, 0.1f, 0.05f, 1);

            for (int i = 0; i < 5; i++)
            {
                m_laneLines[i] = new Drawable3D()
                {
                    Texture = Texture.Empty,
                    Material = m_resources.AquireMaterial("materials/basic"),
                    Mesh = Mesh.CreatePlane(Vector3.UnitX, Vector3.UnitZ, 0.01f, 1, Anchor.BottomCenter),
                    Params = defaultLaneLineParams,
                };
                m_resources.Manage(m_laneLines[i].Mesh);
            }

            var bowlMesh = new Mesh();
            bowlMesh.SetIndices(0, 1, 2, 2, 1, 3,
                                2, 3, 4, 4, 3, 5,
                                4, 5, 6, 6, 5, 7);

            bowlMesh.SetVertices(new[]
            {
                new VertexP3T2(new Vector3(-0.50f,  0,  0), Vector2.Zero),
                new VertexP3T2(new Vector3(-0.50f,  0, -1), Vector2.Zero),
                new VertexP3T2(new Vector3(-0.25f, -1,  0), Vector2.Zero),
                new VertexP3T2(new Vector3(-0.25f, -1, -1), Vector2.Zero),

                new VertexP3T2(new Vector3( 0.25f, -1,  0), Vector2.Zero),
                new VertexP3T2(new Vector3( 0.25f, -1, -1), Vector2.Zero),
                new VertexP3T2(new Vector3( 0.50f,  0,  0), Vector2.Zero),
                new VertexP3T2(new Vector3( 0.50f,  0, -1), Vector2.Zero),
            });

            var bowlParams = new MaterialParams();
            bowlParams["Color"] = new Vector4(1, 1, 1, 0.1f);

            m_highwayBowl = new Drawable3D()
            {
                Texture = Texture.Empty,
                Material = m_resources.AquireMaterial("materials/basic"),
                Mesh = bowlMesh,
                Params = bowlParams,
            };
            m_resources.Manage(bowlMesh);

            m_window.Refresh();
            return true;
        }

        public void SetViewport(float x, float y, float size) => Viewport = ((int)x, (int)y, (int)size);

        [MoonSharpHidden]
        public Vector2 Project(Transform worldTransform, Vector3 worldPosition)
        {
            var p = Camera.ProjectNormalized(worldTransform, worldPosition) * new Vector2(Viewport.Size * 2);
            return new Vector2(Viewport.X - Viewport.Size / 2 + p.X, Viewport.Y - Viewport.Size / 2 + p.Y);
        }

        public void Update()
        {
            Camera.ViewportWidth = Window.Width;
            Camera.ViewportHeight = Window.Height;

            static Transform GetTransform()
            {
                const float ANCHOR_ROT = 2.75f;
                const float ANCHOR_Y = -0.85f;
                const float CONTNR_Z = -0.45f;

                var anchor = Transform.RotationX(ANCHOR_ROT)
                           * Transform.Translation(0, ANCHOR_Y, 0);
                var contnr = Transform.Translation(0, 0, 0)
                           * Transform.Translation(0, 0, CONTNR_Z);

                return contnr * anchor;
            }

            WorldTransform = GetTransform();

            var critDir = Vector3.Normalize(((Matrix4x4)WorldTransform).Translation);
            float rotToCrit = MathL.Atan(critDir.Y, -critDir.Z);

            float cameraRot = Camera.FieldOfView * 0.32f;
            float cameraPitch = rotToCrit + MathL.ToRadians(cameraRot);

            Camera.Position = Vector3.Zero;
            Camera.Rotation = Quaternion.CreateFromYawPitchRoll(0, cameraPitch, 0);

            static Vector3 V3Project(Vector3 a, Vector3 b) => b * (Vector3.Dot(a, b) / Vector3.Dot(b, b));
            static float SignedDistance(Vector3 point, Vector3 ray)
            {
                Vector3 projected = V3Project(point, ray);
                return MathL.Sign(Vector3.Dot(ray, projected)) * projected.Length();
            }

            Vector3 cameraForward = Vector3.Transform(new Vector3(0, 0, -1), Camera.Rotation);
            float bottomClipDistance = SignedDistance(Vector3.Transform(Vector3.Zero, WorldTransform.Matrix) - Camera.Position, cameraForward);
            float topClipDistance = SignedDistance(Vector3.Transform(new Vector3(0, 0, -Length), WorldTransform.Matrix) - Camera.Position, cameraForward);

            float minClipDist = Math.Min(bottomClipDistance, topClipDistance);
            float maxClipDist = Math.Max(bottomClipDistance, topClipDistance);

            float clipNear = Math.Max(0.01f, minClipDist - 0.12f);
            float clipFar = maxClipDist;

            // TODO(local): see if the default epsilon is enough? There's no easy way to check clip planes manually right now
            if (clipNear.ApproxEq(clipFar))
                clipFar = clipNear + 0.001f;

            Camera.NearDistance = clipNear;
            Camera.FarDistance = clipFar;
        }

        public void Render()
        {
            var renderState = new RenderState
            {
                Viewport = (Viewport.X - Viewport.Size / 2, -Window.Height + (Viewport.Y - Viewport.Size / 2) + Viewport.Size * 2, Viewport.Size * 2, Viewport.Size * 2),
                ProjectionMatrix = Camera.ProjectionMatrix,
                CameraMatrix = Camera.ViewMatrix,
            };

            using var queue = new RenderQueue(renderState);

            //m_flatHighwayDrawable!.DrawToQueue(queue, Transform.Scale(1, 1, Length) * WorldTransform);
            m_highwayBowl!.DrawToQueue(queue, Transform.Scale(1, Depth, Length) * WorldTransform);

            float[] yVals = new float[6];
            for (int i = 0; i < 5; i++)
                yVals[i] = (i % 2) == 0 ? 0 : (LanesHaveDepth ? -Depth : 0);
            yVals[5] = -Depth;

            void DrawLane(HybridLabel label)
            {
                var entities = GetDrawables(label);
                foreach (var (entity, drawable) in entities)
                {
                    int i = (int)label;
                    float posX = i == 5 ? 0 : (i / 4.0f) - 0.5f;

                    float ePos = -(float)((entity.AbsolutePosition - StartTime) / TotalTime) * Length;
                    float eLen = entity.IsInstant ? 0 : (float)(entity.AbsoluteDuration / TotalTime) * Length;

                    drawable.Render(queue, Transform.Translation(posX, yVals[i], ePos) * WorldTransform, eLen);
                }
            }

            void DrawTick(time_t when, bool measure)
            {
                float pos = -(float)((when - StartTime) / TotalTime) * Length;
                (measure ? m_tickMeasureDrawable : m_tickBeatDrawable).DrawToQueue(queue, Transform.Scale(1, (LanesHaveDepth ? Depth : 0), 0.01f) * Transform.Translation(0, 0, pos) * WorldTransform);
            }

            DrawLane(5);

            tick_t startTicks = Chart.CalcTickFromTime(StartTime);
            tick_t maxTick = Chart.CalcTickFromTime(EndTime);

            var currentPoint = Chart.ControlPoints.MostRecent(startTicks);
            tick_t ticksPerBeat = 1.0 / currentPoint.BeatCount;

            tick_t currentTick = MathL.Floor(0.5 + ((double)startTicks / (double)ticksPerBeat)) * ticksPerBeat;
            if (currentTick < startTicks)
                currentTick += ticksPerBeat;

            while (currentTick < maxTick)
            {
                DrawTick(Chart.CalcTimeFromTick(currentTick), ((double)currentTick % 1) == 0);
                currentTick += ticksPerBeat;
            }

            for (int i = 0; i < 5; i++)
            {
                float posX = (i / 4.0f) - 0.5f;
                m_laneLines[i].DrawToQueue(queue, Transform.Scale(1, 1, Length) * Transform.Translation(posX, yVals[i], 0) * WorldTransform);
            }

            for (int i = 0; i < 5; i++)
                DrawLane(i);
        }
    }
}
