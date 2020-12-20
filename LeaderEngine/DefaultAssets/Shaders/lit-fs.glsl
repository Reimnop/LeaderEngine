#version 330 core

layout (location = 0) out vec4 fragColor;

uniform int useTexture;

uniform sampler2D texture0;
uniform sampler2D shadowMap;

in vec3 VertCol;
in vec3 Normal;
in vec2 TexCoord;
in vec4 FragPosLightSpace;

in vec3 FragPos;

vec3 ambient = vec3(0.2, 0.2, 0.2);
vec3 lightColor = vec3(1.0, 1.0, 1.0);
vec3 lightPos = vec3(0.0, 0.0, 0.0);

float ShadowCalculation(vec4 fragPosLightSpace)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;

    float closestDepth = texture(shadowMap, projCoords.xy).r; 
    float currentDepth = projCoords.z;
    float shadow = currentDepth - 0.05 > closestDepth  ? 1.0 : 0.0;

    return shadow;
}  

void main() 
{
	vec4 objectColor;

	if (useTexture == 1)
		objectColor = texture(texture0, TexCoord) * vec4(VertCol, 1.0);
	else 
		objectColor = vec4(VertCol, 1.0);

	vec3 norm = normalize(Normal);
	vec3 lightDir = normalize(lightPos - FragPos);

	float diff = max(dot(norm, lightDir), 0.0);
	vec3 diffuse = diff * lightColor;

	float shadow = ShadowCalculation(FragPosLightSpace);  

	vec3 result = (ambient + (1.0 - shadow) * diffuse) * vec3(objectColor);

	fragColor = vec4(result, 1.0);
}
