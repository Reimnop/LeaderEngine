#include "GameObject.h"
#include "../Application.h"

using namespace LeaderEngine;

GameObject::GameObject(const char* name) {
	this->name = (char*)name;
	Application::instance->gameObjects.push_back(this);
	start();
}

GameObject::~GameObject() {
	Application::instance->gameObjects.remove(this);

	delete[] name;
}

void GameObject::start() {
	transform = new Transform();
	addComponent(transform);

	setActive(true);
}

void GameObject::update() {
	if (!active)
		return;

	for (auto& comp : components)
		comp.second->update();
	for (auto& comp : components)
		comp.second->lateUpdate();
}

void GameObject::render() {
	if (!active)
		return;

	if (!vertArray || !shader)
		return;

	shader->use();
	vertArray->use();

	glDrawElements(GL_TRIANGLES, vertArray->indices.size(), GL_UNSIGNED_INT, (void*)0);
}

void GameObject::onClosing() {
	for (auto& comp : components)
		comp.second->onClosing();

	delete shader;
	delete vertArray;
}

void GameObject::setActive(bool active) {
	this->active = active;
}

//methods for the entity component system
void GameObject::addComponent(Component* component) {
	if (typeid(*component) == typeid(Transform))
		return;

	components[typeid(*component)] = component;
	component->start();
}

void GameObject::addComponents(std::vector<Component*> components) {
	for (auto comp : components)
		if (typeid(*comp) == typeid(Transform))
			return;

	for (auto comp : components)
		this->components[typeid(*comp)] = comp;

	for (auto comp : components)
		comp->start();
}

template<typename T>
void GameObject::removeComponent() {
	if (typeid(T) == typeid(Transform))
		return;

	components.erase(typeid(T));
}