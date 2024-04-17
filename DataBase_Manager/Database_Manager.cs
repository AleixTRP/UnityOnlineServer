﻿using MySql.Data.MySqlClient;

public class DatabaseManager
{
    private MySqlConnection connection;

    public DatabaseManager()
    {
        // Connection string to connect with the database
        string connectionString = "Server=127.0.0.1;Port=3306;database=duckgame;Uid=root;password=;SSL Mode=None;connect timeout=3600;default command timeout=3600;";
        // Establish connection with the database
        connection = new MySqlConnection(connectionString);

        try
        {
            // Try to connect with the database
            connection.Open();
            Console.WriteLine("Connection to the database established successfully.");
        }
        catch (Exception ex)
        {
            // If connection fails, display error
            Console.WriteLine("Error connecting to the database: " + ex.Message);
        }
    }

    // Register new user in the database
    public bool Register(string nickname, string password, string race)
    {
        // Check if the user already exists in the database
        string checkCommandString = "SELECT * FROM Usuarios WHERE apodo = @nickname";
        MySqlCommand checkCommand = new MySqlCommand(checkCommandString, connection);
        checkCommand.Parameters.AddWithValue("@nickname", nickname);
        MySqlDataReader reader = checkCommand.ExecuteReader();
        bool userExists = reader.HasRows;
        reader.Close();

        if (userExists)
        {
            Console.WriteLine("The user already exists.");
            return false;
        }

        // Insert new user into the database
        string commandString = "INSERT INTO Usuarios (apodo, contraseña, raza) VALUES (@nickname, @password, @race)";
        MySqlCommand command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@nickname", nickname);
        command.Parameters.AddWithValue("@password", password);
        command.Parameters.AddWithValue("@race", race);
        try
        {
            // Execute user insertion command in the database
            command.ExecuteNonQuery();
            Console.WriteLine("Successful insertion.");
            return true;
        }
        catch (Exception ex)
        {
            // If the command fails, display error
            Console.WriteLine("Error executing command: " + ex.Message);
            return false;
        }
    }

    // Log in a user within the database
    public string Login(string nickname, string password)
    {
        // Check if the login is correct using a SELECT query
        string commandString = "SELECT * FROM Usuarios WHERE apodo = @nickname AND contraseña = @password";
        MySqlCommand command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@nickname", nickname);
        command.Parameters.AddWithValue("@password", password);
        MySqlDataReader reader = command.ExecuteReader();
        bool loginSuccessful = reader.HasRows;

        if (loginSuccessful)
        {
            // The user exists, so we get its race
            reader.Read();
            string race = reader["raza"].ToString();
            reader.Close();

            // Now we get the values of speed and jumpforce for that race
            string raceCommandString = "SELECT velocidad, fuerza_salto FROM Razas WHERE raza = @race";
            MySqlCommand raceCommand = new MySqlCommand(raceCommandString, connection);
            raceCommand.Parameters.AddWithValue("@race", race);
            MySqlDataReader raceReader = raceCommand.ExecuteReader();

            if (raceReader.HasRows)
            {
                // The race exists, so we get the values of speed and jumpforce
                raceReader.Read();
                float speed = float.Parse(raceReader["velocidad"].ToString());
                float jumpforce = float.Parse(raceReader["fuerza_salto"].ToString());
                raceReader.Close();

                // Return speed and jumpforce values as part of the response
                return "true/" + speed.ToString() + "/" + jumpforce.ToString();
            }
            else
            {
                // The race does not exist, so we return an error
                raceReader.Close();
                return "false";
            }
        }
        else
        {
            // The user does not exist, so we return an error
            reader.Close();
            return "false";
        }
    }
}
