#version 330 core

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

    vec3 lighting = Albedo;
    
    // diffuse
    vec3 lightDir = normalize(vec3(0.0) - FragPos);
    vec3 diffuse = max(dot(Normal, lightDir), 0.0) * Albedo;
    lighting += diffuse;

	fragColor = vec4(lighting, 1.0);
}