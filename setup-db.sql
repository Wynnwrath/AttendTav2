-- =====================================================================
-- AttendTa LocalDB Database Setup Script
-- Database: AsiBasecodeDB on (localdb)\MSSQLLocalDB
-- =====================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AsiBasecodeDB')
BEGIN
    CREATE DATABASE AsiBasecodeDB;
END
GO

USE AsiBasecodeDB;
GO

-- 1. Users Table (Authentication)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId VARCHAR(50) NOT NULL UNIQUE,
        Name VARCHAR(50) NOT NULL,
        Password VARCHAR(50) NOT NULL,
        CreatedBy VARCHAR(50) NOT NULL,
        CreatedTime DATETIME NOT NULL,
        UpdatedBy VARCHAR(50) NOT NULL,
        UpdatedTime DATETIME NOT NULL
    );
END
GO

-- 2. Members Table (Master Data)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'members')
BEGIN
    CREATE TABLE members (
        member_id INT IDENTITY(1,1) PRIMARY KEY,
        user_id INT NULL,
        student_number VARCHAR(255) NOT NULL UNIQUE,
        full_name VARCHAR(255) NOT NULL,
        course_year VARCHAR(255) NOT NULL,
        section VARCHAR(255) NOT NULL,
        contact_details VARCHAR(255) NULL,
        membership_type VARCHAR(255) NULL,
        status VARCHAR(255) NULL
    );
END
GO

-- 3. Event Types Table (Master Data)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'event_types')
BEGIN
    CREATE TABLE event_types (
        type_id INT IDENTITY(1,1) PRIMARY KEY,
        type_name VARCHAR(255) NOT NULL UNIQUE,
        description TEXT NULL
    );
END
GO

-- 4. Venues Table (Master Data)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'venues')
BEGIN
    CREATE TABLE venues (
        venue_id INT IDENTITY(1,1) PRIMARY KEY,
        room_name VARCHAR(255) NOT NULL,
        capacity INT NOT NULL,
        location VARCHAR(255) NULL,
        availability_notes TEXT NULL,
        contact_person VARCHAR(255) NULL
    );
END
GO

-- =====================================================================
-- Seed Initial Demo / Test Data
-- =====================================================================

-- Seed Members if empty
IF NOT EXISTS (SELECT * FROM members)
BEGIN
    INSERT INTO members (student_number, full_name, course_year, section, contact_details, membership_type, status)
    VALUES 
    ('2023-10482', 'Maria Clara C. Santos', 'BSIT 3rd Year', '3-A', 'maria.santos@ustp.edu.ph', 'Officer', 'active'),
    ('2023-09821', 'Adrian Jose V. Ramirez', 'BSIT 3rd Year', '3-B', 'adrian.ramirez@ustp.edu.ph', 'Regular', 'active'),
    ('2022-04112', 'Danica Mae P. Navarro', 'BSCS 4th Year', '4-A', 'danica.navarro@ustp.edu.ph', 'Officer', 'active'),
    ('2024-11002', 'Joshua Gabriel T. Cruz', 'BSIT 2nd Year', '2-C', 'joshua.cruz@ustp.edu.ph', 'Regular', 'inactive');
END
GO

-- Seed Venues if empty
IF NOT EXISTS (SELECT * FROM venues)
BEGIN
    INSERT INTO venues (room_name, capacity, location, availability_notes, contact_person)
    VALUES 
    ('Main Auditorium', 150, 'College of Engineering, Ground Floor', 'Full audio system and dual projectors available.', 'Engr. D. Santos'),
    ('IT Multimedia Lab 401', 60, 'Information Technology Building, 4th Floor', 'Computer workstations and central teaching console.', 'Prof. M. Vance');
END
GO

-- Seed Event Types if empty
IF NOT EXISTS (SELECT * FROM event_types)
BEGIN
    INSERT INTO event_types (type_name, description)
    VALUES 
    ('Seminar', 'Academic and technical discussion sessions led by invited guest speakers.'),
    ('Workshop', 'Hands-on practical training and collaborative building exercises.'),
    ('Recognition Ceremony', 'Honoring organization achievements, officers, and active student members.');
END
GO
