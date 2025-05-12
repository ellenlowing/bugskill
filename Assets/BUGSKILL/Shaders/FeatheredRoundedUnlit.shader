Shader "Custom/FeatheredRoundedUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Corner Radius", Range(0,0.5)) = 0.2
        _Feather ("Feather Amount", Range(0.001,0.2)) = 0.05
        _RectSize ("Rect Size", Vector) = (0.8, 0.8, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" }

        // 0. It's important to have One OneMinusSrcAlpha so it blends properly against transparent background (passthrough)
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            // 1. Keywords are used to enable different occlusions
            #pragma multi_compile _ HARD_OCCLUSION SOFT_OCCLUSION

            // if your shaders are in a BiRP project, you would add this include instead:
            #include "Packages/com.meta.xr.sdk.core/Shaders/EnvironmentDepth/BiRP/EnvironmentOcclusionBiRP.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 positionOS : TEXCOORD1;

                // 3. This macro adds 'posWorld' to the varyings struct.
                //    The subsequent macros require this field to be named as such.
                //    The number has to be filled with the recent TEXCOORD number + 1
                //    Or 0 as in this case, if there are no other TEXCOORD fields
                META_DEPTH_VERTEX_OUTPUT(0)

                UNITY_VERTEX_INPUT_INSTANCE_ID
                // 4. The fragment shader needs to understand to which eye it's currently
                //    rendering, in order to get depth from the correct texture.
                UNITY_VERTEX_OUTPUT_STEREO
            };

            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Radius;
            float _Feather;
            float2 _RectSize;

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = UnityObjectToClipPos(input.vertex.xyz); // Note: input.vertex.xyz is object space position
                output.positionOS = input.vertex;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                // 5. World position is required to calculate the occlusions.
                //    This macro will calculate and set world position value in the output Varyings structure.
                META_DEPTH_INITIALIZE_VERTEX_OUTPUT(output, input.vertex);

                // 6. Passes stereo information to frag shader
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                return output;
            }

            float sdRoundedRect(float2 uv, float2 size, float radius)
            {
                // Transform UV to [-1, 1] centered space
                uv = uv * 2.0 - 1.0;
                float2 q = abs(uv) - size + radius;
                float outside = length(max(q, 0.0));
                float inside = min(max(q.x, q.y), 0.0);
                return outside + inside - radius;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                // 7. Initializes global stereo constant for the frag shader
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv;
                float distance = sdRoundedRect(uv, _RectSize.xy, _Radius);
                float alpha = smoothstep(0.0, _Feather, -distance);
                float4 col = tex2D(_MainTex, uv);
                col.a *= alpha;
                // 8. A third macro required to enable occlusions.
                //    It requires previous macros to be there as well as the naming behind the macro is strict.
                //    It will enable soft or hard occlusions depending on the current keyword set.
                //    finalColor value will be multiplied by the occlusion visibility value.
                //    Occlusion visibility value is 0 if virtual object is completely covered by environment and vice versa.
                //    Fully occluded pixels will be discarded

                // META_DEPTH_OCCLUDE_OUTPUT_PREMULTIPLY(input, finalColor, 0);

                return col;
            }
            ENDHLSL
        }
    }
}
