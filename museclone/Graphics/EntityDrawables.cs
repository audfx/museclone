using System;
using System.Diagnostics;
using System.Numerics;

using theori;
using theori.Charting;
using theori.Graphics;
using theori.Resources;

using Museclone.Charting;
using System.Collections.Generic;

namespace Museclone.Graphics
{
    internal sealed class EntityDrawable3DStaticResources : Disposable
    {
        public readonly Mesh ButtonHeadMesh;
        public readonly Mesh ButtonHoldMesh;

        public readonly Mesh PedalHeadMesh;
        public readonly Mesh PedalHoldMesh;

        public readonly Mesh SmallSpinnerMesh;
        public readonly Mesh LargeSpinnerMesh;
        public readonly Mesh LargeSpinnerDurationMesh;
        public readonly Mesh SpinnerArrowMesh;

        public EntityDrawable3DStaticResources()
        {
            ButtonHeadMesh = new Mesh();
            ButtonHeadMesh.SetIndices(0, 1, 2, 0, 2, 3);
            ButtonHeadMesh.SetVertices(new[]
            {
                new VertexP3T2(new Vector3(-1,  0, 0), new Vector2(0, 0.5f)),
                new VertexP3T2(new Vector3( 0,  1, 0), new Vector2(0.5f, 0)),
                new VertexP3T2(new Vector3( 1,  0, 0), new Vector2(1, 0.5f)),
                new VertexP3T2(new Vector3( 0, -1, 0), new Vector2(0.5f, 1)),
            });

            ButtonHoldMesh = new Mesh();
            ButtonHoldMesh.SetIndices(0, 1, 2, 2, 1, 3,
                                      2, 3, 4, 4, 3, 5);
            ButtonHoldMesh.SetVertices(new[]
            {
                new VertexP3T2(new Vector3(-1   , 0,  0), new Vector2(0, 1)),
                new VertexP3T2(new Vector3(-0.5f, 0, -1), new Vector2(0, 0)),

                new VertexP3T2(new Vector3( 0, 1   ,  0), new Vector2(0.5f, 1)),
                new VertexP3T2(new Vector3( 0, 0.5f, -1), new Vector2(0.5f, 0)),

                new VertexP3T2(new Vector3( 1   , 0,  0), new Vector2(1, 1)),
                new VertexP3T2(new Vector3( 0.5f, 0, -1), new Vector2(1, 0)),
            });

            PedalHeadMesh = new Mesh();
            PedalHeadMesh.SetIndices(0, 1, 2, 0, 2, 3);
            PedalHeadMesh.SetVertices(new[]
            {
                new VertexP3T2(new Vector3(-1,  0, 0), new Vector2(0, 0)),
                new VertexP3T2(new Vector3( 1,  0, 0), new Vector2(1, 0)),
                new VertexP3T2(new Vector3( 1, -1, 0), new Vector2(1, 1)),
                new VertexP3T2(new Vector3(-1, -1, 0), new Vector2(0, 1)),
            });

            PedalHoldMesh = new Mesh();
            PedalHoldMesh.SetIndices(0, 1, 2, 2, 1, 3);
            PedalHoldMesh.SetVertices(new[]
            {
                new VertexP3T2(new Vector3(-1   , 0,  0), new Vector2(0, 1)),
                new VertexP3T2(new Vector3(-0.5f, 0, -1), new Vector2(0, 0)),

                new VertexP3T2(new Vector3( 1   , 0,  0), new Vector2(1, 1)),
                new VertexP3T2(new Vector3( 0.5f, 0, -1), new Vector2(1, 0)),
            });

            {
                float B = 1.0f / 6;
                var indices = new List<ushort>()
                {
                    // left and right tris, dark
                    0, 1, 2, 0, 3, 4,
                    // front and back tris, light
                    5, 7, 8, 5, 9, 6,
                    // top quad, bright
                    10, 11, 12, 10, 12, 13
                };

                var verticies = new List<VertexP3T2>()
                {
                    new VertexP3T2(new Vector3(0, 0, 0), new Vector2(0.2f, 0.5f)),
                    new VertexP3T2(new Vector3(-B, 2 * B,  0), new Vector2(0.2f, 0.5f)),
                    new VertexP3T2(new Vector3( 0, 2 * B, -B), new Vector2(0.2f, 0.5f)),
                    new VertexP3T2(new Vector3( B, 2 * B,  0), new Vector2(0.2f, 0.5f)),
                    new VertexP3T2(new Vector3( 0, 2 * B,  B), new Vector2(0.2f, 0.5f)),

                    new VertexP3T2(new Vector3(0, 0, 0), new Vector2(0.8f, 0.5f)),
                    new VertexP3T2(new Vector3(-B, 2 * B,  0), new Vector2(0.8f, 0.5f)),
                    new VertexP3T2(new Vector3( 0, 2 * B, -B), new Vector2(0.8f, 0.5f)),
                    new VertexP3T2(new Vector3( B, 2 * B,  0), new Vector2(0.8f, 0.5f)),
                    new VertexP3T2(new Vector3( 0, 2 * B,  B), new Vector2(0.8f, 0.5f)),

                    new VertexP3T2(new Vector3(-B, 2 * B,  0), new Vector2(0.5f, 0.5f)),
                    new VertexP3T2(new Vector3( 0, 2 * B, -B), new Vector2(0.5f, 0.5f)),
                    new VertexP3T2(new Vector3( B, 2 * B,  0), new Vector2(0.5f, 0.5f)),
                    new VertexP3T2(new Vector3( 0, 2 * B,  B), new Vector2(0.5f, 0.5f)),
                };

                for (int i = 0; i < 2; i++)
                {
                    float angle = MathF.PI * (i + 1) / 6.0f;
                    float y1 = B * (3 + i * 2), y2 = y1 + B;
                    float sz = B * (1 + (i + 1) / 2.0f);

                    static void RotatePoint(float rad, ref float x, ref float y)
                    {
                        float sin = MathF.Sin(rad), cos = MathF.Cos(rad);

                        float tx = x;
                        x = x * cos - y * sin;
                        y = tx * sin + y * cos;
                    }

                    float xx = -sz, zz = 0;
                    RotatePoint(-angle, ref xx, ref zz);

                    for (int j = 0; j < 4; j++)
                    {
                        float x = xx, z = zz;
                        float tc = j % 2 == 0 ? 0.5f : 0.8f;
                        int ic = verticies.Count;

                        RotatePoint(-MathF.PI * 0.5f * j, ref x, ref z);
                        verticies.Add(new VertexP3T2(new Vector3(x, y1, z), new Vector2(tc, 0.5f)));
                        verticies.Add(new VertexP3T2(new Vector3(x, y2, z), new Vector2(tc, 0.5f)));
                        RotatePoint(-MathF.PI * 0.5f, ref x, ref z);
                        verticies.Add(new VertexP3T2(new Vector3(x, y1, z), new Vector2(tc, 0.5f)));
                        verticies.Add(new VertexP3T2(new Vector3(x, y2, z), new Vector2(tc, 0.5f)));

                        indices.Add((ushort)(ic + 0));
                        indices.Add((ushort)(ic + 1));
                        indices.Add((ushort)(ic + 2));
                        indices.Add((ushort)(ic + 2));
                        indices.Add((ushort)(ic + 1));
                        indices.Add((ushort)(ic + 3));
                    }
                }

                SmallSpinnerMesh = new Mesh();
                SmallSpinnerMesh.SetIndices(indices.ToArray());
                SmallSpinnerMesh.SetVertices(verticies.ToArray());
            }

            {
                float B = 1.0f / 6;
                var indices = new List<ushort>()
                {
                    // left and right tris, dark
                    0, 1, 2, 0, 3, 4,
                    // front and back tris, light
                    5, 7, 8, 5, 9, 6,
                    // top quad, bright
                    10, 11, 12, 10, 12, 13
                };

                var verticies = new List<VertexP3T2>()
                {
                    new VertexP3T2(new Vector3(0, 0, 0), new Vector2(0.2f, 0.5f)),
                    new VertexP3T2(new Vector3(-B, 2 * B,  0), new Vector2(0.2f, 0.5f)),
                    new VertexP3T2(new Vector3( 0, 2 * B, -B), new Vector2(0.2f, 0.5f)),
                    new VertexP3T2(new Vector3( B, 2 * B,  0), new Vector2(0.2f, 0.5f)),
                    new VertexP3T2(new Vector3( 0, 2 * B,  B), new Vector2(0.2f, 0.5f)),

                    new VertexP3T2(new Vector3(0, 0, 0), new Vector2(0.8f, 0.5f)),
                    new VertexP3T2(new Vector3(-B, 2 * B,  0), new Vector2(0.8f, 0.5f)),
                    new VertexP3T2(new Vector3( 0, 2 * B, -B), new Vector2(0.8f, 0.5f)),
                    new VertexP3T2(new Vector3( B, 2 * B,  0), new Vector2(0.8f, 0.5f)),
                    new VertexP3T2(new Vector3( 0, 2 * B,  B), new Vector2(0.8f, 0.5f)),

                    new VertexP3T2(new Vector3(-B, 2 * B,  0), new Vector2(0.5f, 0.5f)),
                    new VertexP3T2(new Vector3( 0, 2 * B, -B), new Vector2(0.5f, 0.5f)),
                    new VertexP3T2(new Vector3( B, 2 * B,  0), new Vector2(0.5f, 0.5f)),
                    new VertexP3T2(new Vector3( 0, 2 * B,  B), new Vector2(0.5f, 0.5f)),
                };

                for (int i = 0; i < 4; i++)
                {
                    float angle = MathF.PI * (i + 1) / 12.0f;
                    float y1 = B * (3 + i), y2 = y1 + B * 0.5f;
                    float sz = B * (1 + (i + 1) / 2.0f);

                    static void RotatePoint(float rad, ref float x, ref float y)
                    {
                        float sin = MathF.Sin(rad), cos = MathF.Cos(rad);

                        float tx = x;
                        x = x * cos - y * sin;
                        y = tx * sin + y * cos;
                    }

                    float xx = -sz, zz = 0;
                    RotatePoint(-angle, ref xx, ref zz);

                    for (int j = 0; j < 4; j++)
                    {
                        float x = xx, z = zz;
                        float tc = j % 2 == 0 ? 0.5f : 0.8f;
                        int ic = verticies.Count;

                        RotatePoint(-MathF.PI * 0.5f * j, ref x, ref z);
                        verticies.Add(new VertexP3T2(new Vector3(x, y1, z), new Vector2(tc, 0.5f)));
                        verticies.Add(new VertexP3T2(new Vector3(x, y2, z), new Vector2(tc, 0.5f)));
                        RotatePoint(-MathF.PI * 0.5f, ref x, ref z);
                        verticies.Add(new VertexP3T2(new Vector3(x, y1, z), new Vector2(tc, 0.5f)));
                        verticies.Add(new VertexP3T2(new Vector3(x, y2, z), new Vector2(tc, 0.5f)));

                        indices.Add((ushort)(ic + 0));
                        indices.Add((ushort)(ic + 1));
                        indices.Add((ushort)(ic + 2));
                        indices.Add((ushort)(ic + 2));
                        indices.Add((ushort)(ic + 1));
                        indices.Add((ushort)(ic + 3));
                    }
                }

                LargeSpinnerMesh = new Mesh();
                LargeSpinnerMesh.SetIndices(indices.ToArray());
                LargeSpinnerMesh.SetVertices(verticies.ToArray());
            }

            LargeSpinnerDurationMesh = Mesh.CreatePlane(Vector3.UnitX, Vector3.UnitZ, 1, 1, Anchor.BottomCenter);
        }

