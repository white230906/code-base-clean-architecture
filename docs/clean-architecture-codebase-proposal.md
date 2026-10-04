# Đề xuất codebase Clean Architecture

## 1. Mục tiêu

Codebase này là template khởi đầu cho các ASP.NET Core API:

- Giữ đúng chiều phụ thuộc của Clean Architecture.
- Có sẵn các phần dùng lại thường xuyên: exception handling, response model, JWT, cookie, mail, upload media, database, Swagger, CORS và rate limiting.
- Không đưa business logic của một dự án cụ thể vào template.
- Khi mở project, có thể nhanh chóng biết code cần đặt ở đâu.
- Không tạo quá nhiều project hoặc abstraction khi chưa có nhu cầu thực tế.

Template nên có bốn project runtime:

```text
CodeBase.Domain
CodeBase.Application
CodeBase.Infrastructure
CodeBase.API
```

Chiều phụ thuộc:

```text
Domain
  ↑
Application
  ↑
Infrastructure
  ↑
API
```

Hiểu chính xác hơn:

```text
Domain          → không phụ thuộc project nào
Application     → phụ thuộc Domain
Infrastructure  → phụ thuộc Application và Domain
API             → phụ thuộc Application và Infrastructure
```

API là nơi khởi động ứng dụng và ghép các implementation của Infrastructure vào interface của Application.

---

## 2. Nhận xét codebase hiện tại

Code hiện tại đã có nhiều thành phần tái sử dụng tốt:

- JWT authentication và authorization.
- Cookie chứa access token, refresh token.
- Global exception middleware.
- Swagger.
- CORS và rate limiting.
- MailKit, Cloudinary.
- Response envelope và pagination.
- EF Core `AppDbContext`.
- `BaseEntity` và `IAuditableEntity`.

Tuy nhiên, cấu trúc `API → Service → Repo` hiện tại chưa phải Clean Architecture hoàn chỉnh:

1. `CodeBase.Service` vừa chứa abstraction, vừa chứa implementation phụ thuộc MailKit, Cloudinary và ASP.NET Core.
2. `MediaService.IService` nhận `IFormFile`, khiến tầng service bị phụ thuộc HTTP.
3. `ApiResponse` là HTTP contract nhưng đang nằm trong Service.
4. `Repo` đang chứa entity nền tảng; khi áp dụng Clean Architecture, domain object nên thuộc Domain, còn EF Core mapping thuộc Infrastructure.
5. Tên `IService` và `Service` lặp lại ở nhiều namespace, khó tìm kiếm và đọc stack trace.
6. Options đang được bind thủ công trong constructor, chưa validate ngay lúc ứng dụng khởi động.
7. `appsettings` hiện chứa credential thật hoặc có hình thức giống credential thật. Template không được lưu secret; các giá trị này cần được thay và secret đã lộ nên được rotate.
8. Package hiện tại có phiên bản không đồng nhất với `.NET 8`; template mới nên chỉ giữ package đúng layer và cùng major version.
9. `Quartz`, Redis, Cloudinary và Mail không phải dự án nào cũng cần. Có thể chuẩn bị adapter nhưng không đăng ký mặc định nếu feature chưa được bật.

---

## 3. Structure đề xuất

