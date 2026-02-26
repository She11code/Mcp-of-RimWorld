# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RimWorld AI Mod provides a WebSocket API that allows external AI agents to control the RimWorld game. AI agents connect to `ws://localhost:8080/ai` and send JSON commands to query game state or control colonists.

## Build

```bash
cd Source && dotnet build -c Release
```

Output: `Assemblies/RimWorldAI.dll`

## Architecture

### Core Flow
```
WebSocket → CommandQueue (ConcurrentQueue, cross-thread)
     ↓
TickManagerPatch (Harmony patch, calls each tick)
     ↓
GameStateQuery.HandleQuery (routes to command handler)
     ↓
GameStateProvider (RimWorld API calls)
```

### Key Files

| File | Purpose |
|------|---------|
| `Source/RimWorldAI.cs` | Mod entry, starts WebSocket server |
| `Source/Core/CommandQueue.cs` | Thread-safe command queue (WebSocket thread → Main thread) |
| `Source/Core/GameStateQuery.cs` | Command routing and JSON parsing/handling |
| `Source/Core/GameStateProvider.cs` | RimWorld API wrappers, all game interaction logic |
| `Source/Core/CommandRegistry.cs` | Command definitions with help text |
| `Source/Core/WebSocketServer.cs` | WebSocket server implementation |
| `Source/Core/TickManagerPatch.cs` | Harmony patch, processes command queue each game tick |

### Adding New API Commands

1. Add command definition to `CommandRegistry.cs`
2. Add implementation method in `GameStateProvider.cs`
3. Add routing case in `GameStateQuery.cs`
4. Build and test

## RimWorld API Reference

- See `反汇编代码/` for ILSpy'decompiled RimWorld API signatures and patterns
- See `docs/RimWorld-API-Reference.md` for API documentation

## JSON Handling

- Uses SimpleJson (custom lightweight JSON library) for serialization
- Arrays from WebSocket are returned as strings (e.g., `"[1,2,3]"`) - need manual parsing with `ParseCellsList()` helper

## Testing

Python test client in `tests/`:
```bash
cd tests
pip install -r requirements.txt
python test_api.py --quick        # Quick test
python test_api.py --control     # Includes control commands
python test_api.py -v            # Verbose output
```

Game must be running with the mod loaded before testing.
