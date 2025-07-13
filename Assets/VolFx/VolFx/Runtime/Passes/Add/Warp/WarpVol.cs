using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Warp")]
    public sealed class WarpVol : VolumeComponent, IPostProcessComponent
    {
		public ClampedFloatParameter _intensity = new ClampedFloatParameter(0f, 0f, 1f);
		public ClampedFloatParameter _depth     = new ClampedFloatParameter(2f, 0f, 2f);
		public GradientParameter	 _color     = new GradientParameter(new GradientValue(new Gradient()), false);
        public ColorParameter        _emission  = new ColorParameter(Color.clear);
		public ClampedFloatParameter _distort   = new ClampedFloatParameter(0f, 0f, 1f);

		public NoInterpClampedFloatParameter _size      = new NoInterpClampedFloatParameter(.7f, 0f, 10f);
		public NoInterpClampedFloatParameter _hardness  = new NoInterpClampedFloatParameter(0.583f, -2f, 1f);
		public NoInterpClampedFloatParameter _count     = new NoInterpClampedFloatParameter(.5f, 0f, 1f);
		public NoInterpClampedFloatParameter _density   = new NoInterpClampedFloatParameter(2.3f, 0f, 17f);
		public NoInterpClampedFloatParameter _speed     = new NoInterpClampedFloatParameter(3f, -7f, 7f);
		

        // =======================================================================
        public bool IsActive() => active && (_intensity.value > 0f || _distort.value > 0f);

        public bool IsTileCompatible() => true;
    }
}