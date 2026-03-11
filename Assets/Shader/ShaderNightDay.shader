Shader "SkyBoxJJY/ShaderNightDay"
{
    Properties
    {
        _Texture1 ("Day Sky", 2D) = "white" {}
        _Texture2 ("Night Sky", 2D) = "white" {}
        _Blend ("Blend", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float3 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _Texture1;
            sampler2D _Texture2;
            float _Blend;

            v2f vert(appdata v)
            {
                v2f o;
                o.texcoord = v.vertex.xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float2 ToRadialcoords(float3 coords)
            {
                float3 n = normalize(coords);
                float latitude = acos(n.y);
                float longitude = atan2(n.z, n.x);
                float2 sphere = float2(longitude, latitude) * float2(0.5/UNITY_PI, 1.0/UNITY_PI);
                return float2(0.5, 1.0) - sphere;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = ToRadialcoords(i.texcoord);

                fixed4 day = tex2D(_Texture1, uv);
                fixed4 night = tex2D(_Texture2, uv);

                return lerp(day, night, _Blend);
            }
            ENDCG
        }
    }
}
