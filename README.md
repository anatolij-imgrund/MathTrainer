# MathTrainer

MathTrainer ist eine Desktop-App, mit der Grundschulkinder Kopfrechnen üben. Das Kind wählt aus, was es trainieren möchte, und bekommt dann eine endlose Reihe zufälliger Aufgaben – eine nach der anderen, jede mit einem kurzen Countdown –, bis es selbst auf „Stop“ drückt. Für richtige Antworten gibt es Punkte und Medaillen, die man gegen Belohnungen eintauschen kann, die die Eltern festlegen.

Entstanden im 3. Semester meiner Ausbildung zum Fachinformatiker für Anwendungsentwicklung – für meine eigenen Kinder.

## So funktioniert das Training

Vor dem Start stellt das Kind drei Dinge ein:

| Einstellung | Auswahl |
|---|---|
| Rechenart | Addition (+) oder Subtraktion (−) |
| Zahlenbereich bis | 10, 20, 50 oder 100 |
| Was wird gesucht? | das Ergebnis `1 + 2 = ?`, die erste Zahl `? + 2 = 3` oder die zweite Zahl `1 + ? = 3` |

Danach geht es los:
- Die App erzeugt zufällige Aufgaben, die immer im gewählten Zahlenbereich bleiben und keine negativen Ergebnisse haben.
- Für jede Aufgabe gibt es **10 Sekunden**. Ist die Zeit um, zeigt die App die richtige Antwort und macht mit der nächsten Aufgabe weiter.
- Die Antwort wird über ein großes Ziffernfeld auf dem Bildschirm eingegeben – ideal für kleine Hände und Touchscreens.
- Nach jeder Antwort kommt sofort eine Rückmeldung („Richtig“ oder die richtige Lösung).

**Punkte und Medaillen**
- Eine richtige Antwort bringt 1 Punkt. Wird eine der beiden Zahlen gesucht (nicht das Ergebnis) und reicht der Zahlenbereich bis mindestens 20, gibt es 2 Punkte, im Bereich bis 100 sogar 3 Punkte.
- Ein Fortschrittsbalken füllt sich mit jeder richtigen Antwort. Ist er voll (100 richtige Antworten), gibt es eine Medaille 🏅.
- Punkte, Medaillen und Fortschritt bleiben gespeichert.

## Weitere Funktionen

- **Mehrere Kinder:** Profile mit Namen, Avatar und Hintergrundfarbe anlegen, bearbeiten und löschen
- **Belohnungen 🎁:** Kinder sehen die Belohnungen samt Preis in Medaillen und können sie „bestellen“
- **Eltern-/Lehrerbereich 🏆 (mit Passwort):** Belohnungen mit Bild und Preis anlegen, bearbeiten und löschen, Bestellungen ansehen und als ausgegeben markieren, Passwort ändern

## Technik

| Bereich | Umsetzung |
|---|---|
| Sprache / Plattform | C#, .NET 9 |
| Oberfläche | [Avalonia UI](https://avaloniaui.net/) 11 (AXAML), Fluent-Theme |
| Architektur | Aufteilung in Model, View und Persistence (je ein eigenes Projekt) |
| Datenhaltung | JSON-Dateien im Benutzerordner |

### Projektstruktur

```
MathTrainer.sln
├── MathTrainer.Model/        Kind, Belohnung, Bestellung, Enums (Rechenart, Aufgabentyp)
├── MathTrainer.View/         Fenster (Hauptfenster, Training, Belohnungen, Einstellungen …), Assets
├── MathTrainer.Persistence/  Laden und Speichern als JSON, Passwort
└── MathTrainer.Controller/   (noch leer – war für eine spätere Trennung der Logik gedacht)
```

## Starten

Voraussetzung: [.NET 9 SDK](https://dotnet.microsoft.com/download)

```bash
git clone https://github.com/anatolij-imgrund/MathTrainer.git
cd MathTrainer
dotnet run --project MathTrainer.View
```

Alternativ `MathTrainer.sln` in Visual Studio oder JetBrains Rider öffnen und `MathTrainer.View` als Startprojekt wählen.

- **Standardpasswort für den Eltern-/Lehrerbereich:** `0000` (kann in der App geändert werden)
- **Speicherort der Daten:** `%APPDATA%\MathTrainer\` unter Windows. Zum Zurücksetzen einfach den Ordner löschen.

## Bekannte Grenzen

Das ist ein Lernprojekt aus der Ausbildung. Unter anderem:
- Multiplikation und Division sind im Modell schon vorgesehen, aber noch nicht umgesetzt.
- Die Trainingslogik steckt direkt im Code-Behind der Fenster statt in einer eigenen Schicht.
- Das Passwort liegt im Klartext in einer Datei.
- Es gibt keine automatischen Tests.

## Schriftart

Playpen Sans – The Playpen Sans Project Authors, lizenziert unter der [SIL Open Font License 1.1](https://openfontlicense.org/) (Lizenztext unter `MathTrainer.View/Assets/Fonts/Playpen/OFL.txt`).

## Autor

Imgrund Anatolij – entstanden während der Ausbildung zum Fachinformatiker für Anwendungsentwicklung.
