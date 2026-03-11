Shader "Mobile/DoubleSided"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass 
        {
            Lighting Off 
            ZWrite Off  
            Cull Off   
            Fog             
            {
                Mode Off
            }

            Blend SrcAlpha OneMinusSrcAlpha 
            SetTexture [_MainTex]
            {
                Combine texture
            }
        }
    }
    FallBack "Diffuse"
}
