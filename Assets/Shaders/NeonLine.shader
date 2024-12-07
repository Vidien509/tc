Shader "Custom/NeonLine"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Float) = 1.0
        _GlowRange ("Glow Range", Float) = 0.1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade
        #pragma target 3.0

        struct Input
        {
            float4 color : COLOR;
        };

        fixed4 _Color;
        fixed4 _GlowColor;
        float _GlowIntensity;
        float _GlowRange;

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;
            
            // Calculate glow
            float glow = smoothstep(1.0 - _GlowRange, 1.0, IN.color.a) * _GlowIntensity;
            o.Emission = _GlowColor.rgb * glow;
            
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}