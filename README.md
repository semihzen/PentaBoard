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
<img width="1920" height="1028" alt="image" src="https://github.com/user-attachments/assets/cf2af8eb-9ebf-42e8-a57b-070c872d1f75" />

<img width="1920" height="1029" alt="image" src="https://github.com/user-attachments/assets/4feb21e0-af46-4341-a40a-f2435db475ea" />
<img width="1920" height="1026" alt="image" src="https://github.com/user-attachments/assets/12b11b80-0730-42eb-a065-f1560575a0fe" />
<img width="1920" height="1028" alt="image" src="https://github.com/user-attachments/assets/967d3f29-b8aa-4729-8bb4-40a3544b50bc" />
<img width="1920" height="1028" alt="image" src="https://github.com/user-attachments/assets/371569a7-6945-4a9a-84cb-6e91d0fadd1e" />
<img width="1920" height="1031" alt="image" src="https://github.com/user-attachments/assets/c8d49109-80b1-4860-ba09-12d6edb9036a" />
<img width="1920" height="1027" alt="image" src="https://github.com/user-attachments/assets/7e32a804-ff0b-42cf-b671-cf341b927ee8" />


## 📦 Kurulum
```bash
# Backend için
cd backend
cd backend PentaBoard.Api
dotnet run

# Frontend için
cd frontend
cd penta-board-frontend
npm install
npm start











