create database LogiSimEdu
use LogiSimEdu

-- 1. Organization
CREATE TABLE Organization (
    Organization_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);

-- 2. Package
CREATE TABLE Package (
    Package_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    MaxMembers INT,
    MaxGroups INT,
    MaxScenes INT,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);

-- 3. Workspace
CREATE TABLE Workspace (
    Workspace_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Organization_Id UNIQUEIDENTIFIER NOT NULL,
    Package_Id UNIQUEIDENTIFIER,
    Name NVARCHAR(100),
    IsFree BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Organization_Id) REFERENCES Organization(Organization_Id),
    FOREIGN KEY (Package_Id) REFERENCES Package(Package_Id)
);

-- 4. Scene
CREATE TABLE Scene (
    Scene_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    CreatedBy UNIQUEIDENTIFIER, -- Admin ID
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);

-- 5. WorkspaceScene (N-N)
CREATE TABLE WorkspaceScene (
    Workspace_Id UNIQUEIDENTIFIER,
    Scene_Id UNIQUEIDENTIFIER,
    PRIMARY KEY (Workspace_Id, Scene_Id),
    FOREIGN KEY (Workspace_Id) REFERENCES Workspace(Workspace_Id),
    FOREIGN KEY (Scene_Id) REFERENCES Scene(Scene_Id)
);

-- 6. Scenario
CREATE TABLE Scenario (
    Scenario_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Scene_Id UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100),
    Script NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Scene_Id) REFERENCES Scene(Scene_Id)
);

-- 7. Course
CREATE TABLE Course (
    Course_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Workspace_Id UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100),
    Description NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Workspace_Id) REFERENCES Workspace(Workspace_Id)
);

-- 8. Topic
CREATE TABLE Topic (
    Topic_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Course_Id UNIQUEIDENTIFIER NOT NULL,
    Scene_Id UNIQUEIDENTIFIER,
    Name NVARCHAR(100),
    Description NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Course_Id) REFERENCES Course(Course_Id),
    FOREIGN KEY (Scene_Id) REFERENCES Scene(Scene_Id)
);

-- 9. Group
CREATE TABLE LearningGroup (
    Group_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Course_Id UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Course_Id) REFERENCES Course(Course_Id)
);

-- 10. Role
CREATE TABLE Role (
    Role_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL
);

-- 11. Account
CREATE TABLE Account (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RoleId UNIQUEIDENTIFIER NOT NULL,
    UserName NVARCHAR(100) NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100),
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    DeleteAt DATETIME,
    FOREIGN KEY (RoleId) REFERENCES Role(Role_Id)
);

-- 12. GroupMember
CREATE TABLE GroupMember (
    Group_Id UNIQUEIDENTIFIER,
    Account_Id UNIQUEIDENTIFIER,
    PRIMARY KEY (Group_Id, Account_Id),
    FOREIGN KEY (Group_Id) REFERENCES LearningGroup(Group_Id),
    FOREIGN KEY (Account_Id) REFERENCES Account(Id)
);

-- 13. Lab
CREATE TABLE Lab (
    Lab_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Topic_Id UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100),
    Instructions NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Topic_Id) REFERENCES Topic(Topic_Id)
);

-- 14. Quiz
CREATE TABLE Quiz (
    Quiz_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Topic_Id UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(100),
    Description NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Topic_Id) REFERENCES Topic(Topic_Id)
);

-- 15. Question
CREATE TABLE Question (
    Question_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Quiz_Id UNIQUEIDENTIFIER NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Type NVARCHAR(50) CHECK (Type IN ('single_choice', 'multiple_choice', 'true_false')) DEFAULT 'single_choice',
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Quiz_Id) REFERENCES Quiz(Quiz_Id)
);

-- 16. Answer
CREATE TABLE Answer (
    Answer_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Question_Id UNIQUEIDENTIFIER NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    IsCorrect BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Question_Id) REFERENCES Question(Question_Id)
);


INSERT INTO Role (Role_Id, Name)
VALUES
(NEWID(), 'admin'),
(NEWID(), 'staff'),
(NEWID(), 'instructor'),
(NEWID(), 'student');

INSERT INTO Organization (Organization_Id, Name, Email, Phone)
VALUES
(NEWID(), N'FPT University', 'contact@fpt.edu.vn', '0281234567'),
(NEWID(), N'Logistics Training Center', 'info@ltc.vn', '0289876543');


DECLARE @AdminRole UNIQUEIDENTIFIER = (SELECT TOP 1 Role_Id FROM Role WHERE Name = 'admin');
DECLARE @InstructorRole UNIQUEIDENTIFIER = (SELECT TOP 1 Role_Id FROM Role WHERE Name = 'instructor');
DECLARE @StudentRole UNIQUEIDENTIFIER = (SELECT TOP 1 Role_Id FROM Role WHERE Name = 'student');
DECLARE @StaffRole UNIQUEIDENTIFIER = (SELECT TOP 1 Role_Id FROM Role WHERE Name = 'staff');

DECLARE @Org1 UNIQUEIDENTIFIER = (SELECT TOP 1 Organization_Id FROM Organization WHERE Name = N'FPT University');
DECLARE @Org2 UNIQUEIDENTIFIER = (SELECT TOP 1 Organization_Id FROM Organization WHERE Name = N'Logistics Training Center');

INSERT INTO Account (Id, RoleId, UserName, Password, FullName, Email, Phone, IsActive, CreatedAt)
VALUES
(NEWID(), @AdminRole,      'adminfpt',      '123456', N'Admin FPT',      'admin@fpt.edu.vn',     '0909123456', 1, GETDATE()),
(NEWID(), @InstructorRole, 'phuong',     '123456', N'Tran Van A',     'instructor@fpt.edu.vn','0912123456', 1, GETDATE()),
(NEWID(), @StudentRole,    'nguyenthib',    '123456', N'Nguyen Thi B',   'student1@fpt.edu.vn',  '0933123456', 1, GETDATE()),
(NEWID(), @StaffRole,      'letc_ltc',      '123456', N'Le Thi C',       'staff@ltc.vn',         '0911223344', 1, GETDATE()),
(NEWID(), @StudentRole,    'phamvand_ltc',  '123456', N'Pham Van D',     'student2@ltc.vn',      '0988776655', 1, GETDATE());

