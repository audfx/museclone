using System;
using System.Collections.Generic;
using System.Numerics;

using MoonSharp.Interpreter;

using theori;
using theori.Charting;
using theori.Charting.Playback;
using theori.Graphics;
using theori.Graphics.OpenGL;
using theori.Resources;

using Museclone.Charting;

namespace Museclone.Graphics
{
    public sealed class Highway : Disposable
    {
        private const float BehindMult = 0.1f;

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
            set => m_window.LookBehind = BehindMult * (m_window.LookAhead = value);
        }

        public time_t LookBehind => m_window.LookBehind;

        [MoonSharpHidden]
        public time_t StartTime => m_window.Position - LookBehind;
        [MoonSharpHidden]
        public time_t EndTime => m_window.Position + m_window.LookAhead;
        [MoonSharpHidden]
        public time_t TotalTime => m_window.LookBehind + m_window.LookAhead;

        public bool LanesHaveDepth { get; set; } = false;

        public float Length { get; set; } = 12;
        public float Depth { get; set; } = 0.25f;

        [MoonSharpHidden]
        public Transform WorldTransform { get; private set; }

        private Drawable3D? m_flatHighwayDrawable;
        
        private Drawable3D? m_tickMeasureDrawable, m_tickBeatDrawable;

        private readonly Drawable3D[] m_laneLines = new Drawable3D[5];
        private Drawable3D? m_highwayBowl, m_criticalPoint, m_pedal;
        
        private readonly Dictionary<HybridLabel, Dictionary<Entity, EntityDrawable3D>> m_entityDrawables = new Dictionary<HybridLabel, Dictionary<Entity, EntityDrawable3D>>();
        private readonly Dictionary<(int, int), Drawable3D> m_joiningDrawables = new Dictionary<(int, int), Drawable3D>();

        public Highway(ClientResourceLocator locator, Chart chart)
        {
            m_chart = chart;
            m_resources = new ClientResourceManager(locator);

            m_window = new RandomAccessChartWindow(chart);
            LookAhead = 3;

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

            if (entity is ButtonEntity button)
            {
                if (isPedal)
                    drawables[entity] = new PedalRenderState3D((ButtonEntity)entity, m_resources, m_entity3dResources);
                else
                {
                    if (button.IsInstant)
                        drawables[entity] = new ButtonChipRenderState3D(button, m_resources, m_entity3dResources);
                    else drawables[entity] = new ButtonHoldRenderState3D(button, m_resources, m_entity3dResources);
                }
            }
            else if (entity is SpinnerEntity spinner)
            {
                drawables[entity] = new SpinnerRenderState3D(spinner, m_resources, m_entity3dResources);
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
            m_resources.QueueTextureLoad("textures/game/criticalPoint");
            m_resources.QueueTextureLoad("textures/game/pedal");
            m_resources.QueueTextureLoad("textures/game/pedalHead");
            m_resources.QueueTextureLoad("textures/game/pedalHold");
            m_resources.QueueTextureLoad("textures/game/spinner");
            m_resources.QueueTextureLoad("textures/game/spinnerArrow");

            m_resources.QueueMaterialLoad("materials/basic");
            m_resources.QueueMaterialLoad("materials/hold");

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
                new VertexP3T2(new Vector3(-0.50f,  0,  1), Vector2.Zero),
                new VertexP3T2(new Vector3(-0.50f,  0, -1), Vector2.Zero),

                new VertexP3T2(new Vector3(-0.25f, -1,  1), Vector2.Zero),
                new VertexP3T2(new Vector3(-0.25f, -1, -1), Vector2.Zero),

                new VertexP3T2(new Vector3( 0.00f,  0,  1), Vector2.Zero),
                new VertexP3T2(new Vector3( 0.00f,  0, -1), Vector2.Zero),

                new VertexP3T2(new Vector3( 0.25f, -1,  1), Vector2.Zero),
                new VertexP3T2(new Vector3( 0.25f, -1, -1), Vector2.Zero),

                new VertexP3T2(new Vector3( 0.50f,  0,  1), Vector2.Zero),
                new VertexP3T2(new Vector3( 0.50f,  0, -1), Vector2.Zero),
            });
            m_resources.Manage(tickMesh);

