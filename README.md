# 🚀 .NET Real World Playbook

A collection of real-world .NET concepts explained with practical examples, clean C# code, and production-focused insights.

This repository is built alongside a LinkedIn series where each post focuses on solving real development problems using simple and effective approaches.

🔗 Follow me on LinkedIn for regular updates: [LinkedIn | Akash Shah](https://www.linkedin.com/in/akashshah12/)

---

## 🎯 What You’ll Find Here

* Common mistakes developers make ❌
* Best practices used in real projects ✅
* Clear explanations of core concepts 🧠
* Practical C# implementations 💻

---

## 🗂️ Repository Structure

```
dotnet-real-world-playbook/
│
├── posts/                                        ← README + LinkedIn post per topic
│   ├── post-01-singleton-dbcontext/
│   │   └── README.md
│   ├── post-02-async-await/
│   │   └── README.md
│   └── post-03-ihttpclientfactory/
│       └── README.md
│
├── src/
│   └── DotNetConceptLab/                         ← Single ASP.NET Core Web API project
│       │
│       ├── Posts/                                ← All post code lives here
│       │   │
│       │   ├── Post01_SingletonDbContext/         ← Post #1
│       │   │   ├── Controllers/
│       │   │   │   └── Post01OrderController.cs   route: api/p01/orders
│       │   │   ├── Data/
│       │   │   │   └── AppDbContext.cs
│       │   │   └── Services/
│       │   │       ├── IOrderLogService.cs
│       │   │       └── OrderLogService.cs
│       │   │
│       │   ├── Post02_AsyncAwait/                 ← Post #2
│       │   │   ├── Controllers/
│       │   │   │   └── Post02OrderController.cs   route: api/p02/orders
│       │   │   ├── Models/
│       │   │   │   └── Models.cs
│       │   │   ├── Repositories/
│       │   │   │   └── Repositories.cs
│       │   │   └── Services/
│       │   │       ├── IOrderService.cs
│       │   │       └── OrderService.cs
│       │   │
│       │   └── Post03_HttpClientFactory/          ← Post #3
│       │       ├── Controllers/
│       │       │   └── Post03OrderController.cs   route: api/p03/orders
│       │       ├── Models/
│       │       │   └── Models.cs
│       │       ├── Services/
│       │       │   ├── IOrderService.cs
│       │       │   ├── OrderApiClient.cs          typed HttpClient
│       │       │   └── OrderService.cs
│       │       └── Wrong/                         ⚠️ examples only — not in DI
│       │           └── WrongApproaches.cs
│       │
│       ├── appsettings.json
│       └── Program.cs                             ← Single entry point, all DI
│
└── README.md
```

---

## 💻 Where to Find the Code

All runnable code is inside:

👉 `src/DotNetConceptLab`

Each concept is implemented using:

* Controllers → API endpoints
* Services → Business logic
* Data → In-memory / demo data

---

## 📅 Post Index

| Post | Topic                             | Type       | Code Path                                   | Link        |
| --- | --------------------------------- | ---------- | ------------------------------------------- | ----------- |
| 1 | [Using AddSingleton with DbContext](posts/post-01-singleton-dbcontext/README.md) | Mistake    | `src/DotNetConceptLab/Posts/Post01_SingletonDbContext/Data/AppDbContext.cs` | [LinkedIn](https://www.linkedin.com/posts/akashshah12_dotnet-aspnetcore-dependencyinjection-activity-7445168434578870273-Ceu_?utm_source=share&utm_medium=member_desktop&rcm=ACoAAAyuQhMBu9hN6cIx0wrI0m-m65rzhE-5JoU) |
| 2 | [What async/await Really Does (Misunderstood)](posts/post-02-async-await/README.md) | Concept + Mistakes | `src/DotNetConceptLab/Posts/Post02_AsyncAwait/` | [LinkedIn](https://www.linkedin.com/posts/akashshah12_dotnet-csharp-asyncawait-activity-7450022630835015680-6UfL?utm_medium=ios_app&rcm=ACoAAAyuQhMBu9hN6cIx0wrI0m-m65rzhE-5JoU) |
| 3 | [IHttpClientFactory — Why `new HttpClient()` is a production bug](posts/post-03-ihttpclientfactory/README.md) | Mistake | `src/DotNetConceptLab/Posts/post-03-ihttpclientfactory/` | [LinkedIn](https://www.linkedin.com/posts/akashshah12_dotnet-aspnetcore-csharp-share-7461835354304811009-rw7G?utm_source=share&utm_medium=member_desktop&rcm=ACoAAAyuQhMBu9hN6cIx0wrI0m-m65rzhE-5JoU) |

> This table will be updated as new posts are added.

---

## 🔍 How to Use This Repository

1. Pick a topic from the table
2. Read the explanation inside `/posts/post-x/...`
3. Explore the actual implementation inside `/src/DotNetConceptLab/Posts/`
4. Run the project and test APIs

---

## ▶️ Run the Project

```bash
cd src/DotNetConceptLab
dotnet run
```

Then open Swagger:

```
https://localhost:<port>/swagger
```

---

## 📌 Why This Repository Exists

Most tutorials explain syntax.
This repository focuses on:

👉 Writing **correct, scalable, real-world code**

---

## 🔗 Connect

Follow along with the LinkedIn series and explore each concept with working examples.

---

## ⭐ Support

If you find this helpful:

* ⭐ Star the repository
* 🔁 Share with others
* 💬 Give feedback

---

Stay tuned 🔥
