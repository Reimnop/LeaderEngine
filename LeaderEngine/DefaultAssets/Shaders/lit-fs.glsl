#version 330 core

layout (location = 0) out vec4 gAlbedoSpec;
layout (location = 1) out vec3 gPosition;
layout (location = 2) out vec3 gNormal;

uniform bool useTexture;

uniform vec4 color;
uniform vec3 lightDir;

uniform sampler2D texture0;
uniform sampler2D shadowMap;

in vec3 VertCol;
in vec3 Normal;
in vec2 TexCoord;
in vec4 FragPosLightSpace;

in vec3 NormalWorldSpace;
in vec3 FragPosWorldSpace;

in vec3 FragPos;

vec3 ambient = vec3(0.6, 0.6, 0.6);
vec3 lightColor = vec3(1.0, 1.0, 0.95);

float intensity = 1.4;

float ShadowCalculation(vec4 fragPosLightSpace)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w * 0.5 + 0.5;

	if(projCoords.z > 1.0)
        return 1.0;

	vec3 norm = normalize(Normal);

    return projCoords.z - 0.0005 > texture(shadowMap, projCoords.xy).r ? 0.0 : 1.0;
}  

void main() 
{
	vec4 objectColor = vec4(VertCol, 1.0) * color;

	if (useTexture)
		objectColor *= texture(texture0, TexCoord);

	vec3 norm = normalize(Normal);

	float shadow = ShadowCalculation(FragPosLightSpace);

	vec3 result = (ambient + shadow * max(dot(norm, lightDir), 0.0) * lightColor * intensity) * vec3(objectColor);

	gAlbedoSpec = vec4(result, 1.0);
	gPosition = FragPosWorldSpace;
	gNormal = normalize(NormalWorldSpace);
}
