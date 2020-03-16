Shader "Custom/Terrain" {

	// These properties can be modified from the material inspector.
	Properties{

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
		#pragma target 3.0 // Lower target = fewer features but more compatibility.

		// Declare our variables (above properties must be declared here)
		sampler2D _MainTex;
		sampler2D _WallTex;
		float _TexScale;

		// Say what information we want from our geometry.
		struct Input {

			float3 worldPos;
			float3 worldNormal;

		};

		// This function is run for every pixel on screen.
		void surf(Input IN, inout SurfaceOutputStandard o) {

			float3 scaledWorldPos = IN.worldPos / _TexScale; // Get a the world position modified by scale.
			float3 pWeight = abs(IN.worldNormal); // Get the current normal, using abs function to ignore negative numbers.
			pWeight /= pWeight.x + pWeight.y + pWeight.z; // Ensure pWeight isn't greater than 1.

			// Get the texture projection on each axes and "weight" it by multiplying it by the pWeight.
			float3 xP = tex2D(_WallTex, scaledWorldPos.yz) * pWeight.x;
			float3 yP = tex2D(_MainTex, scaledWorldPos.xz) * pWeight.y;
			float3 zP = tex2D(_WallTex, scaledWorldPos.xy) * pWeight.z;

			// Return the sum of all of the projections.
			o.Albedo = xP + yP + zP;

		}
		ENDCG
	}
	FallBack "Diffuse"
}