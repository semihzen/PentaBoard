# PentaBoard ( Task & Project Management Platform )

Bu proje, ekiplerin projelerini ve görevlerini yönetebilmesi için geliştirilmiş bir *iş yönetim platformudur*.  
Amaç: Proje oluşturma, kullanıcı daveti, rol/sub-rol atama ve görev (task) takibini kolaylaştırmak.

---

## 🚀 Teknolojiler
- *Frontend:* React  
- *Backend:* .NET 8 (Vertical Slice Architecture)  
- *Database:* SQL Server  
- *Auth:* JWT Token tabanlı kimlik doğrulama  
- *Diğer:* Swagger (API dokümantasyonu), Jest/xUnit (testler)

---

## 🔑 Özellikler
- *Authentication & Authorization:*  
  - JWT token ile güvenli login/kayıt.  
  - Rol bazlı yetkilendirme (Ana Admin, Proje Admini, Kullanıcı).  
  - Sub-roller: Backend Developer, Frontend Developer, Tester, Analyst.  

- *Proje Yönetimi:*  
  - Proje oluşturma, listeleme, güncelleme, silme.  
  - Proje Admini kullanıcıları  mail yolu ile davet edebilir ve roller atayabilir.  

- *Görev (Task) Yönetimi:*  
  - Task CRUD (oluşturma, güncelleme, silme).  
  - Durum takibi (Planned, Ongoing, Test, Publish, Done).  
  - Tag ekleme, filtreleme.  
  - Dosya ekleme/indirme desteği.  

- *Ekip Yönetimi:*  
  - Projedeki kullanıcıların ve rollerinin listelenmesi.  
  - Proje sahibinin görünürlüğü.  

- *Dashboard:*  
  - Kullanıcı kendi projelerini ve görevlerini görebilir.  
  - Task durum özetleri görüntülenebilir.  

---

## ⚙ Mimari
Proje, *Vertical Slice Architecture* ile kurgulanmıştır:  
- Her feature/modül kendi sorumluluklarıyla ayrılır.  
- Daha modüler, bakımı kolay ve genişletilebilir yapı sağlar.  

---

## 🔒 JWT Kullanımı
- Login sonrası kullanıcıya bir *JWT token* üretilir.  
- Token içerisinde kullanıcı ID ve rol bilgileri tutulur.  
- Her API çağrısında bu token gönderilerek kimlik doğrulama yapılır.  

---

## 📦 Kurulum
```bash
# Backend için
cd backend
dotnet run

# Frontend için
cd frontend
npm install
npm start
