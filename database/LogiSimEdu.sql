-- Tạo database mới
CREATE DATABASE [LogisimEdu];
GO

USE [LogisimEdu];
GO

CREATE TABLE [dbo].[GenderType] (
    [Id] INT PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL
);

CREATE TABLE [dbo].[EnrollmentStatus] (
    [Id] INT PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL
);

CREATE TABLE [dbo].[LessonStatus] (
    [Id] INT PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL
);

CREATE TABLE [dbo].[OrganizationRoleStatus] (
    [Id] INT NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL
);

CREATE TABLE Organization (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OrganizationName NVARCHAR(255),
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Address NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME
);

-- Bảng Account
CREATE TABLE [Account] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrganizationId UNIQUEIDENTIFIER NULL,
	SystemMode BIT DEFAULT 0,
    OrganizationRoleId INT NOT NULL,
    UserName NVARCHAR(100) NOT NULL,
    [Password] NVARCHAR(255) NOT NULL,
	IsEmailVerify BIT DEFAULT 0,
    FullName NVARCHAR(100),
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
	GenderId INT,
    Address NVARCHAR(255),
    AvtURL NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
	FOREIGN KEY (OrganizationRoleId) REFERENCES [OrganizationRoleStatus](Id),
	FOREIGN KEY (GenderId) REFERENCES [GenderType]([Id]),
    FOREIGN KEY (OrganizationId) REFERENCES Organization(Id)
);

-- Bảng WorkSpace
CREATE TABLE [WorkSpace] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrderId UNIQUEIDENTIFIER NULL,
    OrganizationId UNIQUEIDENTIFIER NULL,
    WorkSpaceName NVARCHAR(100) NOT NULL,
    NumberOfAccount INT DEFAULT 0,
    ImgURL NVARCHAR(255),
    [Description] NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME
    -- Nếu có bảng Organization thì bổ sung FK ở đây
);

-- Bảng AccountOfWorkSpace
CREATE TABLE [AccountOfWorkSpace] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AccountId UNIQUEIDENTIFIER NOT NULL,
    WorkSpaceId UNIQUEIDENTIFIER NOT NULL,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    CONSTRAINT FK_AOWS_Account FOREIGN KEY (AccountId) REFERENCES [Account](Id),
    CONSTRAINT FK_AOWS_WorkSpace FOREIGN KEY (WorkSpaceId) REFERENCES [WorkSpace](Id)
);

-- Bảng Category
CREATE TABLE [Category] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CategoryName NVARCHAR(100),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME
);

-- Bảng Course
CREATE TABLE [Course] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CategoryId UNIQUEIDENTIFIER NOT NULL,
    WorkSpaceId UNIQUEIDENTIFIER NOT NULL,
    CourseName NVARCHAR(100),
    [Description] NVARCHAR(255),
    RatingAverage FLOAT,
    ImgURL NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    CONSTRAINT FK_Course_Category FOREIGN KEY (CategoryId) REFERENCES [Category](Id),
    CONSTRAINT FK_Course_WorkSpace FOREIGN KEY (WorkSpaceId) REFERENCES [WorkSpace](Id)
);

-- Bảng Scene
CREATE TABLE [Scene] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SceneName NVARCHAR(100),
    [Description] NVARCHAR(255),
    ImgURL NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME
);

-- Bảng Topic
CREATE TABLE [Topic] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SceneId UNIQUEIDENTIFIER NOT NULL,
    CourseId UNIQUEIDENTIFIER NOT NULL,
    TopicName NVARCHAR(100),
    ImgURL NVARCHAR(255),
    [Description] NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    CONSTRAINT FK_Topic_Scene FOREIGN KEY (SceneId) REFERENCES [Scene](Id),
    CONSTRAINT FK_Topic_Course FOREIGN KEY (CourseId) REFERENCES [Course](Id)
);

