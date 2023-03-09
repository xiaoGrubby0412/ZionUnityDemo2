// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Role/Cartoon/Hair Transparent" 
{
	Properties 
	{
        _MainColor ("Main Color", Color) = (1,1,1,1)
		albedo ("主贴图 (RGB) 透明 (A)", 2D) = "white" {}
		cut("clip",range(0,1)) = 0.6
		ramptex("光照梯度图", 2D) = "white" {}
		rampintensity("光照强度", Range(0, 2)) = 1
        bumptex ("法线贴图", 2D) = "bump"{}
		bumpscale("法线 强度", Range(-2, 2)) = 1

        specularcolor ("高光 颜色1", Color) = (1,1,1,1)
        specularcolor2 ("高光 颜色2", Color) = (0.5,0.5,0.5,1)
		specularmultiplier ("高光度1", range(1,1000)) = 100.0
		specularmultiplier2 ("高光度2", range(1,1000)) = 100.0
		
		primaryshift ( "高光偏移1", range(-1,1)) = 0.0
		secondaryshift ( "高光偏移2", range(-1,1)) = .7
		anisotex ("高光抖动", 2D) = "white" {}

	}
	
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Pass
		{
			// Blend SrcAlpha OneMinusSrcAlpha
			zwrite on
			Cull off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"	
			#pragma target 3.0

			sampler2D albedo, anisotex,bumptex,ramptex;
			float4 albedo_ST, anisotex_ST,bumptex_ST;
			float rampintensity;
			float cut;
			half specularmultiplier, primaryshift, specularintensity,secondaryshift,specularmultiplier2;
			half4 specularcolor, _MainColor,specularcolor2;

			half bumpscale;
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
		
			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 TtoW0 : TEXCOORD1;  
				float4 TtoW1 : TEXCOORD2;  
				float4 TtoW2 : TEXCOORD3;
				float4 vertex : SV_POSITION;
			};

			//获取头发高光
			fixed StrandSpecular ( fixed3 T, fixed3 V, fixed3 L, fixed exponent)
			{
				fixed3 H = normalize(L + V);
				fixed dotTH = dot(T, H);
				fixed sinTH = sqrt(1 - dotTH * dotTH);
				fixed dirAtten = smoothstep(-1, 0, dotTH);
				return dirAtten * pow(sinTH, exponent);
			}
			
			//沿着法线方向调整Tangent方向
			fixed3 ShiftTangent ( fixed3 T, fixed3 N, fixed shift)
			{
				return normalize(T + shift * N);
			}

			v2f vert (appdata_full v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.texcoord, albedo);
				o.uv.zw = TRANSFORM_TEX(v.texcoord, bumptex);

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;  
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;

				o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);  

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 albedocol = tex2D(albedo, i.uv);
				half4 diffuseColor = albedocol * _MainColor;
				clip(diffuseColor.a-cut);
				//法线相关
				fixed3 bump = UnpackScaleNormal(tex2D(bumptex, i.uv.zw),bumpscale);
				fixed3 worldNormal = normalize(half3(dot(i.TtoW0.xyz, bump), dot(i.TtoW1.xyz, bump), dot(i.TtoW2.xyz, bump)));
				float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
				fixed3 worldTangent = normalize(half3(i.TtoW0.x, i.TtoW1.x, i.TtoW2.x));
				fixed3 worldBinormal = normalize(half3(i.TtoW0.y, i.TtoW1.y, i.TtoW2.y));			

				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				worldLightDir = normalize(float3(-1,1,1));
				fixed shift = tex2D(anisotex, i.uv*anisotex_ST.xy+anisotex_ST.zw).r;
				//计算切线方向的偏移度
				half3 t1 = ShiftTangent(worldBinormal, worldNormal, primaryshift + shift);
				half3 t2 = ShiftTangent(worldBinormal, worldNormal, secondaryshift + shift);
				//
				float val = dot(worldNormal, worldLightDir) / 2 + 0.5;
				float4 diffuse = tex2D(ramptex, fixed2(val, 0.5));
				//计算高光强度
				half3 spec1 = StrandSpecular(t1, worldViewDir, worldLightDir, specularmultiplier)* specularcolor;
				half3 spec2 = StrandSpecular(t2, worldViewDir, worldLightDir, specularmultiplier2)* specularcolor2* specularcolor2;

				fixed4 finalColor = 0;
				finalColor.rgb = diffuseColor.rgb* diffuse* rampintensity + (spec1+ spec2);//第一层高光
				finalColor.a = diffuseColor.a;
				
				return finalColor;
			};
			ENDCG
		}
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull front
			zwrite off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"	
			#pragma target 3.0

			sampler2D albedo, anisotex,bumptex,ramptex;
			float4 albedo_ST, anisotex_ST,bumptex_ST;
			float rampintensity;
			half specularmultiplier, primaryshift, specularintensity,secondaryshift,specularmultiplier2;
			half4 specularcolor, _MainColor,specularcolor2;

			half bumpscale;
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
		
			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 TtoW0 : TEXCOORD1;  
				float4 TtoW1 : TEXCOORD2;  
				float4 TtoW2 : TEXCOORD3;
				float4 vertex : SV_POSITION;
			};

			//获取头发高光
			fixed StrandSpecular ( fixed3 T, fixed3 V, fixed3 L, fixed exponent)
			{
				fixed3 H = normalize(L + V);
				fixed dotTH = dot(T, H);
				fixed sinTH = sqrt(1 - dotTH * dotTH);
				fixed dirAtten = smoothstep(-1, 0, dotTH);
				return dirAtten * pow(sinTH, exponent);
			}
			
			//沿着法线方向调整Tangent方向
			fixed3 ShiftTangent ( fixed3 T, fixed3 N, fixed shift)
			{
				return normalize(T + shift * N);
			}

			v2f vert (appdata_full v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.texcoord, albedo);
				o.uv.zw = TRANSFORM_TEX(v.texcoord, bumptex);

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;  
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;

				o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);  

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 albedocol = tex2D(albedo, i.uv);
				half4 diffuseColor = albedocol * _MainColor;

				//法线相关
				fixed3 bump = UnpackScaleNormal(tex2D(bumptex, i.uv.zw),bumpscale);
				fixed3 worldNormal = normalize(half3(dot(i.TtoW0.xyz, bump), dot(i.TtoW1.xyz, bump), dot(i.TtoW2.xyz, bump)));
				float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
				fixed3 worldTangent = normalize(half3(i.TtoW0.x, i.TtoW1.x, i.TtoW2.x));
				fixed3 worldBinormal = normalize(half3(i.TtoW0.y, i.TtoW1.y, i.TtoW2.y));			

				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				worldLightDir = normalize(float3(-1,1,1));
				fixed shift = tex2D(anisotex, i.uv*anisotex_ST.xy+anisotex_ST.zw).r;
				//计算切线方向的偏移度
				half3 t1 = ShiftTangent(worldBinormal, worldNormal, primaryshift + shift);
				half3 t2 = ShiftTangent(worldBinormal, worldNormal, secondaryshift + shift);
				//
				float val = dot(worldNormal, worldLightDir) / 2 + 0.5;
				float4 diffuse = tex2D(ramptex, fixed2(val, 0.5));
				//计算高光强度
				half3 spec1 = StrandSpecular(t1, worldViewDir, worldLightDir, specularmultiplier)* specularcolor;
				half3 spec2 = StrandSpecular(t2, worldViewDir, worldLightDir, specularmultiplier2)* specularcolor2* specularcolor2;

				fixed4 finalColor = 0;
				finalColor.rgb = diffuseColor.rgb* diffuse* rampintensity + (spec1+ spec2);//第一层高光
				finalColor.a = diffuseColor.a;
				
				return finalColor;
			};
			ENDCG
		}
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull back
			zwrite off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"	
			#pragma target 3.0

			sampler2D albedo, anisotex,bumptex,ramptex;
			float4 albedo_ST, anisotex_ST,bumptex_ST;
			float rampintensity;
			half specularmultiplier, primaryshift, specularintensity,secondaryshift,specularmultiplier2;
			half4 specularcolor, _MainColor,specularcolor2;

			half bumpscale;
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
		
			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 TtoW0 : TEXCOORD1;  
				float4 TtoW1 : TEXCOORD2;  
				float4 TtoW2 : TEXCOORD3;
				float3 T:TEXCOORD4;
				float3 N:TEXCOORD5;
				float4 vertex : SV_POSITION;
			};

			//获取头发高光
			fixed StrandSpecular ( fixed3 T, fixed3 V, fixed3 L, fixed exponent)
			{
				fixed3 H = normalize(L + V);
				fixed dotTH = dot(T, H);
				fixed sinTH = sqrt(1 - dotTH * dotTH);
				fixed dirAtten = smoothstep(-1, 0, dotTH);
				return dirAtten * pow(sinTH, exponent);
			}
			
			//沿着法线方向调整Tangent方向
			fixed3 ShiftTangent ( fixed3 T, fixed3 N, fixed shift)
			{
				return normalize(T + shift * N);
			}

			v2f vert (appdata_full v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.texcoord, albedo);
				o.uv.zw = TRANSFORM_TEX(v.texcoord, bumptex);

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;  
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;

				o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);  
				o.T = v.tangent.xyz;
				o.N = v.normal.xyz;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 albedocol = tex2D(albedo, i.uv);
				half4 diffuseColor = albedocol * _MainColor;

				//法线相关
				fixed3 bump = UnpackScaleNormal(tex2D(bumptex, i.uv.zw),bumpscale);
				fixed3 worldNormal = normalize(half3(dot(i.TtoW0.xyz, bump), dot(i.TtoW1.xyz, bump), dot(i.TtoW2.xyz, bump)));
				float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
				fixed3 worldTangent = normalize(half3(i.TtoW0.x, i.TtoW1.x, i.TtoW2.x));
				fixed3 worldBinormal = normalize(half3(i.TtoW0.y, i.TtoW1.y, i.TtoW2.y));			

				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				worldLightDir = normalize(float3(-1,1,1));
				fixed shift = tex2D(anisotex, i.uv*anisotex_ST.xy+anisotex_ST.zw).r;
				//计算切线方向的偏移度
				half3 t1 = ShiftTangent(worldBinormal, worldNormal, primaryshift + shift);
				half3 t2 = ShiftTangent(worldBinormal, worldNormal, secondaryshift + shift);
				//
				float val = dot(worldNormal, worldLightDir) / 2 + 0.5;
				float4 diffuse = tex2D(ramptex, fixed2(val, 0.5));
				//计算高光强度
				half3 spec1 = StrandSpecular(t1, worldViewDir, worldLightDir, specularmultiplier)* specularcolor;
				half3 spec2 = StrandSpecular(t2, worldViewDir, worldLightDir, specularmultiplier2)* specularcolor2* specularcolor2;

				fixed4 finalColor = 0;
				finalColor.rgb = diffuseColor.rgb* diffuse* rampintensity + (spec1+ spec2)*val;//第一层高光
				finalColor.a = diffuseColor.a;
				
				return finalColor;
			};
			ENDCG
		}

	}

	FallBack "Diffuse"
	    CustomEditor "CustomMaterialInspector"
}