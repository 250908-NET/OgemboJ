-- SETUP:
-- Create a database server (docker)
-- docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Iworkhere@2025" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
-- Connect to the server (Azure Data Studio / Database extension)
-- Test your connection with a simple query (like a select)
-- Execute the Chinook database (to create Chinook resources in your db)

-- On the Chinook DB, practice writing queries with the following exercises

-- BASIC CHALLENGES
-- List all customers (full name, customer id, and country) who are not in the USA
SELECT CONCAT(FirstName, ' ', LastName) AS Full_name, CustomerId, Country
FROM Customer
WHERE
    country != 'USA';

-- List all customers from Brazil
SELECT * FROM Customer WHERE Country = 'Brazil';

-- List all sales agents
SELECT * FROM Employee WHERE Title LIKE '%Sales%Agent%';

-- Retrieve a list of all countries in billing addresses on invoices
SELECT BillingAddress, BillingCountry from Invoice;

-- Retrieve how many invoices there were in 2009, and what was the sales total for that year?
SELECT
    COUNT(InvoiceId) AS NumberOfInvoices,
    SUM(Total) AS SalesTotal,
    YEAR(InvoiceDate) AS InvoiceYear
FROM Invoice
WHERE
    YEAR(InvoiceDate) = 2009
GROUP BY
    YEAR(InvoiceDate);

-- (challenge: find the invoice count sales total for every year using one query)
SELECT
    COUNT(InvoiceID) AS InvoiceCount,
    Sum(Total) AS SalesTotal,
    YEAR(InvoiceDate) AS InvoiceYear
FROM Invoice
GROUP BY
    YEAR(InvoiceDate);

-- how many line items were there for invoice #37
SELECT COUNT(*) AS Items FROM InvoiceLine WHERE InvoiceId = 37;

-- how many invoices per country? BillingCountry  # of invoices -
SELECT
    BillingCountry,
    COUNT(*) AS NumberOfInvoices
From Invoice
GROUP BY
    BillingCountry
ORDER BY
    NumberOfInvoices DESC,
    BillingCountry;

-- Retrieve the total sales per country, ordered by the highest total sales first.
SELECT BillingCountry, SUM(Total) AS TotalSales
FROM Invoice
GROUP BY
    BillingCountry
ORDER BY TotalSales DESC;

-- JOINS CHALLENGES
-- Every Album by Artist
SELECT t1.Name AS ArtistName, t2.Title AS AlbumTitle
FROM Artist AS t1
    JOIN Album AS t2 ON t1.ArtistId = t2.ArtistId;

-- All songs of the rock genre
SELECT t1.Name AS SongName
FROM Track AS t1
    JOIN Genre AS t2 ON t1.GenreId = t2.GenreId
WHERE
    t2.Name = 'Rock';

-- Show all invoices of customers from brazil (mailing address not billing)
SELECT i.*
FROM Customer c
    JOIN Invoice i ON i.CustomerId = c.CustomerId
WHERE
    c.Country = 'Brazil'
ORDER BY i.InvoiceDate;

-- Show all invoices together with the name of the sales agent for each one
SELECT
    t1.InvoiceId,
    t3.FirstName AS AgentFirstName,
    t3.LastName AS AgentLastName
FROM
    Invoice AS t1
    JOIN Customer AS t2 ON t1.CustomerId = t2.CustomerId
    JOIN Employee AS t3 ON t2.SupportRepId = t3.EmployeeId;

-- Which sales agent made the most sales in 2009?
SELECT
    TOP 1 e.EmployeeId,
    e.FirstName + ' ' + e.LastName AS SalesAgent,
    SUM(i.Total) AS Sales2009
FROM
    Employee e
    JOIN Customer c ON c.SupportRepId = e.EmployeeId
    JOIN Invoice i ON i.CustomerId = c.CustomerId
WHERE
    e.Title LIKE '%Sales%Agent%'
    AND YEAR(InvoiceDate) = 2009
