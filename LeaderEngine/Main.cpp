#include <iostream>
#include "LeaderEngine/Application.h"

int main(void) 
{
	LeaderEngine::Application* app = new LeaderEngine::Application();
	app->start(1280, 720, "Hello, World!", NULL);
}