```text
code-base-clean-architecture/
├── CodeBase.sln
├── global.json
├── Directory.Build.props
├── .gitignore
├── .template.config/
│   └── template.json
├── docs/
│   └── clean-architecture-codebase-proposal.md
│
├── CodeBase.Domain/
│   ├── CodeBase.Domain.csproj
│   ├── Common/
│   │   ├── BaseEntity.cs
│   │   ├── IAuditableEntity.cs
│   │   └── ISoftDeletable.cs
│   ├── Entities/
│   └── Enums/
│
├── CodeBase.Application/
│   ├── CodeBase.Application.csproj
│   ├── DependencyInjection.cs
│   │
│   ├── Abstractions/
│   │   ├── Authentication/
│   │   │   ├── ICurrentUser.cs
│   │   │   └── ITokenService.cs
│   │   ├── Communication/
│   │   │   └── IEmailSender.cs
│   │   ├── Media/
│   │   │   └── IFileStorage.cs
│   │   └── Persistence/
│   │       └── IUnitOfWork.cs
│   │
│   ├── Common/
│   │   ├── Exceptions/
│   │   │   ├── AppException.cs
│   │   │   └── ErrorType.cs
│   │   ├── Models/
│   │   │   ├── PaginationRequest.cs
│   │   │   ├── PaginationResult.cs
│   │   │   ├── EmailMessage.cs
│   │   │   └── FileUpload.cs
│   │
│   └── Services/
│       └── Sample/
│           ├── IService.cs
│           ├── Service.cs
│           ├── Request.cs
│           ├── Response.cs
│           └── Validators/
│               └── EchoRequestValidator.cs
│
├── CodeBase.Infrastructure/
│   ├── CodeBase.Infrastructure.csproj
│   ├── DependencyInjection.cs
│   │
│   ├── Authentication/
│   │   ├── JwtOptions.cs
│   │   └── JwtTokenService.cs
│   │
│   ├── Communication/
│   │   ├── MailOptions.cs
│   │   └── SmtpEmailSender.cs
│   │
│   ├── Media/
│   │   ├── CloudinaryOptions.cs
│   │   └── CloudinaryFileStorage.cs
│   │
│   └── Persistence/
│       ├── AppDbContext.cs
│       ├── Configurations/
│       ├── Interceptors/
│       │   └── AuditableEntityInterceptor.cs
│       ├── Migrations/
│       ├── Repositories/
│       └── Seed/
│
└── CodeBase.API/
    ├── CodeBase.API.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    │
    ├── Controllers/
    │   └── HealthController.cs
    │
    ├── Contracts/
    │   ├── ApiResponse.cs
    │   ├── ApiErrorResponse.cs
    │   └── PagedApiResponse.cs
    │
    ├── Extensions/
    │   ├── AuthenticationExtensions.cs
    │   ├── CookieExtensions.cs
    │   ├── CorsExtensions.cs
    │   ├── RateLimitExtensions.cs
    │   ├── SwaggerExtensions.cs
    │   └── WebApplicationExtensions.cs
    │
    ├── Middleware/
    │   └── ExceptionHandlingMiddleware.cs
    ├── Services/
    │   └── CurrentUser.cs
    │
    └── Properties/
        └── launchSettings.json
```

`Sample` chỉ minh họa format tổ chức service. Khi tạo dự án thật, thay nó bằng các nhóm nghiệp vụ như `Auth`, `User`, `Booking` hoặc `Facility`.

---

## 4. Trách nhiệm từng layer

### Domain

Chứa business object và business rule thuần túy:

- `Entities`: các domain object của dự án.
- `Enums`: trạng thái và loại nghiệp vụ.
- `Common`: thành phần nền dùng chung như `BaseEntity` và `IAuditableEntity`.
- Hàm bảo vệ trạng thái hợp lệ của entity.
- Không biết EF Core, HTTP, JWT, MailKit hoặc Cloudinary.

`BaseEntity` và `IAuditableEntity` có thể đặt tại đây vì chúng mô tả đặc điểm của domain object, nhưng không được chứa attribute hoặc API riêng của EF Core.

Để giữ template gọn, Domain không tạo `DomainException`. Custom exception được thống nhất trong `Application/Common/Exceptions`. Tuy nhiên, Domain cũng không được tham chiếu Application; Application Service chịu trách nhiệm kiểm tra use case và ném `AppException`. Nếu entity tự bảo vệ invariant bên trong, entity chỉ dùng exception chuẩn của .NET hoặc một cơ chế kết quả độc lập, không gọi `AppException` của Application.

### Application

Chứa use case và những gì ứng dụng cần, nhưng chưa quyết định công nghệ triển khai:

- Business service được chia theo từng nhóm nghiệp vụ.
- Mỗi service giữ chung format `IService.cs`, `Service.cs`, `Request.cs`, `Response.cs` để dễ tìm và giống cách tổ chức của dự án hkathon.
- Điều phối domain object và transaction.
- Interface cho database, JWT, mail, file storage và current user.
- Toàn bộ custom exception ở mức use case nằm trong `Common/Exceptions`.
- Pagination model độc lập với HTTP.