GROUP BY
    e.EmployeeId,
    e.FirstName,
    e.LastName
ORDER BY Sales2009 DESC;

-- How many customers are assigned to each sales agent?
SELECT
    e.EmployeeId,
    e.FirstName + ' ' + e.LastName AS SalesAgent,
    COUNT(c.CustomerId) AS CustomerCount
FROM Employee e
    LEFT JOIN Customer c ON c.SupportRepId = e.EmployeeId
WHERE
    e.Title LIKE '%Sales%Agent%'
GROUP BY
    e.EmployeeId,
    e.FirstName,
    e.LastName
ORDER BY CustomerCount DESC, SalesAgent;

-- Which track was purchased the most ing 2010?
SELECT TOP 1 t.TrackId, t.Name AS Track, SUM(il.Quantity) AS QtyPurchased
FROM
    InvoiceLine il
    JOIN Invoice i ON i.InvoiceId = il.InvoiceId
    JOIN Track t ON t.TrackId = il.TrackId
WHERE
    YEAR(InvoiceDate) = 2010
GROUP BY
    t.TrackId,
    t.Name
ORDER BY QtyPurchased DESC;

-- Show the top three best selling artists.
SELECT TOP 3 t4.Name, SUM(t1.Quantity * t1.UnitPrice) AS TotalSales
FROM
    InvoiceLine AS t1
    JOIN Track AS t2 ON t1.TrackId = t2.TrackId
    JOIN Album AS t3 ON t2.AlbumId = t3.AlbumId
    JOIN Artist AS t4 ON t3.ArtistId = t4.ArtistId
GROUP BY
    t4.Name
ORDER BY TotalSales DESC;

-- Which customers have the same initials as at least one other customer?
SELECT DISTINCT
    t1.FirstName,
    t1.LastName
FROM Customer AS t1
    JOIN Customer AS t2 ON t1.CustomerId != t2.CustomerId
WHERE
    substring(t1.FirstName, 1, 1) = substring(t2.FirstName, 1, 1)
    AND substring(t1.LastName, 1, 1) = substring(t2.LastName, 1, 1)
ORDER BY t1.FirstName, t1.LastName;

-- ADVACED CHALLENGES
-- solve these with a mixture of joins, subqueries, CTE, and set operators.
SELECT Name
FROM Artist
WHERE
    ArtistId NOT IN (
        SELECT DISTINCT
            ArtistId
        FROM Album
    );
-- solve at least one of them in two different ways, and see if the execution

-- 1. which artists did not make any albums at all?
SELECT ar.ArtistId, ar.Name
FROM Artist ar
    LEFT JOIN Album al ON al.ArtistId = ar.ArtistId
WHERE
    al.AlbumId IS NULL
ORDER BY ar.Name;

-- 1) Alternative
SELECT ar.ArtistId, ar.Name
FROM Artist ar
WHERE
    NOT EXISTS (
        SELECT 1
        FROM Album al
        WHERE
            al.ArtistId = ar.ArtistId
    )
ORDER BY ar.Name;

-- 2. which artists did not record any tracks of the Latin genre?
SELECT ar.ArtistId, ar.Name
FROM Artist ar
WHERE
    NOT EXISTS (
        SELECT 1
        FROM Album al
            JOIN Track t ON t.AlbumId = al.AlbumId
            JOIN Genre g ON g.GenreId = t.GenreId
        WHERE
            al.ArtistId = ar.ArtistId
            AND g.Name = 'Latin'
    )
ORDER BY ar.Name;

-- 2) Alternative
SELECT ArtistId, Name
FROM Artist
EXCEPT
SELECT DISTINCT
    ar.ArtistId,
    ar.Name
FROM
    Artist ar
    JOIN Album al ON al.ArtistId = ar.ArtistId
    JOIN Track t ON t.AlbumId = al.AlbumId
    JOIN Genre g ON g.GenreId = t.GenreId
