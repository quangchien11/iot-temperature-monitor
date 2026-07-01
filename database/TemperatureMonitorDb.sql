/* ============================================================================
   ĐỒ ÁN IoT - HỆ THỐNG GIÁM SÁT VÀ CẢNH BÁO NHIỆT ĐỘ
   Script tạo cơ sở dữ liệu SQL Server + dữ liệu mẫu
   Tương thích: SQL Server 2017 trở lên (SQL Express được)
   ----------------------------------------------------------------------------
   Lưu ý: Backend dùng EF Core EnsureCreated() sẽ tự tạo DB khi chạy lần đầu.
   Script này dùng khi muốn tạo DB thủ công bằng SSMS / sqlcmd.
   ============================================================================ */

-- 1. TẠO DATABASE -----------------------------------------------------------
IF DB_ID('TemperatureMonitorDb') IS NULL
    CREATE DATABASE TemperatureMonitorDb;
GO

USE TemperatureMonitorDb;
GO

-- 2. XÓA BẢNG CŨ (nếu chạy lại script) --------------------------------------
IF OBJECT_ID('dbo.AlertLogs', 'U')        IS NOT NULL DROP TABLE dbo.AlertLogs;
IF OBJECT_ID('dbo.TemperatureLogs', 'U')  IS NOT NULL DROP TABLE dbo.TemperatureLogs;
IF OBJECT_ID('dbo.AlertSettings', 'U')    IS NOT NULL DROP TABLE dbo.AlertSettings;
GO

-- 3. BẢNG AlertSettings - cấu hình cảnh báo (chỉ 1 dòng Id=1) ----------------
CREATE TABLE dbo.AlertSettings
(
    Id              INT             NOT NULL PRIMARY KEY,
    MaxTemperature  FLOAT           NOT NULL,            -- ngưỡng nhiệt độ tối đa (°C)
    IsActive        BIT             NOT NULL DEFAULT(1), -- bật/tắt cảnh báo
    UpdatedAt       DATETIME2       NOT NULL DEFAULT(SYSDATETIME())
);
GO

-- 4. BẢNG TemperatureLogs - lịch sử nhiệt độ --------------------------------
CREATE TABLE dbo.TemperatureLogs
(
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Temperature  FLOAT             NOT NULL,             -- nhiệt độ (°C)
    Humidity     FLOAT             NOT NULL,             -- độ ẩm (%)
    IsAlert      BIT               NOT NULL DEFAULT(0),  -- bản ghi này có vượt ngưỡng không
    CreatedAt    DATETIME2         NOT NULL DEFAULT(SYSDATETIME())
);
GO
-- Index theo thời gian để truy vấn thống kê nhanh
CREATE INDEX IX_TemperatureLogs_CreatedAt ON dbo.TemperatureLogs(CreatedAt);
GO

-- 5. BẢNG AlertLogs - nhật ký các lần cảnh báo ------------------------------
CREATE TABLE dbo.AlertLogs
(
    Id              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Temperature     FLOAT             NOT NULL,
    MaxTemperature  FLOAT             NOT NULL,
    Message         NVARCHAR(255)     NULL,
    CreatedAt       DATETIME2         NOT NULL DEFAULT(SYSDATETIME())
);
GO
CREATE INDEX IX_AlertLogs_CreatedAt ON dbo.AlertLogs(CreatedAt);
GO

/* ============================================================================
   6. SEED DỮ LIỆU MẪU
   ============================================================================ */

-- 6.1 Cấu hình cảnh báo mặc định: ngưỡng 35°C, đang bật
INSERT INTO dbo.AlertSettings (Id, MaxTemperature, IsActive, UpdatedAt)
VALUES (1, 35, 1, SYSDATETIME());
GO

-- 6.2 Sinh 48 giờ dữ liệu nhiệt độ mẫu (mỗi 10 phút 1 bản ghi)
--     Nhiệt độ dao động theo dạng sin quanh 30°C, thỉnh thoảng vượt 35°C.
DECLARE @i INT = 0;
DECLARE @total INT = 288;                 -- 48h * 6 bản ghi/giờ
DECLARE @now DATETIME2 = SYSDATETIME();
DECLARE @temp FLOAT, @humid FLOAT, @t DATETIME2, @maxTemp FLOAT = 35;

WHILE @i < @total
BEGIN
    SET @t = DATEADD(MINUTE, -10 * (@total - @i), @now);
    -- Dạng sóng nhiệt độ: nền 30 + biên độ 6 theo giờ trong ngày + nhiễu ngẫu nhiên
    SET @temp = 30
                + 6 * SIN((DATEPART(HOUR, @t) / 24.0) * 2 * PI())
                + (RAND(CHECKSUM(NEWID())) * 2 - 1);
    SET @humid = 65 - (@temp - 30) * 1.5 + (RAND(CHECKSUM(NEWID())) * 4 - 2);

    INSERT INTO dbo.TemperatureLogs (Temperature, Humidity, IsAlert, CreatedAt)
    VALUES (ROUND(@temp, 1), ROUND(@humid, 1),
            CASE WHEN @temp > @maxTemp THEN 1 ELSE 0 END, @t);

    -- Nếu vượt ngưỡng thì ghi nhật ký cảnh báo
    IF @temp > @maxTemp
        INSERT INTO dbo.AlertLogs (Temperature, MaxTemperature, Message, CreatedAt)
        VALUES (ROUND(@temp, 1), @maxTemp,
                N'Nhiệt độ ' + CAST(ROUND(@temp,1) AS NVARCHAR) + N'°C vượt ngưỡng 35°C', @t);

    SET @i = @i + 1;
END
GO

-- 7. KIỂM TRA ---------------------------------------------------------------
PRINT '=== Tổng số bản ghi nhiệt độ ===';
SELECT COUNT(*) AS TotalLogs FROM dbo.TemperatureLogs;
PRINT '=== Tổng số cảnh báo ===';
SELECT COUNT(*) AS TotalAlerts FROM dbo.AlertLogs;
PRINT '=== Cấu hình cảnh báo ===';
SELECT * FROM dbo.AlertSettings;
GO
