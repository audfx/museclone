using System;
using System.Diagnostics;
using System.Numerics;

using theori;
using theori.Charting;
using theori.Graphics;
using theori.Resources;

using Museclone.Charting;

namespace Museclone.Graphics
{
    internal sealed class EntityDrawable3DStaticResources : Disposable
    {
        public readonly Mesh ButtonChipMesh;
        public readonly Mesh ButtonHoldMesh;

        public readonly Mesh PedalHoldMesh;

        public EntityDrawable3DStaticResources()
        {
            ButtonChipMesh = new Mesh();
            ButtonChipMesh.SetIndices(0, 1, 2, 0, 2, 3);
            ButtonChipMesh.SetVertices(new[]
            {
                new VertexP3T2(new Vector3(-1,  0, 0), new Vector2(0, 0.5f)),
                new VertexP3T2(new Vector3( 0,  1, 0), new Vector2(0.5f, 0)),
                new VertexP3T2(new Vector3( 1,  0, 0), new Vector2(1, 0.5f)),
                new VertexP3T2(new Vector3( 0, -1, 0), new Vector2(0.5f, 1)),
            });

            ButtonHoldMesh = Mesh.CreatePlane(Vector3.UnitX, Vector3.UnitZ, 1.0f / 6, 1.0f, Anchor.BottomCenter);
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

            PedalHoldMesh = Mesh.CreatePlane(Vector3.UnitX, Vector3.UnitZ, 1, 1, Anchor.BottomCenter);
        }

        protected override void DisposeManaged()
        {
            ButtonChipMesh.Dispose();
            ButtonHoldMesh.Dispose();

            PedalHoldMesh.Dispose();
        }
    }

    internal abstract class EntityDrawable3D : Disposable
    {
        protected static Vector4 Blue = new Vector4(0.55f, 0.96f, 1.0f, 1);
        protected static Vector4 Yellow = new Vector4(1.0f, 0.975f, 0.575f, 1);

        public readonly Entity Entity;

        protected EntityDrawable3D(Entity entity)
        {
            Entity = entity;
        }

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
                Mesh = staticResources.ButtonChipMesh,
                Params = buttonParams,
            };
        }

        public override void Render(RenderQueue rq, Transform world, float len)
        {
            m_drawable.DrawToQueue(rq, Transform.Scale(m_size, m_size, 1) * world);
        }
    }

    internal class ButtonHoldRenderState3D : EntityDrawable3D
    {
        public new ButtonEntity Entity => (ButtonEntity)base.Entity;

        private readonly float m_size;
        private readonly Drawable3D m_drawableHold, m_drawableChip;

        public ButtonHoldRenderState3D(ButtonEntity entity, ClientResourceManager resources, EntityDrawable3DStaticResources staticResources)
            : base(entity)
        {
            Debug.Assert(!entity.IsInstant, "Instant object passed to render state which expects a hold");

            var buttonParams = new MaterialParams();
            buttonParams["Color"] = ((int)entity.Lane % 2 == 0) ? Blue : Yellow;
            var chipParams = new MaterialParams();
            chipParams["Color"] = ((int)entity.Lane % 2 == 0) ? Blue : Yellow;

            m_size = 1.0f / 12;

            m_drawableHold = new Drawable3D()
            {
                Texture = resources.GetTexture("textures/game/buttonHold"),
                Material = resources.GetMaterial("materials/basic"),
                Mesh = staticResources.ButtonHoldMesh,
                Params = buttonParams,
            };

            m_drawableChip = new Drawable3D()
            {
                Texture = resources.GetTexture("textures/game/buttonHead"),
                Material = resources.GetMaterial("materials/basic"),
                Mesh = staticResources.ButtonChipMesh,
                Params = chipParams,
            };
        }

        public override void Render(RenderQueue rq, Transform world, float len)
        {
            m_drawableHold.DrawToQueue(rq, Transform.Scale(m_size, m_size, len) * world);
            m_drawableChip.DrawToQueue(rq, Transform.Scale(m_size, m_size, 1) * world);
        }
    }
}
