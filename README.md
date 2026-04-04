# 🛡️ VisaBot: Macaristan Vize Kotası Radar Sistemi

Bu proje, **AS Visa Solutions** (Macaristan) randevu sistemini 7/24 izleyen ve randevu kotası açıldığında anında Telegram üzerinden bildirim gönderen profesyonel bir **.NET 10.0 Worker Service** uygulamasıdır.

## 🎯 Projenin Amacı
Bu proje, tamamen **eğitim amaçlı** ve vize randevu sistemlerinin işleyişini anlamak, .NET arka plan servisleri (Worker Services) ile tarayıcı otomasyonu (Playwright) arasındaki entegrasyonu tecrübe etmek için geliştirilmiştir. 
- Herhangi bir ticari gelir amacı gütmez.
- Kişisel bir yardımcı araç (assistant) olarak tasarlanmıştır.

## 🚀 Teknolojiler
- **Backend:** .NET 10.0 (C#)
- **Otomasyon:** Playwright .NET (Browser Automation)
- **Bildirim:** Telegram Bot API
- **Loglama:** Serilog (Structured Logging)
- **Mimari:** Background Worker Service, Dependency Injection, JSON Configuration

## ✨ Özellikler
- **Anti-Bot Koruması:** İnsansı gecikmeler (Random Jitter) ve gerçekçi User-Agent kullanımı.
- **Düşük Kaynak Tüketimi:** Headless (görünmez) tarayıcı modu ile 7/24 stabil çalışma.
- **Akıllı Alarm:** Kota açıldığında hem sesli (`Console.Beep`) hem de mobil bildirim.
- **Güvenli Mimari:** Hassas veriler (Token, ID) `appsettings.json` ile yönetilir.

## 🛠️ Kurulum
1. Projeyi klonlayın: `git clone https://github.com/kullaniciadi/VisaBot.git`
2. Bağımlılıkları yükleyin: `dotnet restore`
3. Playwright tarayıcılarını kurun: 
   `pwsh bin/Debug/net10.0/playwright.ps1 install chromium`
4. `appsettings.example.json` dosyasını `appsettings.json` olarak kopyalayın ve kendi bilgilerinizi girin.
5. Çalıştırın: `dotnet run`

## 👨‍💻 Geliştirici
**İbrahim** - YBS Öğrencisi & Backend Geliştirici Adayı

Legal Disclaimer: This project is for educational purposes only. The developer is not responsible for any misuse of this tool or any violations of the target website's Terms of Service. Use at your own risk.