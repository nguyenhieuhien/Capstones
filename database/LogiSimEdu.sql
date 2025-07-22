-- Tạo database mới
CREATE DATABASE [LogiSimEdu];
GO

USE [LogiSimEdu];
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

CREATE TABLE QuestionType (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50)
);

CREATE TABLE QuizStatus (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50)
);

CREATE TABLE CourseProgressStatus (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50)
);

CREATE TABLE LessonProgressStatus (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50)
);

CREATE TABLE PaymentMethod (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50)
);

CREATE TABLE PaymentStatus (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50)
);

CREATE TABLE OrderStatus (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50)
);

CREATE TABLE Organization (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OrganizationName NVARCHAR(255),
    ImgUrl NVARCHAR(255),
	Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Address NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME
);

CREATE TABLE Role (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50)
);

CREATE TABLE Account (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    RoleId INT,
    OrganizationId UNIQUEIDENTIFIER,
    UserName NVARCHAR(100),
    FullName NVARCHAR(100),
    Email NVARCHAR(100),
    Password NVARCHAR(255),
    Phone NVARCHAR(20),
    Gender INT,
    Address NVARCHAR(255),
    AvtURL NVARCHAR(255),
    IsEmailVerify BIT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (RoleId) REFERENCES Role(Id),
    FOREIGN KEY (OrganizationId) REFERENCES Organization(Id),
    FOREIGN KEY (Gender) REFERENCES GenderType(Id)
);

CREATE TABLE WorkSpace (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER,
    WorkSpaceName NVARCHAR(100),
    NumberOfAccount INT,
    ImgURL NVARCHAR(255),
    Description NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (OrganizationId) REFERENCES Organization(Id)
);

CREATE TABLE AccountOfWorkSpace (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AccountId UNIQUEIDENTIFIER,
    WorkSpaceId UNIQUEIDENTIFIER,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (AccountId) REFERENCES Account(Id),
    FOREIGN KEY (WorkSpaceId) REFERENCES WorkSpace(Id)
);

CREATE TABLE Category (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CategoryName NVARCHAR(100),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME
);

CREATE TABLE Course (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    InstructorId UNIQUEIDENTIFIER,
    CategoryId UNIQUEIDENTIFIER,
    WorkSpaceId UNIQUEIDENTIFIER,
    CourseName NVARCHAR(100),
    Description NVARCHAR(255),
    RatingAverage FLOAT,
    ImgURL NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (InstructorId) REFERENCES Account(Id),
    FOREIGN KEY (CategoryId) REFERENCES Category(Id),
    FOREIGN KEY (WorkSpaceId) REFERENCES WorkSpace(Id)
);

CREATE TABLE Class (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    InstructorId UNIQUEIDENTIFIER,
    CourseId UNIQUEIDENTIFIER,
    ClassName NVARCHAR(100),
    Description NVARCHAR(255),
    NumberOfStudent INT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (InstructorId) REFERENCES Account(Id),
    FOREIGN KEY (CourseId) REFERENCES Course(Id)
);

CREATE TABLE AccountOfCourse (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AccountId UNIQUEIDENTIFIER,
    CourseId UNIQUEIDENTIFIER,
    ClassId UNIQUEIDENTIFIER,
    Status INT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (AccountId) REFERENCES Account(Id),
    FOREIGN KEY (CourseId) REFERENCES Course(Id),
    FOREIGN KEY (ClassId) REFERENCES Class(Id),
    FOREIGN KEY (Status) REFERENCES EnrollmentStatus(Id)
);

CREATE TABLE Scene (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SceneName NVARCHAR(100),
    Description NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME
);

CREATE TABLE SceneOfWorkSpace (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SceneId UNIQUEIDENTIFIER,
    WorkSpaceId UNIQUEIDENTIFIER,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (SceneId) REFERENCES Scene(Id),
    FOREIGN KEY (WorkSpaceId) REFERENCES WorkSpace(Id)
);

CREATE TABLE Topic (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CourseId UNIQUEIDENTIFIER,
    TopicName NVARCHAR(100),
    ImgURL NVARCHAR(255),
    Description NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (CourseId) REFERENCES Course(Id)
);

CREATE TABLE Scenario (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SceneId UNIQUEIDENTIFIER,
    TopicId UNIQUEIDENTIFIER,
    FileURL NVARCHAR(255),
    ScenarioName NVARCHAR(100),
    Description NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (SceneId) REFERENCES Scene(Id),
    FOREIGN KEY (TopicId) REFERENCES Topic(Id)
);

CREATE TABLE Lesson (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TopicId UNIQUEIDENTIFIER,
    LessonName NVARCHAR(100),
    Title NVARCHAR(255),
    Description NVARCHAR(255),
    Status INT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (TopicId) REFERENCES Topic(Id),
    FOREIGN KEY (Status) REFERENCES LessonStatus(Id)
);

CREATE TABLE Quiz (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    LessonId UNIQUEIDENTIFIER,
    QuizName NVARCHAR(100),
    TotalScore FLOAT,
    Status INT,
    IsFinalQuiz BIT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (LessonId) REFERENCES Lesson(Id),
    FOREIGN KEY (Status) REFERENCES QuizStatus(Id)
);

CREATE TABLE Question (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    QuizId UNIQUEIDENTIFIER,
    QuestionType INT,
    Description NVARCHAR(255),
    Score FLOAT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (QuizId) REFERENCES Quiz(Id),
    FOREIGN KEY (QuestionType) REFERENCES QuestionType(Id)
);

