# DotNetApiTemplate

此專案為基於 ASP.NET Core 的後端 API，採用 .NET 8.0 框架，專注於高效能與可擴展性。技術重點包括：

1. **身份驗證與授權**：使用 JWT 實現身份驗證，整合 Identity 提供用戶管理功能。
2. **資料庫操作**：採用 Entity Framework Core 作為 ORM，支持 SQL Server，並實現泛型 Repository 模式。
3. **日誌管理**：透過介面與資料表記錄操作日誌。
4. **自動映射**：使用 AutoMapper 實現模型映射。
5. **API 文檔生成**：集成 Swagger 提供文檔與測試介面。
6. **模組化設計**：分層架構提升代碼可維護性。
7. **環境配置**：使用 appsettings.json 管理設定，支持 Visual Studio Code 開發。