#version 330 core

layout (location = 0) out vec4 fragColor;

uniform int useTexture;

uniform mat4 lightRotMat;

uniform sampler2D texture0;
uniform sampler2DShadow shadowMap;

in vec3 VertCol;
in vec3 Normal;
in vec2 TexCoord;
in vec4 FragPosLightSpace;

in vec3 FragPos;

vec3 ambient = vec3(0.5, 0.5, 0.5);

float intensity = 1.5;

float ShadowCalculation(vec4 fragPosLightSpace)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w * 0.5 + 0.5;
    return shadow2D(shadowMap, vec3(projCoords.xy, projCoords.z - 0.0005), 0.0).r; 
}  

void main() 
{
	vec4 objectColor;

	if (useTexture == 1)
		objectColor = texture(texture0, TexCoord) * vec4(VertCol, 1.0);
	else 
		objectColor = vec4(VertCol, 1.0);

	vec3 norm = normalize(Normal);

	float shadow = ShadowCalculation(FragPosLightSpace);  

	vec3 result = clamp(ambient + shadow * max(dot(norm, normalize(vec3(vec4(0.0, 0.0, 1.0, 1.0) * lightRotMat))), 0.0) * intensity, 0.0, 1.0) * vec3(objectColor);

	fragColor = vec4(result, 1.0);
}
