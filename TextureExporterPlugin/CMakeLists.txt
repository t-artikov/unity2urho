cmake_minimum_required(VERSION 3.0)
project(TextureExporterPlugin)

set(CMAKE_INSTALL_PREFIX "${CMAKE_CURRENT_SOURCE_DIR}/../UnityProject") 
set(CMAKE_INSTALL_MESSAGE LAZY)
set(CMAKE_VS_INCLUDE_INSTALL_TO_DEFAULT_BUILD TRUE)

add_subdirectory(DirectXTex)

set(TARGET TextureExporterPlugin)
add_library(${TARGET} MODULE main.cpp)
target_link_libraries(${TARGET} DirectXTex)
install(TARGETS ${TARGET} DESTINATION "Assets/Plugins/x86_64")
