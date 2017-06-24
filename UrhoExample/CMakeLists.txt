cmake_minimum_required(VERSION 3.0)
project(UrhoExample)
set(CMAKE_INSTALL_PREFIX "${CMAKE_SOURCE_DIR}/bin")
set(CMAKE_INSTALL_MESSAGE LAZY)
set(CMAKE_VS_INCLUDE_INSTALL_TO_DEFAULT_BUILD TRUE)

set(CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake" ${CMAKE_MODULE_PATH})
find_package(Urho3D)

file(GLOB SRC "src/*")
set(TARGET UrhoExample)
add_executable(${TARGET} ${SRC})
target_link_libraries(${TARGET} ${URHO3D_LIBS})

install_urho3d()
install(TARGETS ${TARGET} DESTINATION ".")
install(DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/data/" DESTINATION "CoreData")

