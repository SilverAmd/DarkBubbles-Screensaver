# Dark Bubbles Screensaver

This project provides a Windows screensaver concept focused on a dark, slow-moving bubble/balloon display to minimize burn-in risk on OLED/VA/IPS panels while still keeping the screen alive.

## Features

- Wait time before activation
- immediate stop on mouse movement or keyboard input
- fully fullscreen screensaver mode
- optional balloon-style shapes instead of bubbles
- dark mode and subtle glow
- fits typical Windows screensaver launch conventions (`/s`, `/c`, `/p`)

## Build

```bash
dotnet build -c Release
```

## Run

```bash
dotnet run -- --help
```

For a screensaver, the executable can be renamed to `.scr` and launched in Windows with the standard screensaver options, or started with commands such as:

```bash
DarkBubblesScreensaver.exe /s
DarkBubblesScreensaver.exe /c
```

## Notes

This is intentionally designed as a dark, moving overlay rather than a static screenshot, which is safer for burn-in-prone displays. It is a practical compromise between a live desktop effect and display safety.