-- Bảng SceneOfWorkSpace
CREATE TABLE [SceneOfWorkSpace] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SceneId UNIQUEIDENTIFIER NOT NULL,
    WorkSpaceId UNIQUEIDENTIFIER NOT NULL,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    CONSTRAINT FK_SOWS_Scene FOREIGN KEY (SceneId) REFERENCES [Scene](Id),
    CONSTRAINT FK_SOWS_WorkSpace FOREIGN KEY (WorkSpaceId) REFERENCES [WorkSpace](Id)
);

-- Bảng Scenario
CREATE TABLE [Scenario] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SceneId UNIQUEIDENTIFIER NOT NULL,
    ScenarioName NVARCHAR(100),
    [Description] NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    CONSTRAINT FK_Scenario_Scene FOREIGN KEY (SceneId) REFERENCES [Scene](Id)
);

CREATE TABLE [dbo].[Lesson] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TopicId] UNIQUEIDENTIFIER NOT NULL,
    [LessonName] NVARCHAR(100),
    [Title] NVARCHAR(255),
    [Description] NVARCHAR(255),
    [StatusId] INT,
    [IsActive] BIT DEFAULT 1,
    [Created_At] DATETIME2(3) DEFAULT SYSUTCDATETIME(),
    [Updated_At] DATETIME2(3),
    [Delete_At] DATETIME2(3),
    FOREIGN KEY ([TopicId]) REFERENCES [Topic]([Id]),
    FOREIGN KEY ([StatusId]) REFERENCES [LessonStatus]([Id])
);

-- Bảng Quiz
CREATE TABLE [Quiz] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    LessonId UNIQUEIDENTIFIER NOT NULL,
    QuizName NVARCHAR(100),
    Score FLOAT,
    Status NVARCHAR(50),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (LessonId) REFERENCES [Lesson](Id)
);

-- Bảng Question
CREATE TABLE [Question] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    QuizId UNIQUEIDENTIFIER NOT NULL,
    [Description] NVARCHAR(255),
    IsCorrect BIT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    CONSTRAINT FK_Question_Quiz FOREIGN KEY (QuizId) REFERENCES [Quiz](Id)
);

-- Bảng Answer
CREATE TABLE [Answer] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    QuestionId UNIQUEIDENTIFIER NOT NULL,
    [Description] NVARCHAR(255),
    IsAnswerCorrect BIT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    CONSTRAINT FK_Answer_Question FOREIGN KEY (QuestionId) REFERENCES [Question](Id)
);

-- Bảng Class
CREATE TABLE [Class] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CourseId UNIQUEIDENTIFIER NOT NULL,
    ClassName NVARCHAR(100),
    NumberOfStudent INT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    CONSTRAINT FK_Class_Course FOREIGN KEY (CourseId) REFERENCES [Course](Id)
);

-- Bảng AccountOfClass
CREATE TABLE [AccountOfClass] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AccountId UNIQUEIDENTIFIER NOT NULL,
    ClassId UNIQUEIDENTIFIER NOT NULL,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    CONSTRAINT FK_AOC_Account FOREIGN KEY (AccountId) REFERENCES [Account](Id),
    CONSTRAINT FK_AOC_Class FOREIGN KEY (ClassId) REFERENCES [Class](Id)
);

-- Bảng Review
CREATE TABLE [Review] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AccountId UNIQUEIDENTIFIER NOT NULL,
    CourseId UNIQUEIDENTIFIER NOT NULL,
    [Description] NVARCHAR(255),
    Rating INT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    CONSTRAINT FK_Review_Account FOREIGN KEY (AccountId) REFERENCES [Account](Id),
    CONSTRAINT FK_Review_Course FOREIGN KEY (CourseId) REFERENCES [Course](Id)
);

-- Bảng Notification
CREATE TABLE [Notification] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AccountId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(255),
    [Description] NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    CONSTRAINT FK_Notification_Account FOREIGN KEY (AccountId) REFERENCES [Account](Id)
);

CREATE TABLE Conversation (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    MessageId UNIQUEIDENTIFIER NULL,
    IsGroup BIT NOT NULL,
    Title NVARCHAR(255),
    IsActive BIT DEFAULT 1,
    Created_At DATETIME NOT NULL,
    Updated_At DATETIME,
    Delete_At DATETIME
);