        protected override void DisposeManaged()
        {
            ButtonHeadMesh.Dispose();
            ButtonHoldMesh.Dispose();

            PedalHoldMesh.Dispose();
        }
    }

    internal abstract class EntityDrawable3D : Disposable
    {
        protected static Vector4 Blue = new Vector4(0.55f, 0.96f, 1.0f, 1);
        protected static Vector4 Yellow = new Vector4(1.0f, 0.975f, 0.575f, 1);

        public readonly Entity Entity;

        protected Transform? billboard;

        protected EntityDrawable3D(Entity entity)
        {
            Entity = entity;
        }

        public virtual void SetBillboard(Transform? billboard) => this.billboard = billboard;

        public abstract void Render(RenderQueue rq, Transform world, float len);
    }

    internal class ButtonChipRenderState3D : EntityDrawable3D
    {
        public new ButtonEntity Entity => (ButtonEntity)base.Entity;

        private readonly float m_size;
        private readonly Drawable3D m_drawable;

        public ButtonChipRenderState3D(ButtonEntity entity, ClientResourceManager resources, EntityDrawable3DStaticResources staticResources)
            : base(entity)
        {
            Debug.Assert(entity.IsInstant, "Hold object passed to render state which expects an instant");

            var buttonParams = new MaterialParams();
            buttonParams["Color"] = ((int)entity.Lane % 2 == 0) ? Blue : Yellow;

            m_size = 1.0f / 12;

            m_drawable = new Drawable3D()
            {
                Texture = resources.GetTexture("textures/game/buttonHead"),
                Material = resources.GetMaterial("materials/basic"),
                Mesh = staticResources.ButtonHeadMesh,
                Params = buttonParams,
            };
        }

