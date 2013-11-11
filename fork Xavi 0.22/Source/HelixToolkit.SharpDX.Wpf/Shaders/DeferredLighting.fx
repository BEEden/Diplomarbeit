//--------------------------------------------------------------------------------------
// File: Deferred Lighting for HelixToolkitDX
// Author: Przemyslaw Musialski
// Date: 03/03/13
// References & Sources: 
// code based on the shader from: http://hieroglyph3.codeplex.com/
//--------------------------------------------------------------------------------------

#include "./Shaders/Common.fx"
//#define EXERCISE 1

//-------------------------------------------------------------------------------------------------
// STATES
//-------------------------------------------------------------------------------------------------
RasterizerState RSFront
{
	FillMode					= 3;
	CullMode					= FRONT;
	FrontCounterClockwise		= true;
};

RasterizerState RSBack
{
	FillMode					= 3;
	CullMode					= BACK;
	FrontCounterClockwise		= true;
};

BlendState BSAdditive
{
	BlendEnable[0] = true;
    BlendOp = ADD;
    SrcBlend = ONE;
    DestBlend = ONE;
    BlendOpAlpha = ADD;
    SrcBlendAlpha = ONE;
    DestBlendAlpha = ONE;
};

DepthStencilState DSSNoDepth
{
	DepthEnable					= false;	
};


//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
cbuffer LightParams
{
	// the light direction is here the vector which looks towards the light
	float3 vLightDir;
	float3 vLightPos;
	float3 vLightAtt;		// const, linerar, quadratic, 
	float4 vLightSpot;		//(outer angle , inner angle, falloff, light-range!!!)	
	float4 vLightColor;
	float4 vLightAmbient	= float4(0.2f, 0.2f, 0.2f, 1.0f);
	matrix mLightModel;
};

matrix mLightView;
matrix mLightProj;


//-------------------------------------------------------------------------------------------------
// Input/output structs
//-------------------------------------------------------------------------------------------------
struct VSInput
{
	float4 p			: POSITION;
	float4 screenPos	: SV_POSITION;
};

struct VSOutput
{
	float4 pCS			: SV_Position;
	float2 tc			: TEXCOORD0;	
};

struct PSOutput
{
	float4 Color		: SV_Target0;
};

//-------------------------------------------------------------------------------------------------
// Textures
//-------------------------------------------------------------------------------------------------
Texture2D       NormalTexture					: register( t0 );
Texture2D		DiffuseAlbedoTexture			: register( t1 );
Texture2D		SpecularAlbedoTexture			: register( t2 );
Texture2D		PositionTexture					: register( t3 );
Texture2D		DepthTexture					: register( t4 );

float4x4 testMat = {1,0,0,0, 
					0,1,0,1, 
					0,0,1,0, 
					0,0,0,1};

//-------------------------------------------------------------------------------------------------
// Vertex shader entry point
//-------------------------------------------------------------------------------------------------
VSOutput VSSimple( VSInput input )
{
	VSOutput output = (VSOutput)0;
	
	//set position into camera clip space	
	output.pCS = input.p;

	return output;
}

//-------------------------------------------------------------------------------------------------
// Sphere vertex shader entry point
//-------------------------------------------------------------------------------------------------
VSOutput VSSphere( VSInput input )
{
	VSOutput output = (VSOutput)0;
	
	float4 pos = input.p;

#ifdef EXERCISE

	// TODO 2:
	//  - transform sphere to view space

#else

	// transform sphere to proj-view space
	pos = mul(pos, mLightModel);
	pos = mul(pos, mLightView);
	pos = mul(pos, mLightProj);

#endif

	//set position into camera clip space	
	output.pCS = pos;

	return output;
}

//-------------------------------------------------------------------------------------------------
// Cone vertex shader entry point
//-------------------------------------------------------------------------------------------------
VSOutput VSCone( VSInput input )
{
	VSOutput output = (VSOutput)0;
	
	float4 pos = input.p;

#ifdef EXERCISE

	// TODO 2:
	//  - transform cone to view space

#else

	// transform sphere to proj-view space
	pos = mul(pos, mLightModel);
	pos = mul(pos, mLightView);
	pos = mul(pos, mLightProj);

#endif

	//set position into camera clip space	
	output.pCS = pos;

	return output;
}


//-------------------------------------------------------------------------------------------------
// Fragment Shader for ambient light 
//-------------------------------------------------------------------------------------------------
float4 PSAmbient(in VSOutput input, in float4 screenPos : SV_Position ) : SV_Target
{
	int3 samplePos  = int3( screenPos.xy, 0 );
	float4 normal	= NormalTexture.Load( samplePos );
	if(normal.w==0) 
		discard;

	float4 diffuseAlbedo	= DiffuseAlbedoTexture.Load( samplePos );
	return vLightAmbient * diffuseAlbedo.w;
}


