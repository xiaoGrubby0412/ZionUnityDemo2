// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Unlit shader. Simplest possible colored shader.
// - no lighting
// - no lightmap support
// - no texture

Shader "Role/Cartoon/Skin" {

	Properties{
		[MainColor]
		_MainColor("主颜色", Color) = (1,1,1,1)
		albedo("主贴图",2D) = "white"{}
		[Normal]
		bumptex ("法线贴图", 2D) = "bump"{}
		bumpscale("法线强度", Range(-2, 2)) = 1
		lookuptex("lookuptex",2D) = "black"{}
		paramtex("高光&光泽&透射&AO",2D)="white"{}
		roughness("粗糙度", Range(0, 1)) = 1
		glossness("光泽度", Range(0, 10)) = 1
		scatter("透射率", Range(0, 1)) = 1
	}
 
	SubShader
	{
		Pass
		{

			Tags{ "RenderType" = "Opaque" "LightMode"="ForwardBase"}
			CGPROGRAM

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma multi_compile_fwdbase
			#pragma vertex vert
			#pragma fragment frag	



			struct a2v
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float4 tangent: TANGENT;
				float2 uv:TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed2 cap:COLOR2;
				float4 TtoW0 : TEXCOORD1;  
				float4 TtoW1 : TEXCOORD2;  
				float4 TtoW2 : TEXCOORD3;
				float2 uv: TEXCOORD4;
				float3 wView:TEXCOORD5;
				LIGHTING_COORDS(6, 7)
			};

			float4 _MainColor;
 			sampler2D lookuptex;
 			sampler2D albedo;
 			sampler2D paramtex;
 			sampler2D matcap;
 			sampler2D bumptex;
			float roughness;
			float scatter;
			float glossness;
			float bumpscale;
			float4 albedo_ST;
			float4 _LightColor0;
			v2f vert(a2v v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				o.pos = UnityObjectToClipPos(v.vertex);
				
				//--------------------------
				float3 wPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz,1.0)).xyz;  
				fixed3 wNormal = UnityObjectToWorldNormal(v.normal);  
				fixed3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);  
				fixed3 wBinormal = cross(wNormal, wTangent) * v.tangent.w;

				o.TtoW0 = float4(wTangent.x, wBinormal.x, wNormal.x, wPos.x);
				o.TtoW1 = float4(wTangent.y, wBinormal.y, wNormal.y, wPos.y);
				o.TtoW2 = float4(wTangent.z, wBinormal.z, wNormal.z, wPos.z);  
				//-------------------------
				TRANSFER_VERTEX_TO_FRAGMENT(o);

				float3 N = normalize(UnityObjectToWorldNormal(v.normal));
				o.wView =  normalize(_WorldSpaceCameraPos.xyz-wPos);
				o.uv = v.uv;
				return o;
			}
 

			float calSpecular(float3 light,float3 wView,float3 wNormal,float r,float g,float2 uv ){
				float3 halfVec = normalize(wView/3+light);
				float nDotH = dot(halfVec,wNormal);
				nDotH = clamp(nDotH,0.0,1.0)*0.99+0.005;
				r = r*0.99+0.005;
				float PH = pow( 2.0*tex2D(lookuptex,float2(nDotH,r)).a, 10.0); 
		        // PH =  tex2D(lookuptex,float2(nDotH,1.0-0.01)).a; 
				float EdotH = dot(wView,halfVec);
				float exponential = pow(1.0 - EdotH, 5.0);
				float fresnelReflectance = exponential + 0.028 * (1.0 - exponential);  
				float frSpec = max( PH * fresnelReflectance / dot( halfVec, halfVec ), 0 );  
				float _spec = saturate(dot(light,wNormal) * g * frSpec); // BRDF * dot(N,L) * rho_s
				return _spec;
			}
			//定义片元shader
			fixed4 frag(v2f i) : SV_Target
			{
				float3 l1 = normalize(_WorldSpaceLightPos0.xyz);
	
				float4 ambientColor = float4(0.45,0.455,0.455,1.0);

				fixed4 albedocol = _MainColor * tex2D(albedo, i.uv);
			    float3 wNormal = normalize(float3(i.TtoW0.z,i.TtoW1.z,i.TtoW2.z));
                fixed3 bump = UnpackNormal(tex2D(bumptex, i.uv));  
                bump.xy *= bumpscale;
			    wNormal = normalize(half3(dot(i.TtoW0.xyz, bump), dot(i.TtoW1.xyz, bump), dot(i.TtoW2.xyz, bump)));
				float attenuation = LIGHT_ATTENUATION(i);
				float nDotL = dot(wNormal, l1)*0.5+0.5;
				nDotL = lerp(nDotL/4, nDotL,attenuation);
				//float nDotL2 = dot(wNormal, l2)*0.5+0.5;
				//float nDotL3 = dot(wNormal, l3)*0.5+0.5;
				float4 param = tex2D(paramtex,i.uv);
				float r = roughness*param.g;
				float g = glossness*param.r;
				scatter = scatter*param.b;
				float ao = param.a;
				
				float4 diff = tex2D(lookuptex, half2(nDotL, scatter));

				float4 _spec1 = calSpecular(l1,i.wView,wNormal,r,g,i.uv);
				float4 lighting = max(diff* _LightColor0,0.0) + ambientColor;
				float4 shadowCol = lerp(float4(0.9,0.85,0.8,1.0), 1.0,attenuation);
				float4 spec =  _spec1 ;
				float4 aoCol = lerp(albedocol, 1.0, ao);
				float4 col = (albedocol* aoCol *lighting + spec);
				//col = pow(col-0.1,0.6)- float4(0.13,0.12,0.11,0.0);
				//col = col*1.25;
				//col = saturate(col);
	/*			col = aoCol;*/
				col.a = albedocol.a;
				return col ;

			}
			ENDCG
		}
		Pass
		{

			Tags{ "RenderType" = "Opaque" "lightMode" = "ForwardAdd"}
			Blend One One
			CGPROGRAM

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma multi_compile_fwdbase
			#pragma vertex vert
			#pragma fragment frag	



			struct a2v
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float4 tangent: TANGENT;
				float2 uv:TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed2 cap : COLOR2;
				float4 TtoW0 : TEXCOORD1;
				float4 TtoW1 : TEXCOORD2;
				float4 TtoW2 : TEXCOORD3;
				float2 uv: TEXCOORD4;
				float3 wView:TEXCOORD5;
				LIGHTING_COORDS(6, 7)
			};

			float4 _MainColor;
			sampler2D lookuptex;
			sampler2D albedo;
			sampler2D paramtex;
			sampler2D matcap;
			sampler2D bumptex;
			float roughness;
			float scatter;
			float glossness;
			float bumpscale;
			float4 albedo_ST;
			v2f vert(a2v v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				o.pos = UnityObjectToClipPos(v.vertex);

				//--------------------------
				float3 wPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz,1.0)).xyz;
				fixed3 wNormal = UnityObjectToWorldNormal(v.normal);
				fixed3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
				fixed3 wBinormal = cross(wNormal, wTangent) * v.tangent.w;

				o.TtoW0 = float4(wTangent.x, wBinormal.x, wNormal.x, wPos.x);
				o.TtoW1 = float4(wTangent.y, wBinormal.y, wNormal.y, wPos.y);
				o.TtoW2 = float4(wTangent.z, wBinormal.z, wNormal.z, wPos.z);
				//-------------------------
				TRANSFER_VERTEX_TO_FRAGMENT(o);

				float3 N = normalize(UnityObjectToWorldNormal(v.normal));
				o.wView = normalize(_WorldSpaceCameraPos.xyz - wPos);
				o.uv = v.uv;
				return o;
			}


			float calSpecular(float3 light,float3 wView,float3 wNormal,float r,float g,float2 uv) {
				float3 halfVec = normalize(wView / 3 + light);
				float nDotH = dot(halfVec,wNormal);
				nDotH = clamp(nDotH,0.0,1.0) * 0.99 + 0.005;
				r = r * 0.99 + 0.005;
				float PH = pow(2.0 * tex2D(lookuptex,float2(nDotH,r)).a, 10.0);
				// PH =  tex2D(lookuptex,float2(nDotH,1.0-0.01)).a; 
				float EdotH = dot(wView,halfVec);
				float exponential = pow(1.0 - EdotH, 5.0);
				float fresnelReflectance = exponential + 0.028 * (1.0 - exponential);
				float frSpec = max(PH * fresnelReflectance / dot(halfVec, halfVec), 0);
				float _spec = saturate(dot(light,wNormal) * g * frSpec); // BRDF * dot(N,L) * rho_s
				return _spec;
			}
			//定义片元shader
			fixed4 frag(v2f i) : SV_Target
			{
				float3 l1 = normalize(_WorldSpaceLightPos0.xyz);

				fixed4 albedocol = _MainColor * tex2D(albedo, i.uv);
				float3 wNormal = normalize(float3(i.TtoW0.z,i.TtoW1.z,i.TtoW2.z));
				fixed3 bump = UnpackNormal(tex2D(bumptex, i.uv));
				bump.xy *= bumpscale;
				wNormal = normalize(half3(dot(i.TtoW0.xyz, bump), dot(i.TtoW1.xyz, bump), dot(i.TtoW2.xyz, bump)));
				float attenuation = LIGHT_ATTENUATION(i);
				float nDotL = dot(wNormal, l1) * 0.5 + 0.5;
				nDotL = lerp(nDotL / 4, nDotL,attenuation);
				//float nDotL2 = dot(wNormal, l2)*0.5+0.5;
				//float nDotL3 = dot(wNormal, l3)*0.5+0.5;
				float4 param = tex2D(paramtex,i.uv);
				float r = roughness * param.g;
				float g = glossness * param.r;
				scatter = scatter * param.b;
				float ao = param.a;

				float4 diff = tex2D(lookuptex, half2(nDotL, scatter));

				float4 _spec1 = calSpecular(l1,i.wView,wNormal,r,g,i.uv);
				float4 lighting = max(diff,0.0);
				float4 shadowCol = lerp(float4(0.9,0.85,0.8,1.0), 1.0,attenuation);
				float4 spec = _spec1;
				float4 aoCol = lerp(albedocol, 1.0, ao);
				float4 col = (albedocol * aoCol * lighting + spec);
				//col = pow(col-0.1,0.6)- float4(0.13,0.12,0.11,0.0);
				//col = col*1.25;
				//col = saturate(col);
	/*			col = aoCol;*/
				col.a = albedocol.a;
				return col;

			}
			ENDCG
		}
	}
	FallBack "Diffuse"
	CustomEditor "CustomMaterialInspector"
}