Ví dụ Application nói:

> Tôi cần gửi email qua `IEmailSender`.

Application không biết email được gửi bằng Gmail, MailKit, SendGrid hay một message queue.

### Infrastructure

Chứa code làm việc với công nghệ bên ngoài:

- EF Core, PostgreSQL, migrations và repository implementation.
- Sinh JWT.
- Gửi mail bằng MailKit/SMTP.
- Upload file bằng Cloudinary.
- Bind và validate options của các adapter.

Đây là nơi implement các interface do Application định nghĩa.

### API

Chứa mọi thứ thuộc HTTP:

- Controller, route, cookie và authorization policy.
- Swagger, CORS, rate limiting.
- Middleware chuyển exception thành HTTP response.
- `ApiResponse`, status code và trace ID.
- Composition root trong `Program.cs`.

Controller chỉ nhận HTTP request, gọi Application và chuyển kết quả thành HTTP response.

---

## 5. Vị trí mới của code hiện tại

| Code hiện tại | Vị trí đề xuất | Lý do |
|---|---|---|
| `Repo/Abstraction/BaseEntity.cs` | `Domain/Common/BaseEntity.cs` | Là đặc điểm chung của domain object |
| `Repo/Abstraction/IAuditableEntity.cs` | `Domain/Common/IAuditableEntity.cs` | Không nên phụ thuộc EF Core |
| `Repo/AppDbContext.cs` | `Infrastructure/Persistence/AppDbContext.cs` | EF Core là công nghệ bên ngoài |
| `Service/JwtService/IService.cs` | `Application/Abstractions/Authentication/ITokenService.cs` | Application định nghĩa nhu cầu |
| `Service/JwtService/Service.cs` | `Infrastructure/Authentication/JwtTokenService.cs` | JWT là implementation kỹ thuật |
| `Service/MailService/IService.cs` | `Application/Abstractions/Communication/IEmailSender.cs` | Interface thuộc phía sử dụng |
| `Service/MailService/Service.cs` | `Infrastructure/Communication/SmtpEmailSender.cs` | MailKit/SMTP là adapter ngoài |
| `Service/MediaService/IService.cs` | `Application/Abstractions/Media/IFileStorage.cs` | Không dùng `IFormFile` trong Application |
| `Service/CloudinaryService/Service.cs` | `Infrastructure/Media/CloudinaryFileStorage.cs` | Cloudinary là nhà cung cấp cụ thể |
| `Service/Exceptions/AppException.cs` | `Application/Common/Exceptions/AppException.cs` | Lỗi use case thuộc Application |
| `Service/Models/ApiResponse.cs` | `API/Contracts/*` | Response envelope thuộc giao thức HTTP |
| `API/Middleware/GlobalExceptionHandlerMiddleware.cs` | `API/Middleware/ExceptionHandlingMiddleware.cs` | Middleware là HTTP boundary |
| `API/Extensions/*` | `API/Extensions/*` | Đúng layer, chỉ cần chuẩn hóa tên và đăng ký |

Không nên giữ tên chung chung `IService`/`Service`. Tên nên thể hiện đúng trách nhiệm:

```text
ITokenService       → JwtTokenService
IEmailSender        → SmtpEmailSender
IFileStorage        → CloudinaryFileStorage
```

---

## 6. Exception và HTTP response

Codebase chỉ dùng một hệ custom exception tại `Application/Common/Exceptions`; Domain không có `DomainException` riêng.

Không nên cho `AppException` chứa trực tiếp HTTP status code. Application không phải tầng HTTP.

Application chỉ mô tả loại lỗi:

```text
Validation
NotFound
Conflict
Unauthorized
Forbidden
Unexpected
```

`ExceptionHandlingMiddleware` tại API sẽ mapping:

```text
Validation   → 400
Unauthorized → 401
Forbidden    → 403
NotFound     → 404
Conflict     → 409
Unexpected   → 500
```