WHERE
    g.Name = 'Latin'
ORDER BY Name;

-- 3. which video track has the longest length? (use media type table)
SELECT TOP 1 t1.Name, t1.Milliseconds
FROM Track AS t1
    JOIN MediaType AS t2 ON t1.MediaTypeId = t2.MediaTypeId
WHERE
    t2.Name LIKE '%video%'
ORDER BY t1.Milliseconds DESC;

-- 4. find the names of the customers who live in the same city as the
--    boss employee (the one who reports to nobody)
SELECT FirstName, LastName, City
FROM Customer
WHERE
    City = (
        SELECT City
        FROM Employee
        WHERE
            ReportsTo IS NULL
    );

-- 5. how many audio tracks were bought by German customers, and what was
--    the total price paid for them?
SELECT
    COUNT(t1.TrackId) AS TotalAudioTracks,
    SUM(t1.UnitPrice) AS TotalPrice
FROM
    InvoiceLine AS t1
    JOIN Invoice AS t2 ON t1.InvoiceId = t2.InvoiceId
    JOIN Customer AS t3 ON t2.CustomerId = t3.CustomerId
    JOIN Track AS t4 ON t1.TrackId = t4.TrackId
    JOIN MediaType AS t5 ON t4.MediaTypeId = t5.MediaTypeId
WHERE
    t3.Country = 'Germany'
    AND t5.Name = 'MPEG audio file';

-- 6. list the names and countries of the customers supported by an employee
--    who was hired younger than 35.
SELECT t1.FirstName, t1.LastName, t1.Country
FROM Customer AS t1
    JOIN Employee AS t2 ON t1.SupportRepId = t2.EmployeeId
WHERE
    DATEDIFF(
        year,
        t2.BirthDate,
        t2.HireDate
    ) < 35;
-- DML exercises

-- 1. insert two new records into the employee table.
INSERT INTO
    Employee (
        EmployeeId,
        LastName,
        FirstName,
        Title,
        ReportsTo,
        BirthDate,
        HireDate,
        Address,
        City,
        State,
        Country,
        PostalCode,
        Phone,
        Fax,
        Email
    )
VALUES (
        9,
        'Doe',
        'John',
        'IT Staff',
        6,
        '1990-01-01 00:00:00',
        '2024-01-01 00:00:00',
        '123 Main St',
        'New York',
        'NY',
        'USA',
        '10001',
        '+1 (212) 555-1234',
        NULL,
        'john.doe@chinookcorp.com'
    ),
    (
        10,
        'Smith',
        'Jane',
        'IT Staff',
        6,
        '1992-02-02 00:00:00',
        '2024-02-02 00:00:00',
        '456 Elm St',
        'Los Angeles',
        'CA',
        'USA',
        '90210',
        '+1 (310) 555-5678',
        NULL,
        'jane.smith@chinookcorp.com'
    );

-- 2. insert two new records into the tracks table.
INSERT INTO
    Track (
        TrackId,
        Name,
        AlbumId,
        MediaTypeId,
        GenreId,
        Composer,
        Milliseconds,
        Bytes,
        UnitPrice
    )
VALUES (
        3504,
        'New Track One',
        1,
        1,
        1,
        'New Composer',
        180000,
        5000000,
        0.99
    ),
    (
        3505,
        'New Track Two',
        1,
        1,
        1,
        'Another Composer',
        200000,
        5500000,
        0.99
    );

-- 3. update customer Aaron Mitchell's name to Robert Walter
UPDATE Customer
SET
    FirstName = 'Robert',
    LastName = 'Walter'
WHERE
    FirstName = 'Aaron'
    AND LastName = 'Mitchell';

-- 4. delete one of the employees you inserted.
DELETE FROM Employee WHERE EmployeeId = 9;

-- 5. delete customer Robert Walter.
DELETE FROM Customer
WHERE
    FirstName = 'Robert'
    AND LastName = 'Walter';