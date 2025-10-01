USE OSHManagement;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Drop the old column
    ALTER TABLE Employees DROP COLUMN LegacyPassword;

    -- 2. Add the new column with correct type
    ALTER TABLE Employees ADD LegacyPassword VARBINARY(100) NULL;

    COMMIT TRANSACTION;
    PRINT 'LegacyPassword column recreated as VARBINARY(100).';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error occurred: ' + ERROR_MESSAGE();
END CATCH;
GO