Shader "SmallGenius/DistanceField"
{
    Properties //着色器的输入 
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {
            "RenderType"="Opaque"
            "RenderPipeLine"="UniversalRenderPipeline" //用于指明使用URP来渲染
        }

        HLSLINCLUDE 
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl" 
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

        CBUFFER_START(UnityPerMaterial) //声明变量
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
        CBUFFER_END

        TEXTURE2D(_MainTex); //贴图采样  
        SAMPLER(sampler_MainTex);

        struct a2v //顶点着色器
        {
            float4 positionOS: POSITION;
            float3 normalOS: TANGENT;
            half4 vertexColor: COLOR;
            float2 uv : TEXCOORD0;
        };

        struct v2f //片元着色器
        {
            float4 positionCS: SV_POSITION;
            float2 uv: TEXCOORD0;
            half4 vertexColor: COLOR;
        }; 

        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertexColor = v.vertexColor;
                return o;
            }

            float2 PointSampledUVs(float2 uv,float2 textureSize)
            {
                return (floor(uv * textureSize) + 0.5) / textureSize;
            }

            float4 frag (v2f i) : SV_Target  /* 注意在HLSL中，fixed4类型变成了half4类型*/
            {
                float2 uv = PointSampledUVs(i.uv,_MainTex_TexelSize.zw);
                float4 col = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, uv , 0);
                float distance = length(uv - col.xy);
                return distance;
            }
            ENDHLSL
        }
    }
}