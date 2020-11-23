#include "Behaviour.h"
#include "Application.h"

using namespace LeaderEngine;

Behaviour::Behaviour() {
	onStart();
	Application::instance->behaviours.push_back(this);
}

Behaviour::~Behaviour() {
	Application::instance->behaviours.remove(this);
}