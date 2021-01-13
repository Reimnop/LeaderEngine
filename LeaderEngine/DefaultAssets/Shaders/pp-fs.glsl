#version 400 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D gAlbedoSpec;
uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D depthMap;

in vec2 TexCoord;

void main() 
{
    vec3 Albedo = texture(gAlbedoSpec, TexCoord).rgb;
    vec3 FragPos = texture(gPosition, TexCoord).rgb;
    vec3 Normal = texture(gNormal, TexCoord).rgb;

	fragColor = vec4(Albedo, 1.0);
}