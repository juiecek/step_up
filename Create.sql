create table Users(
Id INT PRIMARY KEY IDENTITY(1,1),
FullName NVARCHAR(100) NOT NULL,
Email NVARCHAR(100) UNIQUE NOT NULL,
PasswordHash NVARCHAR(255) NOT NULL,
Photo NVARCHAR(255),
Role NVARCHAR(50) NOT NULL CHECK (Role IN ('Admin','Client')),
CreatedAt DATETIME DEFAULT GETDATE()
);


CREATE TABLE Halls (
    Id INT PRIMARY KEY IDENTITY(1,1), 
    Name NVARCHAR(100) NOT NULL, 
    Capacity INT NOT NULL, 
    Description NVARCHAR(MAX), 
    Photo NVARCHAR(255), 
    Price DECIMAL(10, 2) NOT NULL 
);

CREATE TABLE Bookings (
    Id INT PRIMARY KEY IDENTITY(1,1), 
    UserId INT NOT NULL, 
    HallId INT NOT NULL, 
    BookingDate DATETIME NOT NULL, 
    Duration INT NOT NULL, 
    TotalPrice DECIMAL(10, 2) NOT NULL, 
    Status NVARCHAR(50) DEFAULT 'Pending', 
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (HallId) REFERENCES Halls(Id) 
);

CREATE TABLE Instructors (
    Id INT PRIMARY KEY IDENTITY(1,1), 
    FullName NVARCHAR(100) NOT NULL, 
    Specialization NVARCHAR(255) NOT NULL, 
    Description NVARCHAR(MAX) NOT NULL,
    Photo NVARCHAR(255) NOT NULL, 
    Phone NVARCHAR(15)
);

CREATE TABLE Schedule (
    Id INT PRIMARY KEY IDENTITY(1,1), 
    InstructorId INT NOT NULL, 
    HallId INT NOT NULL,
    Date DATETIME NOT NULL, 
    Duration INT NOT NULL, 
    DanceStyle NVARCHAR(100) NOT NULL, 
    MaxParticipants INT NOT NULL,
    FOREIGN KEY (InstructorId) REFERENCES Instructors(Id),
    FOREIGN KEY (HallId) REFERENCES Halls(Id) 
);

CREATE TABLE Registrations (
    Id INT PRIMARY KEY IDENTITY(1,1), 
    UserId INT NOT NULL, 
    ScheduleId INT NOT NULL, 
    RegistrationDate DATETIME DEFAULT GETDATE(), 
    FOREIGN KEY (UserId) REFERENCES Users(Id), 
    FOREIGN KEY (ScheduleId) REFERENCES Schedule(Id) 
);

SET IDENTITY_INSERT [dbo].[Instructors] ON
INSERT INTO [dbo].[Instructors] ([Id], [Specialization], [Photo], [Description], [FullName], [Phone]) VALUES (1, N'Контемпарари', N'/images/teachers/teacher1.jpg
', N'Наш преподаватель Hip Hop, которая влюблена в уличную культуру и с удовольствием делится своей энергией с учениками. С ней каждый урок — это настоящее шоу, наполненное драйвом и ритмом. Алена помогает студентам раскрыть свой стиль и научиться чувствовать музыку, объединяя их с культурой танца, где нет границ для самовыражения.', N'Алена', N'375295289656')
INSERT INTO [dbo].[Instructors] ([Id], [Specialization], [Photo], [Description], [FullName], [Phone]) VALUES (7, N'Контемпарари', N'/images/teachers/teacher1.jpg
', N'Наш преподаватель Hip Hop, которая влюблена в уличную культуру и с удовольствием делится своей энергией с учениками. С ней каждый урок — это настоящее шоу, наполненное драйвом и ритмом. Алена помогает студентам раскрыть свой стиль и научиться чувствовать музыку, объединяя их с культурой танца, где нет границ для самовыражения.', N'Алена', N'375295289656')
INSERT INTO [dbo].[Instructors] ([Id], [Specialization], [Photo], [Description], [FullName], [Phone]) VALUES (8, N'Контемпарари', N'/images/teachers/teacher1.jpg
', N'Наш преподаватель Hip Hop, которая влюблена в уличную культуру и с удовольствием делится своей энергией с учениками. С ней каждый урок — это настоящее шоу, наполненное драйвом и ритмом. Алена помогает студентам раскрыть свой стиль и научиться чувствовать музыку, объединяя их с культурой танца, где нет границ для самовыражения.', N'Алена', N'375295289656')
SET IDENTITY_INSERT [dbo].[Instructors] OFF
