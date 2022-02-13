#version 330 core

in vec3 aPosition;
in vec4 rectangleColor;

out vec4 vertexColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
    vertexColor = rectangleColor;
}