#version 330 core

layout (location = 0) out vec4 fragColor;

uniform int useTexture;
uniform sampler2D texture0;

uniform mat4 model;

in vec3 VertCol;
in vec3 Normal;
in vec2 TexCoord;

in vec3 FragPos;

vec3 ambient = vec3(0.2, 0.2, 0.2);
vec3 lightColor = vec3(1.0, 1.0, 1.0);
vec3 lightPos = vec3(0.0, 0.0, 0.0);

void main() 
{
	vec4 objectColor;

	if (useTexture == 1)
		objectColor = texture(texture0, TexCoord) * vec4(VertCol, 1.0);
	else 
		objectColor = vec4(VertCol, 1.0);

	mat3 normalMatrix = transpose(inverse(mat3(model)));
    vec3 normal = normalize(Normal * normalMatrix);

	vec3 surfaceToLight = lightPos - FragPos;

	float brightness = dot(normal, surfaceToLight) / (length(surfaceToLight) * length(normal));
    brightness = clamp(brightness, 0, 1);

	vec3 result = brightness * lightColor * vec3(objectColor) + ambient;

	fragColor = vec4(result, 1.0);
}
