# Prerequisites #
SFML.Net: https://github.com/LaurentGomila/SFML.Net

CSFML: https://github.com/LaurentGomila/CSFML

SFML.Net contains latest Windows CSFML libraries, so CSFML is only needed for Linux build.

Tao framework: http://www.mono-project.com/Tao. I'll most likely change that to OpenTK later.

_You no longer need to modify SFML.Net, Gwen.Net works with standard SFML libraries._

# Linux #

You need to compile SFML and CSFML. Renderer needs following libraries in exe's directory (or otherwise accessible):
```
libsfml-graphics.so.2 (SFML)
libsfml-window.so.2 (SFML)
libsfml-system.so.2 (SFML)
libcsfml-graphics-2.so (CSFML)
libcsfml-window-2.so (CSFML)
sfmlnet-graphics-2.dll (SFML.Net)
sfmlnet-window-2.dll (SFML.Net)
Tao.OpenGl.dll
Tao.OpenGl.dll.config
Tao.OpenGl.ExtensionLoader.dll
Tao.OpenGl.ExtensionLoader.dll.config
```

I also needed to set `MONO_WINFORMS_XIM_STYLE=disabled` variable, otherwise it crashed on startup.