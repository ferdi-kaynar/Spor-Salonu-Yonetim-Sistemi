-- ProfilResmi kolonunu AspNetUsers tablosuna ekle
ALTER TABLE [AspNetUsers]
ADD [ProfilResmi] nvarchar(max) NULL;
GO

-- Kontrol et
SELECT TOP 1 * FROM [AspNetUsers];
GO

