USE [step_up]
GO

/****** Объект: Table [dbo].[Instructors] Дата скрипта: 24.02.2025 23:55:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Instructors] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [Specialization] NVARCHAR (MAX) NOT NULL,
    [Photo]          NVARCHAR (MAX) NOT NULL,
    [Description]    NVARCHAR (MAX) NOT NULL,
    [FullName]       NVARCHAR (MAX) NOT NULL,
    [Phone]          NVARCHAR (MAX) NULL
);


INSERT INTO [dbo].[Instructors] ([Specialization], [Photo], [Description], [FullName], [Phone])  
VALUES  
    ('Hip Hop', 'hiphop1.jpg', 'Опытный преподаватель хип-хопа с 5-летним стажем.', 'Иван Петров', '+79001234567'),  
    ('Jazz Funk', 'jazzfunk1.jpg', 'Энергичная танцовщица, обучает джаз-фанку более 3 лет.', 'Мария Иванова', '+79007654321'),  
    ('Girly Choreo', 'girly1.jpg', 'Грациозность и стиль в каждом движении. Обучает более 4 лет.', 'Анна Смирнова', NULL),  
    ('Contemporary', 'contemporary1.jpg', 'Современные техники и уникальная подача.', 'Екатерина Орлова', '+79009876543'),  
    ('Strip', 'strip1.jpg', 'Профессиональный подход к пластике и женственности.', 'Ольга Сидорова', NULL),  
    ('Hip Hop', 'hiphop2.jpg', 'Преподаватель хип-хопа, участвовал в международных баттлах.', 'Артем Васильев', '+79001112233'),  
    ('Jazz Funk', 'jazzfunk2.jpg', 'Эксперт в женственных и динамичных движениях.', 'Дарья Кузнецова', NULL),  
    ('Contemporary', 'contemporary2.jpg', 'Любит соединять классику с современной хореографией.', 'Сергей Михайлов', '+79005556677');  
