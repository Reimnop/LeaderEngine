#version 450 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec3 aColor;
layout (location = 3) in vec2 aTexCoord;
layout (location = 4) in vec3 aTangent;

uniform mat4 model;

uniform mat4 mvp;
uniform mat4 lightSpaceMat;

uniform vec3 viewPos;
uniform vec3 lightDir;

out vec3 Color;
out vec2 TexCoord;

out vec3 Normal;
out vec3 FragPos;

out mat3 TBN;

void main() {
	Color = aColor;
	TexCoord = aTexCoord;

	Normal = aNormal * mat3(transpose(inverse(model)));
	FragPos = vec3(vec4(aPosition, 1.0) * model);

	vec3 T = normalize(vec3(vec4(aTangent,   0.0) * model));
	vec3 N = normalize(vec3(vec4(aNormal,    0.0) * model));
	T = normalize(T - dot(T, N) * N);
	vec3 B = cross(N, T);

	TBN = mat3(T, B, N);

	gl_Position = vec4(aPosition, 1.0) * mvp;
}