#include <iostream>
#include "Application.h"
#include "Component.h"
#include "GameObject.h"

class ccc : public LeaderEngine::Component {
public:
	void start() {
		std::cout << "ccc" << std::endl;
	}
};

void onLoad() {
	LeaderEngine::GameObject* go = new LeaderEngine::GameObject("does it work?");
	go->addComponent(new ccc());

	auto c = go->getComponent<ccc>();
	c->start();
}

int main(void) 
{
	LeaderEngine::Application* app = new LeaderEngine::Application();
	app->start(1280, 720, "Hello, World!", &onLoad);
}