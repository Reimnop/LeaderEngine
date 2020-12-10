#version 450 core

layout (location = 0) in vec3 aPos;

uniform mat4 v;
uniform mat4 p;

out mat4 fragView;
out mat4 fragProj;

out vec3 nearPoint;
out vec3 farPoint;

vec3 UnprojectPoint(float x, float y, float z, mat4 view, mat4 projection) {
    mat4 viewInv = inverse(view);
    mat4 projInv = inverse(projection);
    vec4 unprojectedPoint = vec4(x, y, z, 1.0) * projInv * viewInv;
    return unprojectedPoint.xyz / unprojectedPoint.w;
}

void main() 
{
    fragView = v;
    fragProj = p;
    nearPoint = UnprojectPoint(aPos.x, aPos.y, 0.0, v, p).xyz;
    farPoint = UnprojectPoint(aPos.x, aPos.y, 1.0, v, p).xyz;
	gl_Position = vec4(aPos, 1.0);
}