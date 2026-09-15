# Forklift RoboRally - Design Planning

## Concept

A multiplayer game inspired by RoboRally's programmed-movement mechanic. Players control forklifts, competing to grab shared delivery boxes and return them to their own spawn point to score.

## Round Structure

1. **Programming phase**: every player is dealt 5 random cards from a shared deck.
   - Each player may **mulligan once per round**: choose any number of individual cards from their hand to discard and redraw fresh replacements. Free, no cost or downside.
   - After mulliganing (or choosing not to), players secretly choose the order to play their 5 cards (all simultaneously, no one sees anyone else's hand or order until execution).
2. **Execution phase**, repeated for each of the 5 card slots in order:
   - Players resolve their card for this slot **one after another** (not simultaneously), in turn order.
   - Each forklift's action (move/rotate, including any pushes/carries it triggers) fully resolves before the next player's forklift acts.
   - Once every player has resolved this slot's card, conveyor belts trigger (`RunConveyerBelts`), moving anything sitting on a belt.
   - Move to the next card slot and repeat.
3. Once all 5 slots are resolved, the round ends. **Turn order (who goes first) rotates to the next player** for the following round, then a new hand is dealt and the round repeats.

## Card Types

Mapped to mechanics already implemented:
- Move Forward 1 / 2 / 3 (`Forklift.MoveForward(value)`)
- Rotate Clockwise / Anticlockwise (`Forklift.RotateClockwise` / `RotateAnticlockwise`)
- Move Backward (`MoveForward(-1)`, already supported)
- *Possibly* U-Turn (180°), not currently a single action, would need a new method or two chained rotates

## Goal / Scoring

- Delivery boxes are **shared**, not tied to any one player.
- The number of boxes in play at once is `floor(players / 2)`, so there's always some scarcity/competition.
- Boxes spawn onto a **conveyor belt spawner**, which feeds into a **circular loop of conveyor belts**, boxes continuously circulate around this loop on their own until a player intervenes.
- A player scores by pushing a box off the loop and maneuvering it back to **their own spawn point**.
- When a box is delivered (scored), it's removed and a new box spawns via the spawner to keep the loop's box count topped back up to the target count.
- **Win condition**: first player to reach a score target (delivered box count) wins immediately, ending the game. Exact target number still TBD.

## Board

No RoboRally-style hazards (walls, pits, lasers, repair stations). Kept minimal: tiles, conveyor belts, and boxes only.

- **Level size scales with player count**: `boardSize = 6 + playerCount` (square grid, both width and height), e.g. 8x8 for 2 players, up to 14x14 for the full 8 players.

## Multiplayer

- **Online**, via the existing PurrNet networking setup already integrated in the project (WebRTC P2P, PurrLobby).
- Each player controls their own Forklift.
- **Player count**: up to 8, matching the existing lobby's configured limit (`LobbyProvider.Jam.asset`, `_maxPlayers: 8`).

## Networking Approach

The `Forklift`/`Level`/`Element` push/rotate system built so far is entirely local, client-side logic (no `NetworkIdentity`/`NetworkTransform`/RPCs yet). Since one player's card can push or rotate *another* player's forklift, a purely per-owner-authoritative model doesn't work here (a client can't be trusted to move another player's object). Approach:

- **Host-authoritative resolution**: the host runs the actual simulation, all `PushElement`/`RotateForklift`/`SettleForklifts`/`RunConveyerBelts` logic executes only on the host, using the same `Level`/`Element` code that already exists.
- **Clients only submit their programmed card order** for their own forklift (an RPC to the host once locked in, after their own mulligan/ordering choices, all done locally/privately on their own client first).
- **Host broadcasts results**: as each card slot resolves for each player in turn order, the host sends the resulting position/rotation for every affected forklift/box to all clients, who play back the same tween animations already built to visually match, rather than re-simulating the logic themselves.
- Forklifts (and boxes) should **not** use owner-authoritative `NetworkTransform` (unlike the old `FreeCamPlayer` setup), their transform needs to be host/server-driven since other players' actions can move them.

## Open Questions / Not Yet Decided

- Exact RPC/message shapes for submitting card orders and broadcasting per-step resolution results, not yet scoped at the code level.
- Whether "host" here means one of the players' own clients (peer-hosted, matching the existing WebRTC P2P/PurrLobby setup) or a dedicated server, current assumption is peer-hosted since that's what's already integrated.