CREATE TABLE Answer (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    QuestionId UNIQUEIDENTIFIER,
    Description NVARCHAR(255),
    IsCorrect BIT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (QuestionId) REFERENCES Question(Id)
);

CREATE TABLE QuizSubmission (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AccountId UNIQUEIDENTIFIER,
    QuizId UNIQUEIDENTIFIER,
    SubmitTime DATETIME,
    TotalScore FLOAT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (AccountId) REFERENCES Account(Id),
    FOREIGN KEY (QuizId) REFERENCES Quiz(Id)
);

CREATE TABLE QuestionSubmission (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    QuizSubmissionId UNIQUEIDENTIFIER,
    QuestionId UNIQUEIDENTIFIER,
    SelectedAnswerId UNIQUEIDENTIFIER,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (QuizSubmissionId) REFERENCES QuizSubmission(Id),
    FOREIGN KEY (QuestionId) REFERENCES Question(Id),
    FOREIGN KEY (SelectedAnswerId) REFERENCES Answer(Id)
);

CREATE TABLE LessonProgress (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AccountId UNIQUEIDENTIFIER,
    LessonId UNIQUEIDENTIFIER,
    Status INT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (AccountId) REFERENCES Account(Id),
    FOREIGN KEY (LessonId) REFERENCES Lesson(Id),
    FOREIGN KEY (Status) REFERENCES LessonProgressStatus(Id)
);

CREATE TABLE CourseProgress (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AccountId UNIQUEIDENTIFIER,
    CourseId UNIQUEIDENTIFIER,
    ProgressPercent FLOAT,
    Status INT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (AccountId) REFERENCES Account(Id),
    FOREIGN KEY (CourseId) REFERENCES Course(Id),
    FOREIGN KEY (Status) REFERENCES CourseProgressStatus(Id)
);

CREATE TABLE Notification (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AccountId UNIQUEIDENTIFIER,
    Title NVARCHAR(255),
    Description NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE TABLE Review (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AccountId UNIQUEIDENTIFIER,
    CourseId UNIQUEIDENTIFIER,
    Description NVARCHAR(255),
    Rating INT,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (AccountId) REFERENCES Account(Id),
    FOREIGN KEY (CourseId) REFERENCES Course(Id)
);

CREATE TABLE CertificateTemplete (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER,
    CourseId UNIQUEIDENTIFIER,
    TemplateName NVARCHAR(100),
    BackgroundURL NVARCHAR(255),
    HtmlTemplate NVARCHAR(MAX),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (OrganizationId) REFERENCES Organization(Id),
    FOREIGN KEY (CourseId) REFERENCES Course(Id)
);

CREATE TABLE Certificate (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AccountId UNIQUEIDENTIFIER,
    CourseId UNIQUEIDENTIFIER,
    CertiTempId UNIQUEIDENTIFIER,
    CertificateName NVARCHAR(255),
    Score FLOAT,
    Rank UNIQUEIDENTIFIER,
    FileURL NVARCHAR(255),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (AccountId) REFERENCES Account(Id),
    FOREIGN KEY (CourseId) REFERENCES Course(Id),
    FOREIGN KEY (CertiTempId) REFERENCES CertificateTemplete(Id)
);

CREATE TABLE SubscriptionPlan (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(100),
    Price FLOAT,
    DurationInMonths INT,
    MaxWorkSpaces INT,
    Description NVARCHAR(MAX),
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME
);

CREATE TABLE [Order] (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER,
    AccountId UNIQUEIDENTIFIER,
    SubcriptionPlanId UNIQUEIDENTIFIER,
    Description NVARCHAR(MAX),
    TotalPrice FLOAT,
    OrderTime DATETIME,
    Status INT,
    StartDate DATETIME,
    EndDate DATETIME,
    IsActive BIT,
    Created_At DATETIME,
    Updated_At DATETIME,
    Delete_At DATETIME,
    FOREIGN KEY (OrganizationId) REFERENCES Organization(Id),
    FOREIGN KEY (AccountId) REFERENCES Account(Id),
    FOREIGN KEY (SubcriptionPlanId) REFERENCES SubscriptionPlan(Id),
	FOREIGN KEY (Status) REFERENCES OrderStatus(Id)
);

CREATE TABLE Payment (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OrderId UNIQUEIDENTIFIER,
    TransactionCode NVARCHAR(100),
    Method INT,
    Status INT,
    PaidAmount FLOAT,
    PaidAt DATETIME,
    FOREIGN KEY (OrderId) REFERENCES [Order](Id),
    FOREIGN KEY (Method) REFERENCES PaymentMethod(Id),
    FOREIGN KEY (Status) REFERENCES PaymentStatus(Id)
);

CREATE TABLE ChatHistories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId NVARCHAR(100),
    Role NVARCHAR(10),
    Message NVARCHAR(MAX),
    Timestamp DATETIME
);

INSERT INTO GenderType VALUES (1, 'Male'), (2, 'Female'), (3, 'Other');

INSERT INTO Role VALUES (1, 'Admin'), (2, 'Organization_Admin'), (3, 'Instructor'), (4, 'Student');

INSERT INTO LessonStatus VALUES (1, 'NotStarted'), (2, 'InProgress'), (3, 'Completed');

INSERT INTO EnrollmentStatus VALUES (1, 'Pending'), (2, 'Accepted'), (3, 'Rejected');