            m_tickMeasureDrawable = new Drawable3D()
            {
                Texture = Texture.Empty,
                Material = m_resources.GetMaterial("materials/basic"),
                Mesh = tickMesh,
                Params = tickMeasureParams,
            };

            m_tickBeatDrawable = new Drawable3D()
            {
                Texture = Texture.Empty,
                Material = m_resources.GetMaterial("materials/basic"),
                Mesh = tickMesh,
                Params = tickBeatParams,
            };

            var topLaneLineParams = new MaterialParams();
            topLaneLineParams["Color"] = new Vector4(1, 0.1f, 0.05f, 1);
            var bottomLaneLineParams = new MaterialParams();
            bottomLaneLineParams["Color"] = new Vector4(0.5f, 0.05f, 0.025f, 1);

            for (int i = 0; i < 5; i++)
            {
                m_laneLines[i] = new Drawable3D()
                {
                    Texture = Texture.Empty,
                    Material = m_resources.GetMaterial("materials/basic"),
                    Mesh = Mesh.CreatePlane(Vector3.UnitX, Vector3.UnitZ, 0.01f, 1, Anchor.BottomCenter),
                    Params = i % 2 == 0 ? topLaneLineParams : bottomLaneLineParams,
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
            bowlParams["Color"] = new Vector4(0.3f, 0.3f, 0.3f, 0.5f);

            m_highwayBowl = new Drawable3D()
            {
                Texture = Texture.Empty,
                Material = m_resources.GetMaterial("materials/basic"),
                Mesh = bowlMesh,
                Params = bowlParams,
            };
            m_resources.Manage(bowlMesh);

            var critParams = new MaterialParams();
            critParams["Color"] = new Vector4(1, 0.1f, 0.05f, 1);

            m_criticalPoint = new Drawable3D()
            {
                Texture = m_resources.GetTexture("textures/game/criticalPoint"),
                Material = m_resources.GetMaterial("materials/basic"),
                Mesh = Mesh.CreatePlane(Vector3.UnitX, Vector3.UnitZ, 1, 1),
                Params = critParams,
            };
            m_resources.Manage(m_criticalPoint.Mesh);

            var pedalParams = new MaterialParams();
            pedalParams["Color"] = new Vector4(1, 1, 1, 1);

            m_pedal = new Drawable3D()
            {
                Texture = m_resources.GetTexture("textures/game/pedal"),
                Material = m_resources.GetMaterial("materials/basic"),
                Mesh = Mesh.CreatePlane(Vector3.UnitX, Vector3.UnitZ, 1, 1, Anchor.TopCenter),
                Params = pedalParams,
            };
            m_resources.Manage(m_pedal.Mesh);

            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 5; j++)
                {
                    static float GetXPos(int laneIndex) => laneIndex / 4.0f - 0.5f;
                    static float GetYPos(int laneIndex) => laneIndex % 2 == 0 ? 0 : -1;

                    float lx = GetXPos(i), rx = GetXPos(j);
                    float ly = GetYPos(i), ry = GetYPos(j);

                    var mesh = new Mesh();
                    mesh.SetIndices(0, 1, 2, 2, 1, 3);

                    mesh.SetVertices(new[]
                    {
                        new VertexP3T2(new Vector3(lx, ly,  1), new Vector2()),
                        new VertexP3T2(new Vector3(lx, ly, -1), new Vector2()),
                        new VertexP3T2(new Vector3(rx, ry,  1), new Vector2()),
                        new VertexP3T2(new Vector3(rx, ry, -1), new Vector2()),
                    });

                    var joinParams = new MaterialParams();
                    joinParams["Color"] = new Vector4(1, 0.1f, 0.05f, 1);

                    m_joiningDrawables[(i, j)] = new Drawable3D()
                    {
                        Texture = Texture.Empty,
                        Material = m_resources.GetMaterial("materials/basic"),
                        Mesh = mesh,
                        Params = joinParams,
                    };
                    m_resources.Manage(mesh);
                }
            }

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
                const float ANCHOR_Y = -0.8f;
                const float CONTNR_Z = -0.225f;
                return Transform.Translation(0, ANCHOR_Y, CONTNR_Z);
            }

            WorldTransform = GetTransform();

            var critDir = Vector3.Normalize(((Matrix4x4)WorldTransform).Translation);
            float rotToCrit = MathL.Atan(critDir.Y, -critDir.Z);

            float cameraRot = Camera.FieldOfView * 0.275f;
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
            float bottomClipDistance = SignedDistance(Vector3.Transform(new Vector3(0, 0, Length * BehindMult), WorldTransform.Matrix) - Camera.Position, cameraForward);
            float topClipDistance = SignedDistance(Vector3.Transform(new Vector3(0, 0, -Length), WorldTransform.Matrix) - Camera.Position, cameraForward);

            float minClipDist = Math.Min(bottomClipDistance, topClipDistance);
            float maxClipDist = Math.Max(bottomClipDistance, topClipDistance);

            float clipNear = Math.Max(0.01f, minClipDist);
            float clipFar = maxClipDist;

            // TODO(local): see if the default epsilon is enough? There's no easy way to check clip planes manually right now
            if (clipNear.ApproxEq(clipFar))
                clipFar = clipNear + 0.001f;

            Camera.NearDistance = clipNear;
            Camera.FarDistance = clipFar;
        }

        private readonly Dictionary<tick_t, List<Entity>> m_joinedEntities = new Dictionary<tick_t, List<Entity>>();

        public void Render()
        {
            var renderState = new RenderState
            {
                Viewport = (Viewport.X - Viewport.Size / 2, -Window.Height + (Viewport.Y - Viewport.Size / 2) + Viewport.Size * 2, Viewport.Size * 2, Viewport.Size * 2),
                ProjectionMatrix = Camera.ProjectionMatrix,
                CameraMatrix = Camera.ViewMatrix,
            };

            using var queue = new RenderQueue(renderState);
            queue.DepthFunction = null;

            //m_flatHighwayDrawable!.DrawToQueue(queue, Transform.Scale(1, 1, Length) * WorldTransform);
            m_highwayBowl!.DrawToQueue(queue, Transform.Scale(1, Depth, Length) * WorldTransform);

            float[] yVals = new float[6];
            for (int i = 0; i < 5; i++)
                yVals[i] = (i % 2) == 0 ? 0 : (LanesHaveDepth ? -Depth : 0);
            yVals[5] = -Depth;

            var allEntities = new List<(HybridLabel Label, Entity Entity, EntityDrawable3D Drawable)>();
            void GatherLaneEntities(HybridLabel label)
            {
                var entities = GetDrawables(label);
                foreach (var (entity, drawable) in entities)
                    allEntities.Add((label, entity, drawable));
                allEntities.Sort((a, b) => MathL.Sign((double)b.Entity.AbsoluteEndPosition - (double)a.Entity.AbsoluteEndPosition));
            }

            void DrawLaneEntities()
            {
                foreach (var (label, entity, drawable) in allEntities)
                {
                    int i = (int)label;
                    float posX = i == 5 ? 0 : (i / 4.0f) - 0.5f;

                    float ePos = -(float)((entity.AbsolutePosition - Position) / LookAhead) * Length;
                    float eLen = entity.IsInstant ? 0 : (float)(entity.AbsoluteDuration / TotalTime) * Length;

                    float billboardPos = drawable is HoldRenderState3D ? Math.Min(-0.0001f, ePos) : ePos;
                    var worldPos = Vector3.Transform(new Vector3(posX, yVals[i], billboardPos), WorldTransform.Matrix);
                    var cameraPos = Camera.Position;
                    var cameraUp = Camera.Up;
                    var cameraForward = Camera.Forward;

                    //var lookAtTransform = new Transform(Matrix4x4.CreateLookAt(worldPos, cameraPos, cameraUp));
                    //var lookAtTransform = new Transform(Matrix4x4.CreateConstrainedBillboard(worldPos, cameraPos, Vector3.UnitX, cameraForward, Vector3.UnitZ));
                    var lookAtTransform = new Transform(Matrix4x4.CreateBillboard(worldPos, worldPos - cameraForward, cameraUp, cameraForward));

                    //drawable.SetHeadPosition(Transform.Translation(posX, yVals[i], Math.Min(0, ePos)) * WorldTransform);
                    drawable.SetBillboard(lookAtTransform);
                    if (drawable is SpinnerRenderState3D spin)
                        spin.Spin = 2 * (float)(entity.Position - Chart.CalcTickFromTime(Position));
                    if (drawable is HoldRenderState3D hold)
                    {
                        hold.HeadPosition = ePos;
                        hold.Completion = (float)MathL.Clamp01((double)(Position - entity.AbsolutePosition) / (double)entity.AbsoluteDuration);
                        drawable.Render(queue, Transform.Translation(posX, yVals[i], 0) * WorldTransform, eLen);
                    }
                    else drawable.Render(queue, Transform.Translation(posX, yVals[i], ePos) * WorldTransform, eLen);
                }

                allEntities.Clear();
            }

            void DrawTick(time_t when, bool measure)
            {
                float pos = -(float)((when - Position) / LookAhead) * Length;
                (measure ? m_tickMeasureDrawable : m_tickBeatDrawable).DrawToQueue(queue, Transform.Scale(1, (LanesHaveDepth ? Depth : 0), 0.0035f) * Transform.Translation(0, 0, pos) * WorldTransform);
            }

            GatherLaneEntities(5);
            DrawLaneEntities();

            m_pedal.DrawToQueue(queue, Transform.Scale(0.675f, 1, 0.1f) * Transform.Translation(0, -Depth, 0) * WorldTransform);

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
                m_criticalPoint.DrawToQueue(queue, Transform.Scale(0.2f, 1, 0.15f) * Transform.Translation(posX, yVals[i], 0) * WorldTransform);
            }

            for (int i = 0; i < 5; i++)
            {
                foreach (var (entity, _) in GetDrawables(i))
                {
                    if (!m_joinedEntities.TryGetValue(entity.Position, out var entities))
                        entities = m_joinedEntities[entity.Position] = new List<Entity>();
                    entities.Add(entity);
                }
            }

            foreach (var (when, entities) in m_joinedEntities)
            {
                if (entities.Count <= 1) continue;

                entities.Sort((a, b) =>
                {
                    if (a.Lane == 0 || a.Lane == 2 || a.Lane == 4)
                    {
                        if (b.Lane != 0 && b.Lane != 2 && b.Lane != 4)
                            return 1;
                    }
                    else
                    {
                        if (b.Lane == 0 || b.Lane == 2 || b.Lane == 4)
                            return -1;
                    }
                    return (int)b.Lane - (int)a.Lane;
                });
                for (int i = 0; i < entities.Count; i++)
                {
                    int j = i + 1;
                    if (j >= entities.Count) j = 0;

                    if (j == 0 && entities.Count == 2) break;
                    int min = (int)entities[i].Lane, max = (int)entities[j].Lane;

                    if (min > max)
                    {
                        int temp = min;
                        min = max;
                        max = temp;
                    }

                    float ePos = -(float)((entities[i].AbsolutePosition - Position) / LookAhead) * Length;
                    m_joiningDrawables[(min, max)].DrawToQueue(queue, Transform.Scale(1, LanesHaveDepth ? Depth : 0, 0.005f) * Transform.Translation(0, 0, ePos + 0.0225f) * WorldTransform);
                    m_joiningDrawables[(min, max)].DrawToQueue(queue, Transform.Scale(1, LanesHaveDepth ? Depth : 0, 0.005f) * Transform.Translation(0, 0, ePos) * WorldTransform);
                    m_joiningDrawables[(min, max)].DrawToQueue(queue, Transform.Scale(1, LanesHaveDepth ? Depth : 0, 0.005f) * Transform.Translation(0, 0, ePos - 0.0225f) * WorldTransform);
                }
            }
            m_joinedEntities.Clear();

            //queue.DepthFunction = DepthFunction.LessThanOrEqual;
            for (int i = 0; i < 5; i++)
                GatherLaneEntities(i);
            DrawLaneEntities();
        }
    }
}
