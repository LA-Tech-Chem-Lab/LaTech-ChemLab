Shader "Custom/BeakerStencil"
{
    SubShader
    {
        Tags { "Queue" = "Geometry" }
        ColorMask 0
        ZWrite On

        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }

        Pass {}
    }
}
