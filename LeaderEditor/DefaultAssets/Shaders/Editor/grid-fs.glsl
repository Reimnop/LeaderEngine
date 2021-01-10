#version 450 core

layout (location = 0) out vec4 fragColor;

in vec2 TexCoord;

in mat4 fragView;
in mat4 fragProj;

in vec3 nearPoint;
in vec3 farPoint;

vec4 grid(vec3 fragPos3D, float scale) {
    vec2 coord = fragPos3D.xz * scale;
    vec2 derivative = fwidth(coord);
    vec2 grid = abs(fract(coord - 0.5) - 0.5) / derivative;
    float line = min(grid.x, grid.y);
    float minimumz = min(derivative.y, 1.0);
    float minimumx = min(derivative.x, 1.0);
    vec4 color = vec4(0.2, 0.2, 0.2, 1.0 - min(line, 1.0));

    vec2 coordMajor = coord / 10.0;
    vec2 derivativeMajor = fwidth(coordMajor);
    vec2 gridMajor = abs(fract(coordMajor - 0.5) - 0.5) / derivativeMajor;
    float lineMajor = min(gridMajor.x, gridMajor.y);

    if (int(lineMajor) == 0)
        color.xyz = vec3(0.4);

    if(fragPos3D.x > -1.0 * minimumx && fragPos3D.x < 1.0 * minimumx)
        color.xyz = vec3(0.2, 0.2, 1.0);

    if(fragPos3D.z > -1.0 * minimumz && fragPos3D.z < 1.0 * minimumz)
        color.xyz = vec3(1.0, 0.2, 0.2);

    return color;
}

float computeDepth(vec3 pos) {
    vec4 clip_space_pos = vec4(pos, 1.0) * fragView * fragProj;
    return clip_space_pos.z / clip_space_pos.w;
}

void main() 
{
	float t = -nearPoint.y / (farPoint.y - nearPoint.y);
    vec3 fragPos3D = nearPoint + t * (farPoint - nearPoint);

    gl_FragDepth = computeDepth(fragPos3D) * 0.5 + 0.5;

	fragColor = grid(fragPos3D, 1.0) * float(t > 0.0);
}