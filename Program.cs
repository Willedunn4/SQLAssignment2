using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

class Program
{
    static void Main()
    {
        string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PROG260FA23;Integrated Security=True";
        //Create Characters Table and Seed Data
        CreateCharactersTableAndSeedData(connectionString);

        // Import Data from CSV
        ImportDataFromCSV(connectionString);

        // Execute SQL Queries and Save Outputs
        ExecuteSQLQueriesAndSaveOutputs(connectionString);

        Console.WriteLine("Data import and SQL operations completed. Press any key to exit...");
        Console.ReadKey();
    }

    static void CreateCharactersTableAndSeedData(string connectionString)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Create Characters Table
            string createTableQuery = @"
            CREATE TABLE Characters (
                CharacterID INT PRIMARY KEY,
                Name NVARCHAR(100),
                CharacterType NVARCHAR(50),
                MapLocation NVARCHAR(50),
                OriginalCharacter BIT,
                SwordFighter BIT,
                MagicUser BIT
            )";

            using (var command = new SqlCommand(createTableQuery, connection))
            {
                // command.ExecuteNonQuery();
            }

            // Seed Data into Characters Table
            string seedDataQuery = @"
            INSERT INTO Characters (CharacterID, Name, CharacterType, MapLocation, OriginalCharacter, SwordFighter, MagicUser) VALUES 
            (1, 'Murray', 'Ghost', NULL, 1, 0, 0),
            (2, 'Locke Smith', 'Human', 'Melee Island', 0, 0, 0),
            (3, 'Herman Toothrot', '', 'Terror Island', 0, 0, 0),
            (4, 'Voodoo Lady', 'Melee Island', 'Melee Island', 1, 0, 1),
            -- Add more characters here
            ";

            using (var command = new SqlCommand(seedDataQuery, connection))
            {
                // command.ExecuteNonQuery();
            }
        }
    }

    static void ImportDataFromCSV(string connectionString)
    {
        List<string> swordNonHumanCharacters = new List<string>();
        List<string> lostCharacters = new List<string>();

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Read data from CSV and organize characters based on criteria
            using (var reader = new StreamReader("Chars.csv"))
            {
                reader.ReadLine(); // Skip the header line
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    string characterName = values[0];
                    string characterType = values[1];
                    string mapLocation = values[2];
                    bool originalCharacter = bool.Parse(values[3]);
                    bool swordFighter;
                    if (bool.TryParse(values[4], out swordFighter))
                    {
                        // Successfully parsed the boolean value, proceed with inserting into the database
                    }
                    else
                    {
                        // Failed to parse the boolean value, handle the error or provide a default value
                        swordFighter = false;
                    }
                    bool magicUser;
                    if (bool.TryParse(values[5], out magicUser))
                    {
                        // Successfully parsed the boolean value, proceed with inserting into the database
                    }
                    else
                    {
                        // Failed to parse the boolean value, handle the error or provide a default value
                        magicUser = false;
                    }

                    // Organize characters based on criteria
                    if (swordFighter && !characterType.Equals("Human", StringComparison.OrdinalIgnoreCase))
                    {
                        swordNonHumanCharacters.Add(characterName);
                    }

                    if (string.IsNullOrWhiteSpace(mapLocation))
                    {
                        lostCharacters.Add(characterName);
                    }

                    // Perform the insertion into the database using SQL commands if needed
                     string insertQuery = $"INSERT INTO Characters (Name, CharacterType, MapLocation, OriginalCharacter, SwordFighter, MagicUser) VALUES " +
                                          $"('{characterName}', '{characterType}', '{mapLocation}', {(originalCharacter ? 1 : 0)}, {(swordFighter ? 1 : 0)}, {(magicUser ? 1 : 0)})";
                     using (var command = new SqlCommand(insertQuery, connection))
                     {
                         //command.ExecuteNonQuery();
                     }
                }
            }
        }

        // Write sword non-human characters to SwordNonHuman.txt
        File.WriteAllLines("SwordNonHuman.txt", swordNonHumanCharacters);

        // Write lost characters to Lost.txt
        File.WriteAllLines("Lost.txt", lostCharacters);
    }

    static void ExecuteSQLQueriesAndSaveOutputs(string connectionString)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Execute Inner Join Query and Save Output to FullReport.txt
            string innerJoinQuery = "SELECT * FROM Characters";
            using (var command = new SqlCommand(innerJoinQuery, connection))
            using (var reader = command.ExecuteReader())
            using (StreamWriter writer = new StreamWriter("FullReport.txt"))
            {
                while (reader.Read())
                {
                    // Process rows and write to FullReport.txt
                    writer.WriteLine($"{reader["CharacterID"]} - {reader["Name"]} - {reader["CharacterType"]} - {reader["MapLocation"]} - {reader["OriginalCharacter"]} - {reader["SwordFighter"]} - {reader["MagicUser"]}");
                }
            }

            // Execute Query for Lost Characters and Save Output to Lost.txt
            string lostCharactersQuery = "SELECT * FROM Characters WHERE MapLocation IS NULL";
            using (var command = new SqlCommand(lostCharactersQuery, connection))
            using (var reader = command.ExecuteReader())
            using (StreamWriter writer = new StreamWriter("Lost.txt"))
            {
                while (reader.Read())
                {
                    // Process rows and write to Lost.txt
                    writer.WriteLine($"{reader["CharacterID"]} - {reader["Name"]}");
                }
            }

            // Execute Query for Sword Non-Human Characters and Save Output to SwordNonHuman.txt
            string swordNonHumanCharactersQuery = "SELECT * FROM Characters WHERE CharacterType != 'Human' AND SwordFighter = 1";
            using (var command = new SqlCommand(swordNonHumanCharactersQuery, connection))
            using (var reader = command.ExecuteReader())
            using (StreamWriter writer = new StreamWriter("SwordNonHuman.txt"))
            {
                while (reader.Read())
                {
                    // Process rows and write to SwordNonHuman.txt
                    writer.WriteLine($"{reader["CharacterID"]} - {reader["Name"]}");
                }
            }
        }
    }
}
