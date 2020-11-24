#include <iostream>
#include "Application.h"
#include "Component.h"
#include "GameObject.h"

int main(void) 
{
	LeaderEngine::Application* app = new LeaderEngine::Application();
	app->start(1280, 720, "Hello, World!", NULL);
}