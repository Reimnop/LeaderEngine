#version 430 core

layout (location = 0) in vec3 aPos;

uniform mat4 mvp;

out vec3 TexCoord;

void main() {
	TexCoord = aPos;
	vec4 pos = vec4(aPos, 1.0) * mvp;
	gl_Position = pos.xyww;
}