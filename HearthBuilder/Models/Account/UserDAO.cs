﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HearthBuilder.Models.Account
{
    public sealed class UserDAO
    {
        private static UserDAO instance;
        private static object syncRoot = new Object();

        private MySqlConnection connection;
        private MySqlCommand cmd;

        private UserDAO()
        {
            connection = new MySqlConnection();
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
        }

        public static UserDAO Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        instance = new UserDAO();
                    }
                }
                return instance;
            }
        }

        static string Hash(string input)
        {
            var hash = (new SHA1Managed()).ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }

        public User GetAccountByEmailAndPassword(User user)
        {
            String passHash = Hash(user.Email + ":" + user.Password);

            try
            {
                using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString))
                {
                    connection.Open();
                    cmd = new MySqlCommand("SELECT * FROM account WHERE email = @email AND password = @passHash", connection);
                    cmd.Parameters.AddWithValue("@email", user.Email);
                    cmd.Parameters.AddWithValue("@passHash", passHash);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            //map values to user obj
                            user.ID = Convert.ToInt32(reader.GetString("account_id"));
                            user.FirstName = reader.GetString("first_name");
                            user.LastName = reader.GetString("last_name");
                            user.Email = reader.GetString("email");
                            user.Password = reader.GetString("password");

                            return user;
                        }
                        else
                        {
                            throw new UserException("Could not find matching username and password combination!");
                        }
                    }
                }
            }
            catch (MySqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                throw e;
            }
        }

        public User GetUserbyId(int id)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString))
                {
                    connection.Open();
                    cmd = new MySqlCommand("SELECT * FROM account WHERE account_id = @id", connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            User user = new User();
                            //map values to user obj
                            user.ID = reader.GetInt32("account_id");
                            user.FirstName = reader.GetString("first_name");
                            user.LastName = reader.GetString("last_name");
                            user.Email = reader.GetString("email");
                            user.Password = reader.GetString("password");

                            return user;
                        }
                        else
                        {
                            throw new UserException("Could not find user matching Id! - " + id);
                        }
                    }
                }
            }
            catch (MySqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                throw e;
            }
        }

        public User GetUserbyEmail(string email)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString))
                {
                    connection.Open();
                    cmd = new MySqlCommand("SELECT * FROM account WHERE email = @email", connection);
                    cmd.Parameters.AddWithValue("@email", email);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            User user = new User();
                            //map values to user obj
                            user.ID = reader.GetInt32("account_id");
                            user.FirstName = reader.GetString("first_name");
                            user.LastName = reader.GetString("last_name");
                            user.Email = reader.GetString("email");
                            user.Password = reader.GetString("password");

                            return user;
                        }
                        else
                        {
                            throw new UserException("Could not find user matching email! - " + email);
                        }
                    }
                }
            }
            catch (MySqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                throw e;
            }
        }

        public User RegisterUser(User user)
        {
            String passHash = Hash(user.Email + ":" + user.Password);

            try
            {
                using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString))
                {
                    connection.Open();
                    cmd = new MySqlCommand("SELECT * FROM account WHERE email = @email", connection);
                    cmd.Parameters.AddWithValue("@email", user.Email);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) //is there a result?
                        {
                            throw new UserException("That email address is already in use!");
                        }
                    }
                    //register them
                    cmd = new MySqlCommand("INSERT INTO account (first_name, last_name, email, password) VALUES (@firstname, @lastname, @email, @passHash)", connection);
                    cmd.Parameters.AddWithValue("@firstname", user.FirstName);
                    cmd.Parameters.AddWithValue("@lastname", user.LastName);
                    cmd.Parameters.AddWithValue("@email", user.Email);
                    cmd.Parameters.AddWithValue("@passHash", passHash);
                    cmd.ExecuteNonQuery();

                    int userId = 0;
                    int.TryParse(cmd.LastInsertedId.ToString(), out userId); //get the mysql generated last ID
                    user.ID = userId;

                    return user;
                }
            }
            catch (MySqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                throw e;
            }
        }

        public User DeleteUser(User user)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString))
                {
                    connection.Open();
                    cmd = new MySqlCommand("DELETE FROM account WHERE account_id = @id", connection);
                    cmd.Parameters.AddWithValue("@id", user.ID);
                    return null;
                }
            }
            catch (MySqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                throw e;
            }
        }
    }
}