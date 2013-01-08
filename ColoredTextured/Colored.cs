using System;
using System.Collections.Generic;
using System.Text;
using Gas.Graphics;

namespace Colored
{
    [MainVisualEffectClass]
    public class Colored : VisualEffect
    {
        #region Variables
        private Effect effect = null;
        #endregion

        #region Constructor
        public Colored(Renderer renderer)
            : base(renderer)
        {
            effect = new Effect(renderer, @"E:\Code\Projects\Gas\GasDemo\bin\Release\FXFiles\Lighting.fx");
        }
        #endregion

        #region VisualEffect interface
        public override void BeginRenderScene()
        {
            renderer.Begin(effect);
            renderer.SetPass(2);
        }

        public override void EndRenderScene()
        {
            renderer.End();
        }

        public override void BeginRenderObject(Material material)
        {
            effect.SetValue("world", renderer.WorldMatrix);
            effect.SetValue("worldViewProj", renderer.WorldViewProjectionMatrix);

            effect.CommitChanges();
        }

        public override void EndRenderObject()
        {
        }
        #endregion
    }
}
