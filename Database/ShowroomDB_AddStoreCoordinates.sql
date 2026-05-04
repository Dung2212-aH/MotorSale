USE [ShowroomDB]
GO

IF COL_LENGTH('dbo.SHOWROOM', 'Latitude') IS NULL
BEGIN
    ALTER TABLE [dbo].[SHOWROOM]
    ADD [Latitude] decimal(9, 6) NULL;
END
GO

IF COL_LENGTH('dbo.SHOWROOM', 'Longitude') IS NULL
BEGIN
    ALTER TABLE [dbo].[SHOWROOM]
    ADD [Longitude] decimal(9, 6) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE [name] = N'CK_SHOWROOM_Latitude'
      AND [parent_object_id] = OBJECT_ID(N'dbo.SHOWROOM')
)
BEGIN
    ALTER TABLE [dbo].[SHOWROOM]
    ADD CONSTRAINT [CK_SHOWROOM_Latitude]
    CHECK ([Latitude] IS NULL OR ([Latitude] >= -90 AND [Latitude] <= 90));
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE [name] = N'CK_SHOWROOM_Longitude'
      AND [parent_object_id] = OBJECT_ID(N'dbo.SHOWROOM')
)
BEGIN
    ALTER TABLE [dbo].[SHOWROOM]
    ADD CONSTRAINT [CK_SHOWROOM_Longitude]
    CHECK ([Longitude] IS NULL OR ([Longitude] >= -180 AND [Longitude] <= 180));
END
GO
