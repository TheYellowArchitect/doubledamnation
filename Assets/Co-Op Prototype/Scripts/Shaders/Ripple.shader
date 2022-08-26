//Shader by: http://blog.onebyonedesign.com/unity/unity-ripple-or-shock-wave-effect/ found via https://www.youtube.com/watch?v=UsGuN69g2NI

//Mandatory legal stuff for no worries to anyone:

/**
*    Copyright (c) 2017 Devon O. Wolfgang
*
*    Permission is hereby granted, free of charge, to any person obtaining a copy
*    of this software and associated documentation files (the "Software"), to deal
*    in the Software without restriction, including without limitation the rights
*    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
*    copies of the Software, and to permit persons to whom the Software is
*    furnished to do so, subject to the following conditions:
*
*    The above copyright notice and this permission notice shall be included in
*    all copies or substantial portions of the Software.
*
*    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
*    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
*    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
*    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
*    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
*    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
*    THE SOFTWARE.
*/

Shader "Hidden/Ripple"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	_CenterX("Center X", float) = 300
		_CenterY("Center Y", float) = 250
		_Amount("Amount", float) = 25
		_WaveSpeed("Wave Speed", range(.50, 50)) = 20
		_WaveAmount("Wave Amount", range(0, 20)) = 10
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}

	sampler2D _MainTex;
	float _CenterX;
	float _CenterY;
	float _Amount;
	float _WaveSpeed;
	float _WaveAmount;

	fixed4 frag(v2f i) : SV_Target
	{
		fixed2 center = fixed2(_CenterX / _ScreenParams.x, _CenterY / _ScreenParams.y);
	fixed time = _Time.y *  _WaveSpeed;
	fixed amt = _Amount / 1000;

	fixed2 uv = center.xy - i.uv;
	uv.x *= _ScreenParams.x / _ScreenParams.y;

	fixed dist = sqrt(dot(uv,uv));
	fixed ang = dist * _WaveAmount - time;
	uv = i.uv + normalize(uv) * sin(ang) * amt;

	return tex2D(_MainTex, uv);
	}
		ENDCG
	}
	}
}
