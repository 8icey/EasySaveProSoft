# EasySaveProSoft

EasySaveProSoft is a powerful C# WPF backup software designed for professionals, offering features like real-time parallel execution, file encryption, remote control, and intelligent priority and network-aware backup management.

---

## 🚀 Features

### ✅ Core Functionality
- Full and differential backup modes
- Parallel execution of backup jobs (multi-threaded)
- Real-time progress per job in a dedicated UI
- File encryption based on extension

### 📂 File Priority System
- Set a list of prioritized file extensions (e.g., `.pdf`, `.docx`)
- Non-priority files wait until all priority files are backed up
- Priority list is ordered and respected during execution

### 🌐 Network Load Throttling
- Define a network usage threshold (KB/s)
- Automatically reduce the number of parallel jobs when network is overloaded

### 🧠 Smart Execution Controls
- Block execution if specified software (e.g., `discord.exe`) is running
- Pause, resume, and stop backups manually or remotely
- Large file handling using semaphores

### ⚙️ Settings and Configuration
- Fully configurable via the Settings UI
- All preferences stored in `config.ini` and JSON files:
  - Language (`en`, `fr`)
  - Log format (`json`, `xml`)
  - Priority extensions (with drag-and-drop ordering)
  - Blocked software
  - Network threshold & max jobs

### 🌍 Localization
- Multi-language support (English and French)
- All labels dynamically switchable in real time

### 🔧 Logging
- Logs saved in JSON or XML based on user preference
- Each file transfer is logged with duration, size, encryption info

### 📡 Remote Control
- Control the app via TCP client
- Supported commands: `pause`, `resume`, `stop`

---

## 🖼️ Interface Overview

- **Main Window:** List and control of all backup jobs
- **Settings Window:** Configure language, logs, encryption, network, software rules
- **Parallel Execution Window:** Displays live progress for each running job

---

## 🛠️ Technologies

- **C# / .NET 8**
- **WPF (MVVM)**
- **JSON.NET (Newtonsoft)**
- **Multithreading & Tasks**
- **TCP Sockets**

---

## 📁 Configuration Files

| File | Purpose |
|------|---------|
| `config.ini` | Language, log format, priority order, thresholds |
| `EncryptionExtensions.json` | File types to encrypt |
| `BlockedProcesses.json` | List of processes that block backups |
| `jobs.json` | Saved backup jobs |

---

## 🧪 How to Run

1. Open the solution in Visual Studio 2022+
2. Set `EasySaveProSoftWPF` as the startup project
3. Build and run the project
4. Use the **Settings** view to define your preferences
5. Create and execute jobs from the main view

---

## 🧑‍💻 Contributing

Want to help improve EasySaveProSoft? Fork the project and submit a PR!

---

## 📄 License

CESI.

---

## 👥 Authors

- Noufel Ouanoughi ,Kouba Amine, Hachi Idris and Bensalem Mohammed
- Developed as part of a 2025 Software Engineering project

---
