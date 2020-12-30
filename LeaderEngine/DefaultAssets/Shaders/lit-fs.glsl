#version 330 core

layout (location = 0) out vec4 fragColor;

uniform int useTexture;

uniform vec3 lightDir;

uniform sampler2D texture0;
uniform sampler2DShadow shadowMap;

in vec3 VertCol;
in vec3 Normal;
in vec2 TexCoord;
in vec4 FragPosLightSpace;

in vec3 FragPos;

vec3 ambient = vec3(0.9, 0.9, 0.9);
vec3 lightColor = vec3(1.0, 1.0, 0.95);

float intensity = 1.4;

float ShadowCalculation(vec4 fragPosLightSpace)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w * 0.5 + 0.5;

	if(projCoords.z > 1.0)
        return 1.0;

    return shadow2D(shadowMap, vec3(projCoords.xy, projCoords.z - 0.001), 0.0).r; 
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

	vec3 result = (ambient + shadow * max(dot(norm, lightDir), 0.0) * lightColor * intensity) * vec3(objectColor);

	fragColor = vec4(result, 1.0);
}