Như vậy cùng Application Service có thể được gọi bởi Controller, background job hoặc message consumer mà không bị gắn cứng với HTTP.

Response model nên tách rõ:

```text
ApiResponse<T>       → response thành công
PagedApiResponse<T>  → response phân trang
ApiErrorResponse     → response lỗi
```

Không cần đồng thời lưu cả `IsSuccess` và `IsFailed`, vì hai giá trị luôn ngược nhau. Chỉ một trạng thái hoặc chính HTTP status code là đủ.

---

## 7. Thiết kế interface dùng chung

### Upload file

Application không nhận `IFormFile`. API chuyển `IFormFile` thành model trung lập:

```text
FileUpload
├── FileName
├── ContentType
├── Length
└── Stream
```

Sau đó gọi:

```text
IFileStorage.UploadAsync(FileUpload file, CancellationToken ct)
```

Nhờ vậy Application không phụ thuộc ASP.NET Core và có thể được gọi từ background job.

### Mail

```text
IEmailSender.SendAsync(EmailMessage message, CancellationToken ct)
```

`EmailMessage` chỉ chứa recipient, subject và body. SMTP credential thuộc Infrastructure options.

### JWT

`ITokenService` chỉ chịu trách nhiệm tạo/đọc token. Việc đọc cookie và cấu hình JwtBearer vẫn thuộc API.

Không nên bắt mọi dự án dùng sẵn role `Admin`, `Staff`, `User`. Policy cụ thể nên được khai báo khi dự án thật xác định role, ví dụ dự án booking sẽ dùng `Admin`, `Owner`, `Customer`.

---

## 8. Dependency Injection

Mỗi layer tự đăng ký dependency do chính layer đó sở hữu:

```text
Application.DependencyInjection
└── AddApplication()
    ├── validators
    └── application services/use cases

Infrastructure.DependencyInjection
└── AddInfrastructure(configuration)
    ├── DbContext
    ├── repositories
    ├── JWT implementation
    ├── mail implementation
    └── media implementation

API.Extensions
└── AddPresentation(configuration)
    ├── controllers
    ├── authentication
    ├── Swagger
    ├── CORS
    └── rate limiting
```

`Program.cs` chỉ còn vai trò ghép hệ thống:

```text
AddApplication()
AddInfrastructure(configuration)
AddPresentation(configuration)

UseExceptionHandling()
UseCors()
UseAuthentication()
UseRateLimiter()
UseAuthorization()
MapControllers()
```

---

## 9. Cấu hình appsettings

`appsettings.json` chỉ lưu cấu trúc và giá trị không nhạy cảm:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Issuer": "CodeBase",
    "Audience": "CodeBase.API",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 7,
    "SigningKey": ""
  },
  "Mail": {
    "Host": "",
    "Port": 587,
    "UseSsl": true,
    "FromAddress": "",
    "DisplayName": "",
    "UserName": "",
    "Password": ""
  },
  "Cloudinary": {
    "CloudName": "",
    "ApiKey": "",
    "ApiSecret": ""
  },
  "Cors": {
    "AllowedOrigins": []
  },
  "RateLimiting": {
    "PermitLimit": 100,
    "WindowSeconds": 60
  },
  "AllowedHosts": "*"
}
```

`appsettings.Development.json` chỉ override cấu hình phát triển không nhạy cảm, ví dụ log level và frontend localhost.

Secret phải được cấp qua User Secrets hoặc environment variables:

```text
ConnectionStrings__DefaultConnection
Jwt__SigningKey
Mail__Password
Cloudinary__ApiSecret
```

Options phải đăng ký bằng strongly typed options và kiểm tra ngay khi startup:

```text
BindConfiguration(...)
ValidateDataAnnotations()
ValidateOnStart()
```

Điều này giúp ứng dụng fail sớm với thông báo rõ ràng nếu thiếu configuration.

---

## 10. Thành phần mặc định và thành phần tùy chọn

### Nên có mặc định

- Exception handling middleware.
- Response/error contract thống nhất.
- Pagination model.
- Swagger/OpenAPI.
- CORS configuration.
- Rate limiting cơ bản.
- EF Core setup, audit timestamp và migration convention.
- Options validation.
- Dependency injection theo từng layer.
- Health check tối thiểu.
- `CancellationToken` cho mọi I/O async.

### Chỉ bật khi dự án cần

- JWT/cookie authentication.
- SMTP/MailKit.
- Cloudinary.
- Redis.
- Quartz/background jobs.
- Outbox, message broker và distributed cache.

Các thành phần tùy chọn vẫn có thể nằm trong source template, nhưng phải được đăng ký qua extension riêng. Không nên bắt ứng dụng khởi động thất bại vì thiếu Cloudinary hoặc Mail khi dự án không sử dụng chúng.

---

## 11. Cách thêm nghiệp vụ của dự án booking sau này

Template không chứa entity của dự án đặt sân. Khi tạo project thật, Domain giữ structure phẳng, dễ nhìn:

```text
Domain/
├── Common/
│   ├── BaseEntity.cs
│   ├── IAuditableEntity.cs
│   └── ISoftDeletable.cs
├── Entities/
│   ├── User.cs
│   ├── Facility.cs
│   ├── Court.cs
│   ├── Booking.cs
│   └── BookingDetail.cs
└── Enums/
    ├── UserRole.cs
    ├── BookingStatus.cs
    └── TransactionType.cs
