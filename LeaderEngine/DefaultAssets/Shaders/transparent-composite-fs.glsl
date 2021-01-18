#version 330 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D colorTexture;
uniform sampler2D accumTex;
uniform sampler2D revealTex;

in vec2 TexCoord;

void main() 
{
	ivec2 bufferCoords = ivec2(gl_FragCoord.xy);
    vec4 accum = texelFetch(accumTex, bufferCoords, 0);
    float revealage = accum.a;

    accum.a = texelFetch(revealTex, bufferCoords, 0).r;

    // suppress underflow
    if (isinf(accum.a)) {
        accum.a = max(max(accum.r, accum.g), accum.b);
    }

    // suppress overflow
    if (any(isinf(accum.rgb))) {
        accum = vec4(isinf(accum.a) ? 1.0 : accum.a);
    }

    vec3 dst = (accum.rgb / accum.a) * (1.0 - revealage) + texture(colorTexture, TexCoord).rgb * revealage;

    fragColor = vec4(dst, 1.0);
}