#version 450 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec3 aColor;
layout (location = 3) in vec2 aTexCoord;

uniform mat4 model;

uniform mat4 mvp;
uniform mat4 lightSpaceMat;

out vec3 Color;
out vec2 TexCoord;

out vec3 Normal;
out vec3 FragPos;

out float Depth;

void main() {
	Color = aColor;
	TexCoord = aTexCoord;

	Normal = aNormal * mat3(transpose(inverse(model)));
	FragPos = vec3(vec4(aPosition, 1.0) * model);

	gl_Position = vec4(aPosition, 1.0) * mvp;
}