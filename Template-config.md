```json
{
  "$schema": "http://json.schemastore.org/template",
  "author": "Hung",
  "classifications": [
    "Web",
    "ASP.NET Core"
  ],
  "identity": "Hung.CleanArchitecture.Template",
  "name": "Hung Clean Architecture API",
  "shortName": "basecode",
  "preferNameDirectory": true,
  "sources": [
    {
      "exclude": [
        ".template.config/**",
        "**/bin/**",
        "**/obj/**",
        "**/.git/**",
        "**/.idea/**",
        "**/*.user",
        "**/*.lock.json",
        "Template-config.md",
        "docs/**",
        ".template-tests/**"
      ]
    }
  ],
  "symbols": {
    "safeName": {
      "type": "generated",
      "generator": "regex",
      "dataType": "string",
      "parameters": {
        "source": "name",
        "steps": [
          {
            "regex": "[^a-zA-Z0-9_.]",
            "replacement": "_"
          }
        ]
      },
      "replaces": "CodeBase",
      "fileRename": "CodeBase"
    }
  }
}
```

## Cài template local

```bash
dotnet new install .
```

Nếu đã cài phiên bản cũ:

```bash
dotnet new uninstall .
dotnet new install .
```

## Tạo project mới

```bash
dotnet new basecode -n {Tên project}

# Ví dụ:
dotnet new basecode -n mini-project-import-excel
dotnet new basecode -n MyProjectTest
```

Tên có dấu `-` sẽ được chuyển thành `_` trong namespace để C# compile được.

Sau khi tạo project:

```bash
dotnet build
dotnet run --project {Tên project}.API
```

Mặc định Database, JWT, Mail và Cloudinary đang tắt để project chạy ngay. Bật integration cần dùng trong `appsettings.json`, sau đó cung cấp secret bằng User Secrets hoặc environment variables.

## Giải thích:

- **safeName**: Sử dụng regex để convert tất cả ký tự không hợp lệ (như dấu `-`) thành `_` để tránh lỗi namespace trong C#
- **replaces**: Thay thế "CodeBase" trong tất cả file (namespace, class, project references)
- **fileRename**: Đổi tên file và folder từ "CodeBase" sang tên project mới
- **sources.exclude**: Loại trừ tài liệu nội bộ, file IDE, build output và file kiểm thử template