        public override void Render(RenderQueue rq, Transform world, float len)
        {
            m_drawable.DrawToQueue(rq, Transform.Scale(m_size, m_size, 1) * (billboard ?? world));
        }
    }

    internal abstract class HoldRenderState3D : EntityDrawable3D
    {
        protected HoldRenderState3D(Entity entity)
            : base(entity)
        {
        }

        public float HeadPosition { get; set; } = 0;
        public float Completion { get; set; } = 0;
    }

    internal class ButtonHoldRenderState3D : HoldRenderState3D
    {
        public new ButtonEntity Entity => (ButtonEntity)base.Entity;

        private readonly float m_size;
        private readonly Drawable3D m_hold, m_head;

        public ButtonHoldRenderState3D(ButtonEntity entity, ClientResourceManager resources, EntityDrawable3DStaticResources staticResources)
            : base(entity)
        {
            Debug.Assert(!entity.IsInstant, "Instant object passed to render state which expects a hold");

            var holdParams = new MaterialParams();
            holdParams["Color"] = ((int)entity.Lane % 2 == 0) ? Blue : Yellow;
            var headParams = new MaterialParams();
            headParams["Color"] = ((int)entity.Lane % 2 == 0) ? Blue : Yellow;

            m_size = 1.0f / 12;

            m_hold = new Drawable3D()
            {
                Texture = resources.GetTexture("textures/game/buttonHold"),
                Material = resources.GetMaterial("materials/hold"),
                Mesh = staticResources.ButtonHoldMesh,
                Params = holdParams,
            };

            m_head = new Drawable3D()
            {
                Texture = resources.GetTexture("textures/game/buttonHead"),
                Material = resources.GetMaterial("materials/basic"),
                Mesh = staticResources.ButtonHeadMesh,
                Params = headParams,
            };
        }

