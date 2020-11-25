#include "logger.h"

void Logger::log(const char* msg) {
	time_t now = time(0);
	tm* ltm = new tm;
	localtime_s(ltm, &now);

	std::cout << "[" << ltm->tm_hour << ":" << ltm->tm_min << ":" << ltm->tm_sec << "] " << msg << std::endl;

	delete ltm;
}