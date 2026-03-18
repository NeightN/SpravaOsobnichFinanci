# Správa osobních financí

Tento repozitář obsahuje zdrojové kódy desktopové aplikace pro správu osobních financí. Projekt vznikl jako praktická část bakalářské práce na téma **"Správa osobních financí v C#"**.

---

# Personal Finance Management

This repository contains the source code of a desktop application for personal finance management. The project was created as the practical part of a bachelor’s thesis titled **"Personal Finance Management in C#"**.

## 📖 O projektu
Aplikace slouží k evidenci a správě každodenních příjmů a výdajů. Poskytuje uživateli přehled o jeho finanční situaci a umožňuje kategorizaci jednotlivých transakcí. Aplikace je navržena s důrazem na jednoduchost a přehlednost.

## 📖 About the project
The application is used to record and manage everyday income and expenses. It provides the user with an overview of their financial situation and allows categorization of individual transactions. The app is designed with an emphasis on simplicity and clarity.

## ✨ Hlavní funkce
- **Evidence transakcí**: Přidávání, úprava a mazání příjmů a výdajů.
- **Správa kategorií**: Možnost vytvářet a upravovat vlastní kategorie pro detailní přehled o tocích financí.
- **Export dat**: Ukládání uživatelských dat (transakcí a příslušných detailů) do formátů `CSV` (pro MS Excel apod.) a `JSON`.
- **Uživatelská nastavení**: Volba výchozí měny (Kč, €, $, £) s okamžitým propisem napříč aplikací.
- **Správa dat**: Bezpečné zálohování a možnost kompletního smazání dat do továrního nastavení (Factory Reset).

## ✨ Key features
- **Transaction tracking**: Add, edit, and delete income and expense transactions.
- **Category management**: Create and edit custom categories for a detailed overview of cash flow.
- **Data export**: Save user data (transactions and related details) in `CSV` (e.g., for MS Excel) and `JSON` formats.
- **User settings**: Choose a default currency (CZK, EUR, USD, GBP) with instant propagation across the app.
- **Data management**: Secure backups and the option to delete all data and restore factory settings (Factory Reset).

## 🛠️ Použité technologie
- **Jazyk**: C# 13.0
- **Framework**: .NET 9.0
- **Uživatelské rozhraní**: WPF (Windows Presentation Foundation)
- **Architektura**: MVVM (Model-View-ViewModel)

## 🛠️ Technologies used
- **Language**: C# 13.0
- **Framework**: .NET 9.0
- **UI**: WPF (Windows Presentation Foundation)
- **Architecture**: MVVM (Model-View-ViewModel)

## 🚀 Spuštění projektu
1. Naklonujte si tento repozitář: git clone https://github.com/NeightN/SpravaOsobnichFinanci.git
2. Otevřete řešení (`.sln`) ve Visual Studiu 2022.
3. Obnovte NuGet balíčky, pokud je to vyžadováno.
4. Spusťte projekt stisknutím __F5__ nebo z nabídky __Ladit > Spustit ladění__.

## 🚀 Running the project
1. Clone this repository: git clone https://github.com/NeightN/SpravaOsobnichFinanci.git
2. Open the solution (`.sln`) in Visual Studio 2022.
3. Restore NuGet packages if required.
4. Run the project by pressing __F5__ or via __Debug > Start Debugging__.

## 📄 Licence
Tento projekt byl vytvořen pro akademické účely jako bakalářská práce.

## 📄 License
This project was created for academic purposes as a bachelor’s thesis.

---

# Screens

Dashboard

[Dashboard](https://i.imgur.com/fOuqXUF.png)

Transaction List

[Transaction List](https://i.imgur.com/fZtb0El.png)

Transaction Editor

[Transaction Editor](https://i.imgur.com/ujEAcSk.png)

Categories

[Categories](https://i.imgur.com/77xBKzo.png)

Category Editor

[Category Editor](https://i.imgur.com/DxoSyC9.png)

Settings

[Settings](https://i.imgur.com/Te1rShZ.png)
