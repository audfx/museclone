#version 330
#extension GL_ARB_separate_shader_objects : enable

layout (location = 0) in vec3 in_Position;
layout (location = 1) in vec2 in_TexCoord;

out gl_PerVertex
{
	vec4 gl_Position;
};

layout (location = 1) out vec2 frag_TexCoord;

uniform mat4 Projection;
uniform mat4 Camera;
uniform mat4 World;

uniform float Scale;
uniform float HeadPosition;
uniform float Completion;

void main()
{
	frag_TexCoord = in_TexCoord;

	vec2 scaleFactor = mix(1, 0.5 - in_Position.z * 0.5, Completion) * vec2(1, 0.93);
	vec3 newPosition = vec3(in_Position.xy * scaleFactor, min(0, HeadPosition + Scale * in_Position.z));

	gl_Position = Projection * Camera * World * vec4(newPosition, 1);
}