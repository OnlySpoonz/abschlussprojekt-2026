# Dungeon Generator – Editor-Tool für Unity

Praktische Umsetzung im Rahmen der Facharbeit **„Analyse und Evaluation des Einsatzes hausinterner proprietärer Tools in der Spieleentwicklung mit Fokus auf Environment-Tools"**.

Ein Editor-Tool für Unity, mit dem Game Designer – **ohne Programmierkenntnisse** – prozedural generierte Dungeons erstellen und konfigurieren können. Das Tool nutzt **Binary Space Partitioning (BSP)**, um Räume zu erzeugen, verbindet sie über Korridore und klassifiziert sie automatisch nach Raumtyp.

## Features

- **Eigenes Editor-Window** unter `Praxisarbeit → DungeonGenerator` mit zwei Tabs: *Dungeon Settings* und *Room Settings*
- **BSP-basierte Raumaufteilung** – rekursive Unterteilung des Dungeon-Grundstücks in rechteckige Bereiche
- **Automatische Raum-Klassifizierung** in sechs Raumtypen: Spawn, Boss, Treasure, Puzzle, Shop, NPC (+ Standard-Fallback)
- **ScriptableObject-basierte Konfiguration**:
  - `RoomData` – Prefabs (Floor/Wall/Ceiling/Door), Wandhöhe, Spawn-Content-Regeln und Spawn-Wahrscheinlichkeit pro Raumtyp
  - `Room Type Library` – Sammlung aller RoomData-Objekte, referenziert vom Generator
- **Konfigurierbare Generierungsparameter**: Dungeon Width/Length, Min Room Size, Max Split Depth, Corridor Width (Small/Medium/Large), Fixed Seed für reproduzierbare Ergebnisse, Random Room Sizes
- **JSON-Import/Export** – Raumkonfiguration und Dungeon-Layout können gesichert und wiederhergestellt werden
- **Automatische Validierung** mit klaren Fehlermeldungen (z.B. fehlender Dungeon Parent, ungültige Dungeon-Größe, zu hohe Split Depth)
- **Live-Updates** – Änderungen an RoomData wirken sich bei bestehendem Dungeon sofort auf die Szene aus, ohne erneute Generierung

## Technischer Hintergrund

Dieses Tool dient als praktischer Nachweis der in der theoretischen Facharbeit untersuchten Konzepte hausinterner Environment-Tools – u.a. anhand von Analysen zu *Binding of Isaac* (parametrische Raumanzahl-Formel, BFS-Expansion, seed-basierte Determinismus) und *Skyrim* (Kit-basierte Kombinationsregeln, "high bang-for-buck"-Prinzip).

## Verwendung

Eine ausführliche Anleitung für Game Designer liegt im Repository unter [`Anleitung.pdf`](./Anleitung.pdf) und beschreibt:

1. Öffnen des Tools (`Praxisarbeit → DungeonGenerator`)
2. Zuweisen von Dungeon Parent und Room Type Library
3. Konfiguration von Größe, Korridoren und Seed
4. Einrichten der Raumtypen in *Room Settings*
5. Generieren, Prüfen und Sichern des Dungeons

### Empfohlener Workflow (Kurzfassung)

1. Dungeon Parent & Room Type Library zuweisen
2. Dungeon Width/Length, Min Room Size und Max Split Depth festlegen
3. Corridor Width auf *Medium* setzen (empfohlener Standardwert)
4. Bei Bedarf Fixed Seed aktivieren
5. Raumtypen in *Room Settings* konfigurieren
6. **Generate Dungeon** klicken
7. Stand über **Export** sichern

## Autor

Philip Nitschke – Game Design, Macromedia Akademie München
Betreuung: Herr Kohl

## Lizenz

"nur zu Bildungszwecken"