CREATE TABLE Message (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ConversationId UNIQUEIDENTIFIER NOT NULL,
    SenderId UNIQUEIDENTIFIER NOT NULL,
    MessageType NVARCHAR(50),
    Content NVARCHAR(MAX),
    AttachmentURL NVARCHAR(500),
    IsEdited BIT DEFAULT 0,
    IsDeleted BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    Created_At DATETIME NOT NULL,
    Updated_At DATETIME,
    Delete_At DATETIME,

    FOREIGN KEY (ConversationId) REFERENCES Conversation(Id),
    FOREIGN KEY (SenderId) REFERENCES Account(Id)  
);

CREATE TABLE ConversationParticipant (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ConversationId UNIQUEIDENTIFIER NOT NULL,
    AccountId UNIQUEIDENTIFIER NOT NULL,
    JoinedAt DATETIME NOT NULL,
    LastReadAt DATETIME,
    IsActive BIT DEFAULT 1,
    Created_At DATETIME NOT NULL,
    Updated_At DATETIME,
    Delete_At DATETIME,

    FOREIGN KEY (ConversationId) REFERENCES Conversation(Id),
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE TABLE EnrollmentRequest (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    StudentId UNIQUEIDENTIFIER NOT NULL,
    CourseId UNIQUEIDENTIFIER NOT NULL,
    StatusId INT NOT NULL, 
    RequestedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RespondedAt DATETIME2 NULL,
	FOREIGN KEY ([StatusId]) REFERENCES [EnrollmentStatus]([Id]),

    CONSTRAINT FK_EnrollmentRequest_Student FOREIGN KEY (StudentId)
        REFERENCES Account(Id) ON DELETE CASCADE,

    CONSTRAINT FK_EnrollmentRequest_Course FOREIGN KEY (CourseId)
        REFERENCES Course(Id) ON DELETE CASCADE
);
 ---------------------------------------------------------------

CREATE TABLE [Order] (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES Organization(Id),
    WorkSpaceId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES WorkSpace(Id),
    Description NVARCHAR(MAX),
    TotalPrice FLOAT,
    BookingTime DATETIME,
    Status NVARCHAR(50),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME
);


CREATE TABLE PackageType (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    PackageName NVARCHAR(100),
    Description NVARCHAR(MAX),
    Price FLOAT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME
);

CREATE TABLE Package (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OrderId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Order](Id),
    WorkSpaceId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES WorkSpace(Id),
    PackageTypeId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES PackageType(Id),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME
);

CREATE TABLE PackageOfScene (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    PackageId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES Package(Id),
    SceneId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES Scene(Id),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME
);

CREATE TABLE QuizSubmission (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    QuizId UNIQUEIDENTIFIER NOT NULL,
    AccountId UNIQUEIDENTIFIER NOT NULL,
    SubmittedAt DATETIME,
    ScoreObtained INT,
    FOREIGN KEY (QuizId) REFERENCES Quiz(Id),
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE TABLE QuizSubmissionAnswer (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    QuizSubmissionId UNIQUEIDENTIFIER NOT NULL,
    QuestionId UNIQUEIDENTIFIER NOT NULL,
    AnswerId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (QuizSubmissionId) REFERENCES QuizSubmission(Id),
    FOREIGN KEY (QuestionId) REFERENCES Question(Id),
    FOREIGN KEY (AnswerId) REFERENCES Answer(Id)
);

INSERT INTO GenderType VALUES (1, 'Male'), (2, 'Female'), (3, 'Other');

INSERT INTO OrganizationRoleStatus VALUES (1, 'Organization_Admin'), (2, 'Instructor'), (3, 'Student'), (4, 'Admin');

INSERT INTO LessonStatus VALUES (1, 'NotStarted'), (2, 'InProgress'), (3, 'Completed');

INSERT INTO EnrollmentStatus VALUES (1, 'Pending'), (2, 'Accepted'), (3, 'Rejected');
