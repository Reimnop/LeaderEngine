#version 430 core

layout (location = 0) out vec4 fragColor;

in vec3 Color;
in vec2 TexCoord;

in vec3 Normal;
in vec3 FragPos;

uniform bool hasDiffuse;
uniform sampler2D diffuse;

uniform vec3 color;

uniform vec3 camPos;

//light uniforms
uniform sampler2D shadowMap;
uniform vec3 lightDir;
uniform float bBias = 0.002;
uniform float lightIntensity = 1.0;

uniform vec3 ambient = vec3(0.05);

in vec4 FragPosLightSpace;

float calculateShadow(vec4 fragPosLightSpace) {
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w * 0.5 + 0.5;

	if(projCoords.z > 1.0)
        return 1.0;

	vec3 norm = normalize(Normal);

	float bias = max(bBias * (1.0 - dot(norm, lightDir)), bBias);

	float shadow = 0.0;
	vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
	for(int x = -1; x <= 1; ++x)
	{
		for(int y = -1; y <= 1; ++y)
		{
			float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; 
			shadow += projCoords.z - bias > pcfDepth ? 0.0 : 1.0;        
		}    
	}
	shadow /= 9.0;

    return shadow;
}

void main() {
	vec3 obColor = color;
	if (hasDiffuse)
		obColor *= texture(diffuse, TexCoord).rgb;

	vec3 norm = normalize(Normal);

	float diffuse = max(dot(norm, lightDir), 0.0) * lightIntensity;
	float shadow = (calculateShadow(FragPosLightSpace));

	vec3 outColor = (ambient + diffuse * shadow) * obColor;

	fragColor = vec4(outColor, 1.0);
}