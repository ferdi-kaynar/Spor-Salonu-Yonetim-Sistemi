-- AspNetUsers tablosuna ProfilResmi kolonunu ekle
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') 
    AND name = 'ProfilResmi'
)
BEGIN
    ALTER TABLE [AspNetUsers]
    ADD [ProfilResmi] nvarchar(max) NULL;
    
    PRINT 'ProfilResmi kolonu başarıyla eklendi.';
END
ELSE
BEGIN
    PRINT 'ProfilResmi kolonu zaten mevcut.';
END
GO

