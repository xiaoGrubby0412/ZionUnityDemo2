// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Role/eyeGlass"
{
    Properties
    {
//    	[Toggle]
//        _EditorType("高级模式",int) = 0
        _MainColor("主颜色", Color) = (1.0,1.0,1.0,1.0)
        _LightColor("灯光颜色", Color) = (1.0,1.0,1.0,1.0)
        _AmbientColor("全局光颜色", Color) = (0.351,0.33,0.3,1.0)
        _Albedo("主贴图", 2D) = "white" {}
        _MetallicTex ("细节贴图(R:金属度,G:粗糙度)", 2D) = "white" {}
        _Metallic("金属度",Range(0,1)) = 0.5
        _Roughness("粗糙度",Range(0,1)) = 0.5
        _BumpMap ("法线贴图", 2D) = "bump"{}
        _BumpScale("法线强度",Range(-10,10)) = 1
        _AOTex ("AO贴图", 2D) = "white" {}
        _AOIntensity("AO强度", range(0,2)) = 1
        _EnvironmentCube ("环境光贴图", Cube) = "" {}
        _EnvironmentRatio ("曝光度(HDR)", Range(0,2)) = 1.0
        [Toggle] _UseARkitEnv("使用AR环境纹理",float) = 1
    }

    SubShader
    {
       Tags { "LightMode" = "ForwardBase" "RenderType"="Transparent"  "Queue" = "Transparent"}
        LOD 100
        Pass
        {        
       	    Tags { "LightMode" = "ForwardBase"  "RenderType"="Transparent" "Queue" = "Transparent" }
       	    Blend SrcAlpha OneMinusSrcAlpha 
       	    zwrite false 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _INVERT_ON

            #include "UnityCG.cginc"  
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float3 normal:NORMAL;
                float4 tangent:TANGENT;
                float4 vertColor:COLOR;
            };

            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float3 wView : TEXCOORD2;
                fixed4 T1:COLOR_1;
                fixed4 T2:COLOR_2;
                fixed4 T3:COLOR_3;
                float4 vertColor:COLOR_4;
            };

            float4 _MainColor;
            float4 _LightColor;
            sampler2D _Albedo;
            float4 _Albedo_ST;
            float4 _AmbientColor;
            float _Metallic;
            sampler2D _MetallicTex ;           
            float4 _MetallicTex_ST;
            sampler2D _AOTex; 
            float4 _AOTex_ST;
            sampler2D _BumpMap; 
            float4 _BumpMap_ST;
            float _BumpScale;
            float _Roughness;
            samplerCUBE _EnvironmentCube ;
            float4 _EnvironmentCube_TexelSize;
            float _EnvironmentRatio;
            float _AOIntensity;

            VertexOutput vert (appdata v)
            {
                VertexOutput o;
                o.pos = UnityObjectToClipPos(v.vertex );
                o.uv = v.uv;
                o.uv1 = v.uv1;
                float4 wPos = mul(unity_ObjectToWorld, v.vertex);
                float3 wNormal = UnityObjectToWorldNormal(v.normal.xyz);
				o.wView = normalize(_WorldSpaceCameraPos.xyz - wPos.xyz);

				fixed3 _wTangent = UnityObjectToWorldDir(v.tangent).xyz;
                fixed3 _wBinormal = cross(wNormal,_wTangent)*v.tangent.w;   
				o.T1 = float4(_wTangent.x,_wBinormal.x,wNormal.x,wPos.x);
                o.T2 = float4(_wTangent.y,_wBinormal.y,wNormal.y,wPos.y);
                o.T3 = float4(_wTangent.z,_wBinormal.z,wNormal.z,wPos.z);
                o.vertColor = v.vertColor;
                return o;
            }

            fixed4 frag (VertexOutput i) : SV_Target
            {
         	//variables
              //variables
                float3 v = normalize(i.wView);
                float3 l =  normalize(fixed3(0.1,0.2,1.5));
                float3 n = normalize(float3(i.T1.z,i.T2.z,i.T3.z));
                fixed3 _bump = UnpackNormal(tex2D(_BumpMap, i.uv*_BumpMap_ST.xy + _BumpMap_ST.zw));  
                _bump.xy *= _BumpScale;
                n = normalize(half3(dot(i.T1.xyz,_bump),dot(i.T2.xyz,_bump),dot(i.T3.xyz,_bump)));
                float4 _detailTex = tex2D(_MetallicTex, i.uv*_MetallicTex_ST.xy+_MetallicTex_ST.zw);
                float metalnessFactor = _Metallic*_detailTex.r;
				float roughnessFactor = _Roughness*_detailTex.g;

                //some consts 
                float3 h =normalize(l+v);
                float _a = roughnessFactor*roughnessFactor;
                float a2 = _a*_a;
                a2 = _a*_a;
                float nl = saturate(dot(n,l));
                float nl2 = dot(n,l)*0.5+0.5;
                float nv = saturate(dot(n,v));
                float nh = saturate(dot(n,h));                        
                float lh = saturate(dot(l,h));

                //albedo
                fixed4 albedoColor = tex2D(_Albedo, i.uv*_Albedo_ST.xy + _Albedo_ST.zw)*_MainColor;
                //AOmap  
               
                //-------
			    float4 _ColorSpaceDielectricSpec = float4(0.16, 0.16, 0.16, 0.779);
			    float   OneMinusReflectivity = _ColorSpaceDielectricSpec.a - metalnessFactor * _ColorSpaceDielectricSpec.a;
			    float3  diffColor = albedoColor.xyz *OneMinusReflectivity;

				float3 diff = lerp(_AmbientColor.rgb,_LightColor.xyz*1.2,nl2)*diffColor;
                //-------

                //specular
                float3 specColor = lerp( 0.16, albedoColor.xyz, metalnessFactor);

                float t = nh*nh*(a2-1)+1;           
                //法线分布项,决定了specular高光的大小、亮度和形状.
                float D = UNITY_INV_PI*a2/(t*t+1e-6);  
                //几何遮挡项.
                float gv = nl * ( _a + ( 1.0 - _a ) * nv);
                float gl = nv * ( _a + ( 1.0 - _a ) * nl );
                float G = 0.5/( gv + gl+ 1e-5 );
                //反射项.决定表面反射的比率.
                float f = exp2( ( -5.55473*lh - 6.98316 )*lh );
                float3 F = specColor +(1-specColor)*f;
                fixed3 facet = D*F*G;
                facet = sqrt(facet);
                facet = min(facet,1);
                float3 spec = facet*nl*_LightColor.rgb;
                //reflect
                float maxMIPLevelScalar = log2(_EnvironmentCube_TexelSize.z);
                float blinnShininess = 2.0/_a-2.0;
      		 	float desiredMIPLevel = maxMIPLevelScalar + 0.79248 - 0.5 * log2( blinnShininess*blinnShininess + 1.0 );
                float _mipLevel = clamp( desiredMIPLevel, 0.0, maxMIPLevelScalar );

			    float grazing = saturate((1.0 - roughnessFactor) + 1.0 - OneMinusReflectivity);
				float3 specular_Environment = lerp(specColor,grazing,pow(1.0-nv,3)*nl2);
				                                      
                float3 nrv = normalize(reflect(-v, n));

              	half3 env =  texCUBElod(_EnvironmentCube, float4(nrv,_mipLevel)).rgb*_EnvironmentRatio*specular_Environment;

              	float3 amb = _AmbientColor.xyz*diffColor*0.3183;
              	//AO
              	fixed4 AO = lerp(1.0,tex2D(_AOTex, i.uv1*_AOTex_ST.xy + _AOTex_ST.zw)*i.vertColor,_AOIntensity);
               	AO = saturate(pow( nv + AO, exp2( - 16.0 * roughnessFactor - 1.0 ) ) - 1.0 + AO );
            	amb *= AO;
            	diff *=AO;
        		env *= AO;

                //ambient
                fixed4 c = (fixed4)1.0;
                c.rgb = (diff + spec + env + amb); 
              	float env_sat = dot(env,float3(0.2125, 0.7154,0.0721));
              	float spec_sat = dot(spec,float3(0.2125, 0.7154,0.0721)); 
                c.a = albedoColor.a + spec_sat + env_sat;
                return c;
            }
            ENDCG
        }
    }
    CustomEditor "CustomMaterialInspector"
}