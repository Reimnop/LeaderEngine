#version 330 core

layout (location = 0) out vec4 fragColor;

uniform int useTexture;
uniform sampler2D texture0;

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

	vec3 norm = normalize(Normal);
	vec3 lightDir = normalize(lightPos - FragPos);

	float diff = max(dot(norm, lightDir), 0.0);
	vec3 diffuse = diff * lightColor;

	vec3 result = (ambient + diffuse) * vec3(objectColor);

	fragColor = vec4(result, 1.0);
}