        public override void Render(RenderQueue rq, Transform world, float len)
        {
            float headScale = m_size * (1 - Completion * 0.5f);

            m_hold.Params["Scale"] = len;
            m_hold.Params["HeadPosition"] = HeadPosition;
            m_hold.Params["Completion"] = Completion;

            m_hold.DrawToQueue(rq, Transform.Scale(m_size, m_size, 1) * world);
            rq.Process(true);
            m_head.DrawToQueue(rq, Transform.Scale(headScale, headScale, 1) * (billboard ?? world));
        }
    }

    internal class PedalRenderState3D : HoldRenderState3D
    {
        const float Aspect = 2.0f / 6.0f;

        public new ButtonEntity Entity => (ButtonEntity)base.Entity;

        private readonly float m_size;
        private readonly Drawable3D m_hold, m_head;

        public PedalRenderState3D(ButtonEntity entity, ClientResourceManager resources, EntityDrawable3DStaticResources staticResources)
            : base(entity)
        {
            var holdParams = new MaterialParams();
            holdParams["Color"] = new Vector4(1, 0.5f, 0.5f, 1);
            var headParams = new MaterialParams();
            headParams["Color"] = new Vector4(1, 1, 1, 1);

            m_size = 0.5f * 0.6f;

            m_hold = new Drawable3D()
            {
                Texture = resources.GetTexture("textures/game/pedalHold"),
                Material = resources.GetMaterial("materials/hold"),
                Mesh = staticResources.PedalHoldMesh,
                Params = holdParams,
            };

            m_head = new Drawable3D()
            {
                Texture = resources.GetTexture("textures/game/pedalHead"),
                Material = resources.GetMaterial("materials/basic"),
                Mesh = staticResources.PedalHeadMesh,
                Params = headParams,
            };
        }