```

Application chia theo service nghiệp vụ:

```text
Application/Services/
├── Identity/
├── Facilities/
├── Scheduling/
├── Bookings/
├── Campaigns/
├── Finance/
├── Feedbacks/
└── Reports/
```

Mỗi folder service dùng cùng một format để người đọc không phải đoán code nằm ở đâu:

```text
Services/
└── Bookings/
    ├── IService.cs
    ├── Service.cs
    ├── Request.cs
    ├── Response.cs
    └── Validators/
        ├── CreateBookingRequestValidator.cs
        └── CancelBookingRequestValidator.cs
```

Trong đó:

- `IService.cs`: contract của business service.
- `Service.cs`: điều phối nghiệp vụ, repository và transaction.
- `Request.cs`: các request model của nhóm nghiệp vụ.
- `Response.cs`: các response model trả về cho API.
- `Validators/`: mỗi request có một validator riêng để file không phình to khi nghiệp vụ tăng.

Nếu một folder lớn dần, có thể tách `Request/` và `Response/` thành hai folder con sau. Chưa cần dùng `Commands`, `Queries`, `Handlers` hoặc CQRS khi quy mô chưa yêu cầu.

---

## 12. Luồng request mẫu

```text
HTTP Request
    ↓
Controller (API)
    ↓
Use case/service interface (Application)
    ↓
Domain object bảo vệ business rule (Domain)
    ↓
Repository/DbContext implementation (Infrastructure)
    ↓
Application response
    ↓
ApiResponse (API)
```

Nếu có lỗi:

```text
AppException từ Application Service
    ↓
ExceptionHandlingMiddleware
    ↓
ApiErrorResponse + HTTP status code + traceId
```

---

## 13. Kết luận

Structure bốn project là điểm cân bằng phù hợp:

- Tách đúng business rule, use case, công nghệ và HTTP.
- Giữ được JWT, Mail, Cloudinary, middleware và extension hiện tại.
- Không cần thêm SharedKernel, BuildingBlocks, CQRS hoặc nhiều project nhỏ ngay từ đầu.
- Dễ dùng làm `dotnet new` template.
- Có thể mở rộng thành dự án booking hiện tại mà không phải đổi lại chiều phụ thuộc.

Thứ tự refactor hợp lý:

1. Tạo `Domain`, `Application`, `Infrastructure` và cập nhật project references.
2. Di chuyển base entity và audit contract vào Domain.
3. Tách interface sang Application và implementation sang Infrastructure.
4. Chuyển response model về API.
5. Chuẩn hóa exception mapping.
6. Chuẩn hóa options, loại secret khỏi source và validate khi startup.
7. Rút gọn `Program.cs` bằng các `DependencyInjection` extension.
8. Xóa WeatherForecast, `Class1.cs` và package không còn sử dụng.
