# Minutnik (Avalonia .NET)

Aplikacja minutnika (technika Pomodoro) odmierzająca czas pracy i przerwy, zbudowana zgodnie z wytycznymi lekcji *„Pierwsza aplikacja budowana w całości z agentem”* (Zadanie 3.1).

---

## 1. Pomiary i limity modelu (Zadanie 3.1)

- **Data pomiaru:** 19 listopada 2026 r.
- **Narzędzie / środowisko agentowe:** Google Antigravity
- **Użyty model:** Gemini 3.8 Flash (Google DeepMind)

### Cztery liczby z lekcji:

1. **Liczba tokenów pierwszej wiadomości po polsku:**
   - **Tekst:** *„Zbuduj minutnik w tym projekcie Avalonia. Ma odmierzać 25 minut pracy, potem 5 minut przerwy, i liczyć skończone cykle. Obie długości mają być do zmiany w oknie. Jeden przycisk start/pauza, drugi reset. Czas widoczny jako mm:ss, licznik cykli widoczny obok. Po skończonej pracy przerwa ma ruszyć sama. Interfejs po polsku, nazwy w kodzie po angielsku, bez dodatkowych bibliotek. Cała logika wprost w pliku MainWindow.axaml.cs, bez osobnych klas pośredniczących.”* (461 znaków)
   - **Liczba tokenów (Tokenizer Playground / cl100k_base):** **163 tokeny** (średnio ok. 2,83 znaku na token).
   - *(Dla porównania w tokenizerze o200k_base: 146 tokenów).*

2. **Liczba tokenów pierwszej wiadomości po angielsku:**
   - **Tekst:** *„Build a timer in this Avalonia project. It should count 25 minutes of work, then 5 minutes of break, and count completed cycles. Both lengths should be changeable in the window. One start/pause button, the other reset. Time visible as mm:ss, cycle counter visible next to it. After work is finished, the break should start on its own. Interface in Polish, code names in English, without additional libraries. All logic directly in the MainWindow.axaml.cs file, without separate intermediary classes.”* (499 znaków)
   - **Liczba tokenów (Tokenizer Playground / cl100k_base):** **105 tokenów** (średnio ok. 4,75 znaku na token).
   - *(Dla porównania w tokenizerze o200k_base: 106 tokenów).*
   - **Wniosek:** Tekst w języku polskim potrzebuje o ok. 55% więcej tokenów niż jego angielski odpowiednik o tej samej objętości informacyjnej, ponieważ polskie morfemy i znaki diakrytyczne częściej ulegają fragmentacji na mniejsze pod-słowa.

