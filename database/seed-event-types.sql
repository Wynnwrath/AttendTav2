/*
   Seeds the MVP's six starter event classifications.
   Run only in the disposable AttendTaMVP_Test database, after the local admin
   seed has created admin@gmail.com as an active admin.
   Auth must create a local test admin before this seed is run. Obtain any
   test credentials securely from the Auth owner; this file never stores them.
   Never use local test credentials outside a disposable local database.
   Uses that active admin's user_id for the required created_by field.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'AttendTaMVP_Test'
    THROW 51020, 'This seed script may only run in AttendTaMVP_Test.', 1;
GO

DECLARE @CreatedByUserId int;

SELECT @CreatedByUserId = [user_id]
FROM dbo.users
WHERE [email] = N'admin@gmail.com'
  AND [role] = N'admin'
  AND [is_active] = 1;

IF @CreatedByUserId IS NULL
    THROW 51021, 'Run the local admin seed before seeding event types.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    INSERT INTO dbo.event_types ([type_name], [created_by])
    SELECT seed.[type_name], @CreatedByUserId
    FROM (VALUES
        (N'Seminar'),
        (N'Outreach'),
        (N'Meeting'),
        (N'Competition'),
        (N'General Assembly'),
        (N'Training')
    ) AS seed([type_name])
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.event_types AS existing
        WHERE existing.[type_name] = seed.[type_name]
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

SELECT [type_id], [type_name], [is_active], [created_by]
FROM dbo.event_types
ORDER BY [type_name];
GO