//-------------------------------------------------------------------------------------------------
// Calculates the directional lighting 
//------------------------------------------------------------------------------------------------
float3 CalcDirLighting( in float3 normal, 
						in float3 position, 
						in float3 diffuseAlbedo,
						in float4 specularAlbedo)
{

#ifdef EXERCISE

	// TODO 1: 
	//  - calculate directional lighting

	return float3(1.0, 1.0, 1.0);

#else

	// Light direction is explicit for directional lights
	float3 L = -normalize(vLightDir);		
	float3 V = normalize( vEyePos - position );
	float3 H = normalize( V + L );
	float3 N = normal.xyz;
	
	float3 Id = diffuseAlbedo * saturate( dot(N,L) );
	float3 Is = specularAlbedo.rgb * pow( saturate( dot(N,H) ), specularAlbedo.w);
	
	return  (Id + Is) * vLightColor.rgb;

#endif

}


//-------------------------------------------------------------------------------------------------
// Pixel Shader for directional light
//-------------------------------------------------------------------------------------------------
float4 PSDirLight(in VSOutput input, in float4 screenPos : SV_Position ) : SV_Target
{

#ifdef EXERCISE

	// This method draws a screen-filling quad, to perform lighting at every pixel in screen-space.
	// TODO 1: 
	//  - use float4 screenPos : SV_Position to get an integer screen-space position
	//  - call CalcLightingDir() and use it instead of lightingReturn

	float4 dummy = float4(1,1,1,1);	
	return float4( CalcDirLighting( dummy.xyz, dummy.xyz, dummy.xyz, dummy ), 1.0f );

#else

	// Get values per pixel from the G-Buffer
	// use float4 screenPos : SV_Position to get an integer screen-space position
	int3 samplePos = int3( screenPos.xy, 0 );

	float4 normal			= NormalTexture.Load( samplePos );
	
	// if the w-component of normal is 0, there is no geometry, 
	// so return just diffuse albedo, which is the clear-color
	if(normal.w==0) 
		discard;

	float4 position			= PositionTexture.Load( samplePos );
	float4 diffuseAlbedo = DiffuseAlbedoTexture.Load( samplePos );	
	float4 specularAlbedo	= SpecularAlbedoTexture.Load( samplePos );			

	// Calculate lighting for a single G-Buffer sample	
	return float4( CalcDirLighting( normal.xyz, position.xyz, diffuseAlbedo.xyz, specularAlbedo ), 1.0f);

#endif

}


//-------------------------------------------------------------------------------------------------
// Current implementation - point light
//-------------------------------------------------------------------------------------------------
float3 CalcPointLighting(	in float3 normal, 
							in float3 position, 
							in float3 diffuseAlbedo,
							in float4 specularAlbedo)
{

#ifdef EXERCISE

	// TODO 1: 
	//  - calculate point light lighting using LightParams

	return float3(1.0, 1.0, 1.0);

#else

	// Calculate the diffuse term
	float3 L = 0;

	// Base the the light vector on the light position
	L = vLightPos - position;

	// Calculate attenuation based on distance from the light source
	float dist = length( L );
	float attenuation_factor = 1/(vLightAtt.x + dist * vLightAtt.y + dist * dist * vLightAtt.z); //clamp?

	L /= dist; //normalization (?)

	float3 V = normalize( vEyePos - position );
	float3 H = normalize( V + L );
	float3 N = normal.xyz;
		
	float3 Id = diffuseAlbedo * saturate( dot(N,L) );	
	float3 Is = specularAlbedo.rgb * pow( saturate( dot(N,H) ), specularAlbedo.w);
	
	return  (Id + Is) * vLightColor.rgb * attenuation_factor;

#endif

}

//-------------------------------------------------------------------------------------------------
// Fragment Shader for point light
//-------------------------------------------------------------------------------------------------
float4 PSPointLight(in VSOutput input, in float4 screenPos : SV_Position ) : SV_Target
{

#ifdef EXERCISE

	// TODO 2: 
	float4 dummy = float4(1,1,1,1);	
	return float4( CalcPointLighting( dummy.xyz, dummy.xyz, dummy.xyz, dummy ), 1.0f );

#else

	int3 samplePos			= int3( screenPos.xy, 0 );

	float4 normal			= NormalTexture.Load( samplePos );
	
	// if the w-component of normal is 0, there is no geometry, 
	// so return just diffuse albedo, which is the clear-color
	if(normal.w==0) 
		discard;

	float4 position			= PositionTexture.Load( samplePos );
	float4 diffuseAlbedo	= DiffuseAlbedoTexture.Load( samplePos );
	float4 specularAlbedo	= SpecularAlbedoTexture.Load( samplePos );			

	// Calculate lighting for a single G-Buffer sample	
	return float4( CalcPointLighting( normal.xyz, position.xyz, diffuseAlbedo.xyz, specularAlbedo ), 1.0f );
	
#endif
}


