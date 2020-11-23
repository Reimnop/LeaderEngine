#include <iostream>
#include "Application.h"

void onLoad() {
	
}

int main(void) 
{
	LeaderEngine::Application* app = new LeaderEngine::Application();
	app->start(1280, 720, "Hello, World!", &onLoad);
}