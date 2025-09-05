# Başarsoft GIS Web Application

This project is a full-stack web application for managing and visualizing geospatial data. It features an interactive map interface for creating, editing, and viewing geographic shapes, along with a robust backend for data storage and validation.

---

Bu proje, coğrafi verileri yönetmek ve görselleştirmek için geliştirilmiş tam donanımlı bir web uygulamasıdır. Coğrafi şekilleri oluşturmak, düzenlemek ve görüntülemek için interaktif bir harita arayüzü ile birlikte veri depolama ve doğrulama için güçlü bir backend'e sahiptir.

## <a name="features"></a>✨ Features / ✨ Özellikler

- **Interactive Map:** View, draw, and edit points, lines, and polygons on a dynamic map powered by OpenLayers.
- **Shape Management:** Full CRUD (Create, Read, Update, Delete) functionality for geographic shapes.
- **Data Grid:** Display shape data in a paginated and searchable table.
- **Shape Merging:** Combine multiple shapes into a single new shape.
- **Image Uploads:** Associate images with shapes.
- **Validation Rules:** Define custom rules to validate newly created shapes against existing ones (e.g., prevent overlapping polygons).
- **Test Data Generation:** Quickly populate the database with randomly generated test shapes for development purposes.

---

- **İnteraktif Harita:** OpenLayers tabanlı dinamik bir harita üzerinde noktaları, çizgileri ve poligonları görüntüleyin, çizin ve düzenleyin.
- **Şekil Yönetimi:** Coğrafi şekiller için tam CRUD (Oluşturma, Okuma, Güncelleme, Silme) işlevselliği.
- **Veri Tablosu:** Şekil verilerini sayfalanmış ve aranabilir bir tabloda görüntüleyin.
- **Şekil Birleştirme:** Birden fazla şekli tek bir yeni şekilde birleştirin.
- **Görsel Yükleme:** Şekillere görseller ekleyin.
- **Doğrulama Kuralları:** Yeni oluşturulan şekilleri mevcut olanlara karşı doğrulamak için özel kurallar tanımlayın (örneğin, çakışan poligonları önleme).
- **Test Verisi Oluşturma:** Geliştirme amacıyla veritabanını hızla rastgele oluşturulmuş test şekilleriyle doldurun.

## <a name="tech-stack"></a>🛠️ Tech Stack / 🛠️ Teknolojiler

### Backend

- **Framework:** ASP.NET Core 8
- **API:** RESTful API
- **Database:** PostgreSQL with PostGIS extension
- **ORM:** Entity Framework Core
- **Geospatial Library:** NetTopologySuite
- **Test Data:** Bogus

### Frontend

- **Framework:** React 19
- **Build Tool:** Vite
- **Language:** TypeScript
- **Mapping Library:** OpenLayers
- **Geometric Operations:** JSTS (JavaScript Topology Suite)
- **UI/Styling:** Basic CSS and Modals

## <a name="setup"></a>🚀 Setup and Installation / 🚀 Kurulum ve Başlatma

### Prerequisites / Ön Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18 or higher)
- [PostgreSQL](https://www.postgresql.org/download/) with [PostGIS](https://postgis.net/install/) extension enabled.

### Backend

1.  **Clone the repository:**
    ```bash
    git clone <repository-url>
    cd <repository-directory>
    ```
2.  **Configure Database Connection:**
    - Open `BaşarsoftStaj/appsettings.json`.
    - Update the `DefaultConnection` string with your PostgreSQL credentials.
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Host=localhost;Database=your_db;Username=your_user;Password=your_password"
    }
    ```
3.  **Apply Migrations:**
    - Navigate to the backend project directory: `cd BaşarsoftStaj`
    - Run the Entity Framework Core migrations to create the database schema.
    ```bash
    dotnet ef database update
    ```
4.  **Run the Backend:**
    ```bash
    dotnet run
    ```
    The backend API will be running at `http://localhost:5294`.

### Frontend

1.  **Navigate to the frontend directory:**
    ```bash
    cd frontend
    ```
2.  **Install Dependencies:**
    ```bash
    npm install
    ```
3.  **Run the Frontend Development Server:**
    ```bash
    npm run dev
    ```
    The frontend application will be available at `http://localhost:5173`.

> **Note:** The frontend is configured to communicate with the backend at `http://localhost:5294`. If you change the backend port, update `API_BASE_URL` in `frontend/src/services/shapeService.ts` and `frontend/src/services/ruleService.ts`.

## <a name="api"></a>🌐 API Endpoints

The backend provides the following main API endpoints under `/api`:

### Shape Controller (`/api/Shape`)

-   `GET /GetAll`: Get all shapes with pagination and search.
-   `GET /GetById/{id}`: Get a shape by its ID.
-   `POST /Add`: Add a new shape (with optional image upload).
-   `POST /UpdateById/{id}`: Update an existing shape.
-   `POST /DeleteById/{id}`: Delete a shape by its ID.
-   `POST /DeleteAll`: Delete all shapes.
-   `POST /Merge`: Merge multiple shapes into a new one.
-   `POST /CreateTestData/{count}`: Generate a specified number of test shapes.

### Rules Controller (`/api/Rules`)

-   Provides full CRUD operations for validation rules.
-   `GET /`: Get all rules.
-   `GET /{id}`: Get a rule by ID.
-   `POST /`: Create a new rule.
-   `PUT /{id}`: Update a rule.
-   `DELETE /{id}`: Delete a rule.
