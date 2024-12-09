Shader "Custom/ShieldEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
        _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
        _IntersectionThreshold ("Intersection Threshold", Range(0,1)) = 0.1
        _GridTexture ("Grid Texture", 2D) = "white" {}
        _GridScale ("Grid Scale", Float) = 1.0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _GridTexture;
        fixed4 _Color;
        float4 _RimColor;
        float _RimPower;
        float _IntersectionThreshold;
        float _GridScale;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            
            // Grid effect
            float2 gridUV = IN.worldPos.xy * _GridScale;
            fixed4 gridColor = tex2D(_GridTexture, gridUV);
            
            // Rim effect
            half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
            o.Emission = _RimColor.rgb * pow (rim, _RimPower);
            
            // Combine effects
            float gridFactor = gridColor.r * 0.5 + 0.5;
            o.Alpha = c.a * rim * gridFactor;
            
            // Add intersection effect
            o.Alpha = lerp(o.Alpha, 1, step(rim, _IntersectionThreshold));
        }
        ENDCG
    }
    FallBack "Diffuse"
}

