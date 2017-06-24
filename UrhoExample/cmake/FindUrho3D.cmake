# output:
# URHO3D_LIBS
# install_urho3d()

set(URHO3D_DIR "c:/Program Files/Urho3D" CACHE PATH "Path to Urho3D SDK")

if(NOT EXISTS "${URHO3D_DIR}/include/Urho3D/Urho3D.h")
	message(FATAL_ERROR "Urho3D SDK not found. Set 'URHO3D_DIR' variable.")
endif()

include_directories("${URHO3D_DIR}/include" "${URHO3D_DIR}/include/Urho3D/ThirdParty")
set(URHO3D_LIBS "${URHO3D_DIR}/lib/Urho3D.lib")

function(install_urho3d)
    install(FILES "${URHO3D_DIR}/bin/Urho3D.dll" DESTINATION ".")
	install(FILES "${URHO3D_DIR}/bin/d3dcompiler_47.dll" DESTINATION ".")
	install(DIRECTORY "${URHO3D_DIR}/share/Resources/CoreData" DESTINATION ".")
    install(FILES "${URHO3D_DIR}/share/Resources/Data/Textures/LogoLarge.png" DESTINATION "Data/Textures")
    install(FILES "${URHO3D_DIR}/share/Resources/Data/Textures/UrhoIcon.png" DESTINATION "Data/Textures")
    install(FILES "${URHO3D_DIR}/share/Resources/Data/Models/Box.mdl" DESTINATION "CoreData/DefaultModels")
endfunction()
