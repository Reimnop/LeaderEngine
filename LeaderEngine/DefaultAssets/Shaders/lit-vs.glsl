#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aVertCol;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec2 aTexCoord;

out vec3 VertCol;

out vec2 TexCoord;

out vec3 NormalWorldSpace;
out vec3 FragPosWorldSpace;

out vec3 NormalViewSpace;
out vec3 FragPosViewSpace;

uniform mat4 mvp;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() 
{
	VertCol = aVertCol;

	TexCoord = aTexCoord;

	NormalWorldSpace = mat3(transpose(inverse(model))) * aNormal;
	FragPosWorldSpace = vec3(vec4(aPos, 1.0) * model);

	NormalViewSpace = vec3(vec4(mat3(transpose(inverse(model))) * aNormal, 1.0) * view);
	FragPosViewSpace = vec3(vec4(aPos, 1.0) * model * view);

	gl_Position = vec4(aPos, 1.0) * mvp;
}