//-------------------------------------------------------------------------------------------------
// Compute the spotlight
//-------------------------------------------------------------------------------------------------
float3 CalcSpotLighting(	in float3 normal, 
							in float3 position, 
							in float3 diffuseAlbedo,
							in float4 specularAlbedo)
{

#ifdef EXERCISE

	// TODO 1: 
	//  - calculate point light lighting using LightParams

	return float3(1.0, 1.0, 1.0);

#else

	///TODO ANYWAY!!!
	return float3(1.0, 1.0, 1.0);

#endif

}

//-------------------------------------------------------------------------------------------------
// Fragment Shader for spot light
//-------------------------------------------------------------------------------------------------
float4 PSSpotLight(in VSOutput input, in float4 screenPos : SV_Position ) : SV_Target
{

#ifdef EXERCISE

	// TODO 3: 

	float4 dummy = float4(1,1,1,1);	
	return float4( CalcSpotLighting( dummy.xyz, dummy.xyz, dummy.xyz, dummy ), 1.0f );

#else
		
	int3 samplePos			= int3( screenPos.xy, 0 );

	float4 normal			= NormalTexture.Load( samplePos );
	
	// if the w-component of normal is 0, there is no geometry, 
	// so return just diffuse albedo, which is the clear-color
	if(normal.w==0) 
		discard;

	float4 position			= PositionTexture.Load( samplePos );
	float4 diffuseAlbedo	= DiffuseAlbedoTexture.Load( samplePos );
	float4 specularAlbedo	= SpecularAlbedoTexture.Load( samplePos );		
	return float4( CalcSpotLighting( normal.xyz, position.xyz, diffuseAlbedo.xyz, specularAlbedo ), 1.0f );
#endif
}





//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------
technique11 RenderDeferredLighting
{
	pass AmbientPass
    {
		SetRasterizerState	( RSBack );
		SetDepthStencilState( DSSNoDepth, 0);
	    SetBlendState		( BSNoBlending, float4( 1.0f, 1.0f, 1.0f, 1.0f ), 0xffffffff );
        SetVertexShader		( CompileShader( vs_4_0, VSSimple() ) );
        SetHullShader		( NULL );
        SetDomainShader		( NULL );
        SetGeometryShader	( NULL );        
		SetPixelShader		( CompileShader( ps_4_0, PSAmbient() ) );        
    }

	pass DirectionalLightPass
    {
		SetRasterizerState	( RSBack );
		SetDepthStencilState( DSSNoDepth, 0);
	    SetBlendState		( BSAdditive, float4( 1.0f, 1.0f, 1.0f, 1.0f ), 0xffffffff );	
        SetVertexShader		( CompileShader( vs_4_0, VSSimple() ) );
        SetHullShader		( NULL );
        SetDomainShader		( NULL );
        SetGeometryShader	( NULL );      
		SetPixelShader		( CompileShader( ps_4_0, PSDirLight() ) );        
    }

	pass PointLightPass
    {
		SetRasterizerState	( RSFront );
		SetDepthStencilState( DSSNoDepth, 0);
	    SetBlendState		( BSAdditive, float4( 1.0f, 1.0f, 1.0f, 1.0f ), 0xffffffff ); //float4: blendFactor
        SetVertexShader		( CompileShader( vs_4_0, VSSphere() ) );
        SetHullShader		( NULL );
        SetDomainShader		( NULL );
        SetGeometryShader	( NULL );      
		SetPixelShader		( CompileShader( ps_4_0, PSPointLight() ) );        
    }  
		
	pass SpotLightPass
    {
		SetRasterizerState	( RSFront );
		SetDepthStencilState( DSSNoDepth, 0);
	    SetBlendState		( BSAdditive, float4( 1.0f, 1.0f, 1.0f, 1.0f ), 0xffffffff ); //float4: blendFactor
        SetVertexShader		( CompileShader( vs_4_0, VSCone() ) );
        SetHullShader		( NULL );
        SetDomainShader		( NULL );
        SetGeometryShader	( NULL );      
		SetPixelShader		( CompileShader( ps_4_0, PSSpotLight() ) );        
    } 

}



