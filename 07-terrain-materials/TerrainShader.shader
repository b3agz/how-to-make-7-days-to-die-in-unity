Shader "Custom/Terrain" {

	// These properties can be modified from the material inspector.
	Properties{

		_TexArr("Textures", 2DArray) = "" {}

		_MainTex("Ground Texture", 2D) = "white" {}
		_WallTex("Wall Texture", 2D) = "white" {}
		_TexScale("Texture Scale", Float) = 1

	}

	// You can have multiple subshaders with different levels of complexity. Unity will pick the first one
	// that works on whatever machine is running the game.
	SubShader{

		Tags { "RenderType" = "Opaque" } // None of our terrain is going to be transparent so Opaque it is.
		LOD 200 // We only need diffuse for now so 200 is fine. (higher includes bumped, specular, etc)

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows // Use Unity's standard lighting model
		#pragma target 3.5 // Lower target = fewer features but more compatibility.
		#pragma require 2darray

		// Declare our variables (above properties must be declared here)
		sampler2D _MainTex;
		sampler2D _WallTex;
		UNITY_DECLARE_TEX2DARRAY(_TexArr);
		float _TexScale;

		// Say what information we want from our geometry.
		struct Input {

			float3 worldPos;
			float3 worldNormal;
			float2 uv_TexArr;

		};

		// This function is run for every pixel on screen.
		void surf(Input IN, inout SurfaceOutputStandard o) {

			float3 scaledWorldPos = IN.worldPos / _TexScale; // Get a the world position modified by scale.
			float3 pWeight = abs(IN.worldNormal); // Get the current normal, using abs function to ignore negative numbers.
			pWeight /= pWeight.x + pWeight.y + pWeight.z; // Ensure pWeight isn't greater than 1.

			int texIndex = floor(IN.uv_TexArr.x + 0.1); // Current index of our texture in the array.
			float3 projected; // float3 storing the current 2D UV coords + the index stored in the Z value.

			// Get the texture projection on each axes and "weight" it by multiplying it by the pWeight.
			projected = float3(scaledWorldPos.y, scaledWorldPos.z, texIndex);
			float3 xP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.x;

			projected = float3(scaledWorldPos.x, scaledWorldPos.z, texIndex);
			float3 yP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.y;

			projected = float3(scaledWorldPos.x, scaledWorldPos.y, texIndex);
			float3 zP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.z;

			// Return the sum of all of the projections.
			o.Albedo = xP + yP + zP;

		}
		ENDCG
	}
	FallBack "Diffuse"
}