3. **Okno kontekstu modelu (Input Context Window):**
   - **Wartość:** **1 000 000 tokenów** (1M tokenów; dla wybranych modeli z rodziny Gemini do 2 000 000 tokenów).
   - **Link do dokumentacji:** [Google Gemini API Models Documentation](https://ai.google.dev/gemini-api/docs/gemini-3) (oraz [ai.google.dev/gemini-api/docs/models/gemini](https://ai.google.dev/gemini-api/docs/models/gemini)).
   - *(Dla modeli Claude podanych w lekcji: 1 000 000 tokenów – [Dokumentacja Claude Models Overview](https://platform.claude.com/docs/en/about-claude/models/overview)).*

4. **Maksymalna długość odpowiedzi modelu (Max Output Tokens):**
   - **Wartość:** **65 536 tokenów** (64k tokenów).
   - **Link do dokumentacji:** [Google Gemini API Limits & Capabilities](https://ai.google.dev/gemini-api/docs/gemini-3).
   - *(Dla modeli Claude podanych w lekcji: 128 000 tokenów – [Dokumentacja Claude Models Overview](https://platform.claude.com/docs/en/about-claude/models/overview)).*

---

## 2. Wynik próby: wpisanie `25,5` oraz `25.5` w pole długości czasu

Test przeprowadzony w systemie z domyślną kulturą polską (`pl-PL`):

1. **Wpisanie `25,5` (z przecinkiem):**
   - W kulturze `pl-PL` przecinek jest standardowym separatorem dziesiętnym.
   - Wywołanie `double.TryParse("25,5", out ...)` (lub `double.Parse("25,5")`) interpretuje wartość poprawnie jako **25,5 minuty** (czyli 25 minut i 30 sekund). Po resecie lub przeliczeniu na wyświetlaczu pojawia się czas **`25:30`**.
   - *(Gdyby pole było parsowane za pomocą `int.Parse("25,5")`, rzucony zostałby wyjątek `System.FormatException`, gdyż nie jest to liczba całkowita).*

2. **Wpisanie `25.5` (z kropką):**
   - W kulturze `pl-PL` kropka nie jest traktowana jako separator dziesiętny (w polskich ustawieniach regionalnych kropka bywa separatorem tysięcy lub znakiem niepoprawnym dla formatu liczby zmiennoprzecinkowej).
   - Wywołanie `double.Parse("25.5")` rzuca krytyczny wyjątek:
     ```text
     System.FormatException: The input string '25.5' was not in a correct format.
     ```
     powodując zatrzymanie lub awarię aplikacji (crash), jeśli wyjątek nie jest przechwycony.
   - Przy bezpiecznym parsowaniu za pomocą `double.TryParse("25.5", out ...)` metoda zwraca `false` (błąd parsowania). Aplikacja nie ulega awarii, lecz odrzuca wartość i pozostaje przy poprzedniej (bądź domyślnej) wartości 25 minut.

---

## 3. Siedem zasad spełnionych w aplikacji

1. **Długość pracy i przerwy:** Domyślnie 25 minut pracy i 5 minut przerwy; obie wartości można swobodnie modyfikować w polach okna (`WorkTimeTextBox`, `BreakTimeTextBox`).
2. **Przyciski sterowania:** Jeden wielofunkcyjny przycisk `Start`/`Pauza` (zmieniający etykietę w zależności od stanu pracy) oraz drugi przycisk `Reset`.
3. **Licznik cykli:** Wyświetlany obok czasu (`Ukończone cykle: X`).
4. **Samoczynny start przerwy:** Po upływie czasu sesji pracy licznik cykli zwiększa się o 1, stan przełącza się na przerwę, a odliczanie przerwy rozpoczyna się automatycznie bez konieczności ponownego klikania Start.
5. **Format czasu:** Czas wyświetlany w formacie `mm:ss` (np. `25:00`, `04:59`).
6. **Język:** Cały interfejs użytkownika w języku polskim, nazwy klas, pól i metod w kodzie w języku angielskim.
7. **Brak dodatkowych bibliotek:** Wyłącznie czyste komponenty Avalonia i biblioteki standardowe platformy .NET.
8. **Decyzja techniczna i wątki:** Cała logika zamknięta bezpośrednio w pliku `MainWindow.axaml.cs` bez klas pośredniczących, a jako zegar użyto `Avalonia.Threading.DispatcherTimer` (wykluczający błędy *„Call from invalid thread”*).

---

## 4. Własną ręką: historia cykli

W projekcie zaimplementowano blok rozszerzenia z listy:
- W `MainWindow.axaml` dodano kontrolkę `<ListBox Name="HistoryList" />`.
- W `MainWindow.axaml.cs` utworzono kolekcję `private readonly List<string> _history = new();`.
- Tuż za linijką inkrementującą licznik cykli dopisano:
  ```csharp
  _history.Add($"{DateTime.Now:HH:mm} — cykl {_cycles}");
  HistoryList.ItemsSource = _history.ToArray();
  ```
- Zmiana została zatwierdzona dedykowanym commitem: `wlasna reka: historia cykli`.

---

## 5. Historia commitów

Repozytorium zawiera przejrzystą historię kroków:
1. `Inicjalizacja projektu minutnik (Avalonia)` – czysty szablon Avalonia .NET.
2. `Zbuduj minutnik: 7 zasad (DispatcherTimer, cala logika w MainWindow.axaml.cs)` – implementacja 7 zasad przez agenta.
3. `wlasna reka: historia cykli` – manualne dodanie ListBox i historii cykli.
4. `README: tokeny, limity modelu i test wartosci dziesietnych` – dokumentacja pomiarów, tokenów, limitów kontekstu i analizy formatów liczb.
