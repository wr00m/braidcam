# braidkit
Command-line tool for manipulating and modifying the puzzle-platform game Braid, designed for exploration, experimentation and theorycrafting.

**Note to users:** This tool modifies game memory at runtime. If you use it in a way that violates any End User License Agreement (EULA), Terms of Service (ToS), or similar policies, you are solely responsible for the consequences. Please use it responsibly.

**Note to speedrunners:** You *have to* restart the game after using this tool, to ensure that the game's code is unmodified, before doing competitive speedrunning.

##
```
braidkit camera-lock                         // Lock camera at current position
braidkit camera-lock 10 20                   // Lock camera at x=10 y=20
braidkit camera-lock toggle                  // Toggle camera lock/unlock
braidkit camera-lock unlock                  // Unlock camera
braidkit camera-zoom 0.5                     // Zoom out camera
braidkit camera-zoom reset                   // Reset camera to default zoom
braidkit tim-position 10 20                  // Move Tim to x=10 y=20
braidkit tim-position 10 20 -r               // Move Tim by x=10 y=20 relative to current position
braidkit tim-velocity 100 200                // Set Tim's velocity to x=100 y=200
braidkit tim-speed 2.0                       // Set Tim's movement speed to 200 %
braidkit tim-jump 1.5                        // Set Tim's jump speed to 150 %
braidkit entity-flag monstar greenglow false // Remove green glow from all goombas
braidkit entity-flag mimic nogravity true    // Disable gravity for all rabbits
braidkit entity-flag guy hidden true         // Make Tim invisible
braidkit bg-full-speed                       // Toggle game running at full speed in background
braidkit il-timer                            // Prints level complete times on door entry (flag levels not supported)
braidkit il-timer --reset-pieces             // Additionally resets all puzzle pieces on level entry
braidkit reset-pieces                        // Removes all puzzle pieces on current save
braidkit debug-info                          // Toggle in-game debug info
braidkit show-hitbox                         // Toggle in-game hitbox overlay
braidkit -h                                  // Show help
```

![Screenshot](braidkit_screenshot.jpg)