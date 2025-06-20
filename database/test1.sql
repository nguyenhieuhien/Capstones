create database test1
use test1
CREATE TABLE Organization (
    Organization_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);

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

CREATE TABLE Scene (
    Scene_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    CreatedBy UNIQUEIDENTIFIER,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE WorkspaceScene (
    Workspace_Id UNIQUEIDENTIFIER,
    Scene_Id UNIQUEIDENTIFIER,
    PRIMARY KEY (Workspace_Id, Scene_Id),
    FOREIGN KEY (Workspace_Id) REFERENCES Workspace(Workspace_Id),
    FOREIGN KEY (Scene_Id) REFERENCES Scene(Scene_Id)
);


CREATE TABLE Scenario (
    Scenario_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Scene_Id UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100),
    Script NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Scene_Id) REFERENCES Scene(Scene_Id)
);

CREATE TABLE Course (
    Course_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Workspace_Id UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100),
    Description NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Workspace_Id) REFERENCES Workspace(Workspace_Id)
);

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

CREATE TABLE LearningGroup (
    Group_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Course_Id UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Course_Id) REFERENCES Course(Course_Id)
);

CREATE TABLE [User] (
    User_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Organization_Id UNIQUEIDENTIFIER,
    Role NVARCHAR(50), -- admin, staff, instructor, student
    FullName NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE,
    PasswordHash NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Organization_Id) REFERENCES Organization(Organization_Id)
);

CREATE TABLE GroupMember (
    Group_Id UNIQUEIDENTIFIER,
    User_Id UNIQUEIDENTIFIER,
    PRIMARY KEY (Group_Id, User_Id),
    FOREIGN KEY (Group_Id) REFERENCES LearningGroup(Group_Id),
    FOREIGN KEY (User_Id) REFERENCES [User](User_Id)
);

CREATE TABLE Lab (
    Lab_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Topic_Id UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100),
    Instructions NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Topic_Id) REFERENCES Topic(Topic_Id)
);

CREATE TABLE Quiz (
    Quiz_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Topic_Id UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(100),
    Description NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Topic_Id) REFERENCES Topic(Topic_Id)
);


CREATE TABLE Question (
    Question_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Quiz_Id UNIQUEIDENTIFIER NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Type NVARCHAR(50) CHECK (Type IN ('single_choice', 'multiple_choice', 'true_false')) DEFAULT 'single_choice',
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Quiz_Id) REFERENCES Quiz(Quiz_Id)
);

CREATE TABLE Answer (
    Answer_Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Question_Id UNIQUEIDENTIFIER NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    IsCorrect BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (Question_Id) REFERENCES Question(Question_Id)
);

INSERT INTO Organization (Organization_Id, Name, Email, Phone)
VALUES
(NEWID(), N'FPT University', 'fpt@fpt.edu.vn', '0909123456'),
(NEWID(), N'Logistics Training Center', 'ltc@logistics.vn', '02812345678');

DECLARE @Org1 UNIQUEIDENTIFIER = (SELECT TOP 1 Organization_Id FROM Organization WHERE Name = N'FPT University');
DECLARE @Org2 UNIQUEIDENTIFIER = (SELECT TOP 1 Organization_Id FROM Organization WHERE Name = N'Logistics Training Center');

INSERT INTO [User] (User_Id, Organization_Id, Role, FullName, Email, PasswordHash)
VALUES
(NEWID(), @Org1, 'admin',       N'Admin FPT',      'admin@fpt.edu.vn',     'hashed_admin_pw'),
(NEWID(), @Org1, 'instructor',  N'Tran Van A',      'instructor@fpt.edu.vn','hashed_instr_pw'),
(NEWID(), @Org1, 'student',     N'Nguyen Thi B',    'student1@fpt.edu.vn',  'hashed_stud_pw1'),
(NEWID(), @Org2, 'staff',       N'Le Thi C',        'staff@ltc.vn',         'hashed_staff_pw'),
(NEWID(), @Org2, 'student',     N'Pham Van D',      'student2@ltc.vn',      'hashed_stud_pw2');
