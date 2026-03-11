Shader "Minecraft/FlatLitWithPoint"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Brightness ("Brightness", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
        Cull Back
        ZWrite On

        // --------- BASE PASS (Directional + Ambient) ----------
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _MainTex;
            float _Brightness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Directional Light 색만 받음 (방향 기반 N·L 제거)
                float3 lightCol = _LightColor0.rgb;
                float directional = max(lightCol.r, max(lightCol.g, lightCol.b));

                // Ambient + Directional
                float total = _Brightness * (directional + UNITY_LIGHTMODEL_AMBIENT.r);

                col.rgb *= total;
                return col;
            }
            ENDCG
        }

        // --------- ADD PASS (Point Light / Spot Light) ----------
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }
            Blend One One  // Additive
            CGPROGRAM
            #pragma vertex vertAdd
            #pragma fragment fragAdd
            #pragma multi_compile_fwdadd
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _MainTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            v2f vertAdd(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 fragAdd(v2f i) : SV_Target
            {
                // Point Light 위치
                float3 lightDir = _WorldSpaceLightPos0.xyz - i.worldPos;
                float dist = length(lightDir);

                // Range 기반 감쇠
                float attenuation = saturate(1.0 - dist / _LightColor0.a);

                fixed4 col = tex2D(_MainTex, i.uv);

                // 점광원 색 × 감쇠
                col.rgb *= _LightColor0.rgb * attenuation;

                return col;
            }
            ENDCG
        }
    }
}