        public override void Render(RenderQueue rq, Transform world, float len)
        {
            m_hold.Params["Scale"] = len;
            m_hold.Params["HeadPosition"] = HeadPosition;
            m_hold.Params["Completion"] = Completion;

            m_hold.DrawToQueue(rq, Transform.Scale(m_size, 1, 1) * world);

            if (Completion <= 0)
                m_head.DrawToQueue(rq, Transform.Scale(m_size, m_size * Aspect, 1) * (billboard ?? world));
        }
    }

    internal class SpinnerRenderState3D : HoldRenderState3D
    {
        public new ButtonEntity Entity => (ButtonEntity)base.Entity;

        private readonly float m_size;
        private readonly Drawable3D m_drawable;
        private readonly Drawable3D? m_duration;

        public float Spin { get; set; } = 0;

        public SpinnerRenderState3D(SpinnerEntity entity, ClientResourceManager resources, EntityDrawable3DStaticResources staticResources)
            : base(entity)
        {
            var buttonParams = new MaterialParams();
            buttonParams["Color"] = ((int)entity.Lane % 2 == 0) ? Blue : Yellow;

            m_size = (entity.Large ? 1.0f : 1.0f) / 4;

            m_drawable = new Drawable3D()
            {
                Texture = resources.GetTexture("textures/game/spinner"),
                Material = resources.GetMaterial("materials/basic"),
                Mesh = entity.Large ? staticResources.LargeSpinnerMesh : staticResources.SmallSpinnerMesh,
                Params = buttonParams,
            };

            if (entity.Large)
            {
                var durationParams = new MaterialParams();
                durationParams["Color"] = new Vector4(1, 1, 1, 0.5f);
                m_duration = new Drawable3D()
                {
                    Texture = resources.GetTexture("textures/game/spinner"),
                    Material = resources.GetMaterial("materials/hold"),
                    Mesh = staticResources.LargeSpinnerDurationMesh,
                    Params = durationParams,
                };
            }
        }

        public override void Render(RenderQueue rq, Transform world, float len)
        {
            if (m_duration != null)
            {
                m_duration.Params["Scale"] = len;
                m_duration.Params["HeadPosition"] = HeadPosition;
                m_duration.Params["Completion"] = Completion;
                m_duration.DrawToQueue(rq, Transform.Scale(1.0f / 6, 1, 1) * world);
            }

            //m_drawable.DrawToQueue(rq, Transform.Scale(m_size, m_size, 1) * (billboard ?? world));

            rq.DepthFunction = theori.Graphics.OpenGL.DepthFunction.LessThanOrEqual;
            m_drawable.DrawToQueue(rq, Transform.Scale(m_size, m_size, m_size) * Transform.RotationY(Spin * 90) * Transform.Translation(0, 0, HeadPosition) * world);
            rq.DepthFunction = null;

        }
    }
}
