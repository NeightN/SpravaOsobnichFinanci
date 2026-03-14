# Správa osobních financí

Tento repozitář obsahuje zdrojové kódy desktopové aplikace pro správu osobních financí. Projekt vznikl jako praktická část bakalářské práce na téma **"Správa osobních financí v C#"**.

## 📖 O projektu
Aplikace slouží k evidenci a správě každodenních příjmů a výdajů. Poskytuje uživateli přehled o jeho finanční situaci a umožňuje kategorizaci jednotlivých transakcí. Aplikace je navržena s důrazem na jednoduchost a přehlednost.

## ✨ Hlavní funkce
- **Evidence transakcí**: Přidávání, úprava a mazání příjmů a výdajů.
- **Správa kategorií**: Možnost vytvářet a upravovat vlastní kategorie pro detailní přehled o tocích financí.
- **Export dat**: Ukládání uživatelských dat (transakcí a příslušných detailů) do formátů `CSV` (pro MS Excel apod.) a `JSON`.
- **Uživatelská nastavení**: Volba výchozí měny (Kč, €, $, £) s okamžitým propisem napříč aplikací.
- **Správa dat**: Bezpečné zálohování a možnost kompletního smazání dat do továrního nastavení (Factory Reset).

## 🛠️ Použité technologie
- **Jazyk**: C# 13.0
- **Framework**: .NET 9.0
- **Uživatelské rozhraní**: WPF (Windows Presentation Foundation)
- **Architektura**: MVVM (Model-View-ViewModel)

## 🚀 Spuštění projektu
1. Naklonujte si tento repozitář: git clone https://github.com/NeightN/SpravaOsobnichFinanci.git
2. Otevřete řešení (`.sln`) ve Visual Studiu 2022.
3. Obnovte NuGet balíčky, pokud je to vyžadováno.
4. Spusťte projekt stisknutím __F5__ nebo z nabídky __Ladit > Spustit ladění__.

## 📄 Licence
Tento projekt byl vytvořen pro akademické účely jako bakalářská práce.