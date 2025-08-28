Shader "Skybox/SkyboxCustom"
{

    Properties
    {
        _Tint ("Tint Color", Color) = (.5, .5, .5, 1)
        _Tint2 ("Tint Color 2", Color) = (.5, .5, .5, 1)

        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0

        _Rotation ("Rotation", Range(0, 360)) = 0
        _BlendCubemaps ("Blend Cubemaps", Range(0, 1)) = 0

        [NoScaleOffset] _Tex ("Cubemap (HDR)", Cube) = "grey" {}
        [NoScaleOffset] _Tex2 ("Cubemap (HDR) 2", Cube) = "grey" {}
        
        _SkyScale ("Sky Angular Scale", Range(0.25, 4)) = 1
        _SkyOffset("Sky Angular Offset (deg XY)", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"
        }
        Cull Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            samplerCUBE _Tex;
            samplerCUBE _Tex2;
            
            half4 _Tex_HDR;
            half4 _Tint;
            half4 _Tint2;

            half _Exposure;
            
            float _Rotation;
            float _BlendCubemaps;
            float _SkyScale;
            float4 _SkyOffset;

            float4 RotateAroundYInDegrees(float4 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;

                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);

                return float4(mul(m, vertex.xz), vertex.yw).xzyw;
            }

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float4 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
                o.vertex = UnityObjectToClipPos(rotated);
                o.texcoord = rotated.xyz;

                return o;
            }

            void DirToLonLat(float3 dir, out float lon, out float lat)
            {
                dir = normalize(dir);
                lon = atan2(dir.x, dir.z);
                lat = asin(saturate(dir.y) * 2.0 - saturate(dir.y));
                lat = asin(clamp(dir.y, -0.999999, 0.999999));
            }

            float3 LonLatToDir(float lon, float lat)
            {
                float cl = cos(lat);
                float y = sin(lat);
                float x = sin(lon) * cl;
                float z = cos(lon) * cl;
                return normalize(float3(x, y, z));
            }

             float WrapPi(float a)
            {
                const float TWO_PI = 6.283185307179586;
                a = fmod(a + UNITY_PI, TWO_PI);
                
                if (a < 0)
                    a += TWO_PI;
                
                return a - UNITY_PI;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.texcoord);
                float lon;
                float lat;
                
                DirToLonLat(dir, lon, lat);

                float lonOff = radians(_SkyOffset.x);
                float latOff = radians(_SkyOffset.y);

                lon = WrapPi(lon * _SkyScale + lonOff);
                lat = clamp(lat * _SkyScale + latOff, -0.4999 * UNITY_PI, 0.4999 * UNITY_PI);

                float3 scaledDir = LonLatToDir(lon, lat);
                
                float4 env1 = texCUBE(_Tex, scaledDir);
                float4 env2 = texCUBE(_Tex2, scaledDir);
                float4 env = lerp(env1, env2, _BlendCubemaps);
                
                half3 c = DecodeHDR(env, _Tex_HDR);

                float4 tint = lerp(_Tint, _Tint2, _BlendCubemaps);
                c *= tint.rgb * unity_ColorSpaceDouble;
                c *= _Exposure;

                return half4(c, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}