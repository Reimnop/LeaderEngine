#version 330 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D albedo;
uniform sampler2D accumTex;
uniform sampler2D revealTex;
uniform sampler2D depthMap;

in vec2 TexCoord;

void main() 
{
	vec4 outColor = vec4(texture(albedo, TexCoord).rgb, 1.0);

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

    vec3 avgColor = accum.rgb / max(accum.a, 1e-4);
    // dst' = (accum.rgb / accum.a) * (1 - revealage) + dst * revealage
    vec4 trans = vec4(avgColor, revealage);

    fragColor = mix(outColor, trans, 1.0 - revealage);

	gl_FragDepth = texture(depthMap, TexCoord).r;
}