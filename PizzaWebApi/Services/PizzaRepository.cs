﻿using System.Data.SqlClient;
using PizzaWebApi.Models;

namespace PizzaWebApi
{
    public class PizzaRepository
    {
        // SEZIONE COMPLETA (30%)
        // ========== CONFIGURAZIONE E METODI BASE ==========

        // Stringa di connessione al database SQL Server
        // Contiene tutte le informazioni necessarie per connettersi al database:
        // - Data Source: il server dove si trova il database (in questo caso, locale)
        // - Initial Catalog: il nome del database
        // - Integrated Security: usa l'autenticazione Windows
        public const string CONNECTION_STRING = "Data Source=localhost;Initial Catalog=PizzaDB;Integrated Security=True;";

        // Metodo che recupera tutte le pizze dal database
        // Il parametro limit è opzionale (può essere null) e limita il numero di risultati
        public async Task<List<Pizza>> GetAllPizzas(int? limit = null)
        {
            // Query SQL che recupera i dati delle pizze e le relative informazioni
            // Usa dei JOIN per collegare le tabelle e ottenere:
            // - Informazioni della categoria associata
            // - Lista degli ingredienti di ogni pizza
            var query = @$"SELECT {(limit == null ? "" : $"TOP {limit}")} p.*, c.Id AS CategoryId, c.Name AS CategoryName,
                                 i.Id AS IngredientId, i.Name AS IngredientName
                        FROM Pizzas p
                        LEFT JOIN Categories c ON p.CategoryId = c.Id
                        LEFT JOIN PizzaIngredient pi ON p.Id = pi.PizzaId
                        LEFT JOIN Ingredients i ON pi.IngredientId = i.Id";

            // Apre una connessione al database
            using var conn = new SqlConnection(CONNECTION_STRING);
            await conn.OpenAsync();

            // Dizionario per tenere traccia delle pizze già create
            // Chiave: ID della pizza
            // Valore: oggetto Pizza corrispondente
            Dictionary<int, Pizza> Pizzas = new Dictionary<int, Pizza>();

            // Esegue la query e processa i risultati
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        GetPizzaFromData(reader, Pizzas);
                    }
                }
            }

            // Restituisce la lista di tutte le pizze trovate
            return Pizzas.Values.ToList();
        }

        // QUIZ 1 (25%): Come implementeresti la ricerca delle pizze per nome?
        // Obiettivo: Creare una query che cerca le pizze per nome
        // Processo logico:
        // 1. La query deve filtrare le pizze per nome
        // 2. Deve includere le stesse JOIN di GetAllPizzas
        // 3. Deve usare parametri SQL per prevenire SQL injection

        /* SCEGLI TRA:
        A)
        public async Task<List<Pizza>> GetPizzasByName(string name)
        {
            var query = "SELECT * FROM Pizzas WHERE name = '" + name + "'";
            using var conn = new SqlConnection(CONNECTION_STRING);
            await conn.OpenAsync();
            Dictionary<int, Pizza> Pizzas = new Dictionary<int, Pizza>();
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        GetPizzaFromData(reader, Pizzas);
                    }
                }
            }
            return Pizzas.Values.ToList();
        }

        B)
        public async Task<List<Pizza>> GetPizzasByName(string name)
        {
            var query = @"SELECT p.*, c.Id AS CategoryId, c.Name AS CategoryName, 
                                 i.Id AS IngredientId, i.Name AS IngredientName
                                 FROM Pizzas p
                                 LEFT JOIN Categories c ON p.CategoryId = c.Id
                                 LEFT JOIN PizzaIngredient pi ON p.Id = pi.PizzaId
                                 LEFT JOIN Ingredients i ON pi.IngredientId = i.Id
                          WHERE p.name=@name";
            using var conn = new SqlConnection(CONNECTION_STRING);
            await conn.OpenAsync();
            Dictionary<int, Pizza> Pizzas = new Dictionary<int, Pizza>();
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@name", name));
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        GetPizzaFromData(reader, Pizzas);
                    }
                }
            }
            return Pizzas.Values.ToList();
        }

        C)
        public async Task<List<Pizza>> GetPizzasByName(string name)
        {
            return (await GetAllPizzas()).Where(p => p.Name.Contains(name)).ToList();
        }
        */

        // QUIZ 2 (25%): Come implementeresti l'inserimento di una nuova pizza con i suoi ingredienti?
        // Obiettivo: Inserire una pizza e gestire la relazione con gli ingredienti
        // Processo logico:
        // 1. Inserire prima la pizza base
        // 2. Recuperare l'ID della pizza appena inserita
        // 3. Inserire le relazioni con gli ingredienti

        /* SCEGLI TRA:
        A)
        public async Task<int> InsertPizza(Pizza pizza)
        {
            using var conn = new SqlConnection(CONNECTION_STRING);
            await conn.OpenAsync();
            var query = "INSERT INTO Pizzas (Name, Description, Price) VALUES (@name, @description, @price)";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@name", pizza.Name));
                cmd.Parameters.Add(new SqlParameter("@description", pizza.Description));
                cmd.Parameters.Add(new SqlParameter("@price", pizza.Price));
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        B)
        public async Task<int> InsertPizza(Pizza pizza)
        {
            using var conn = new SqlConnection(CONNECTION_STRING);
            await conn.OpenAsync();
            var query = "INSERT INTO Pizzas (Name, Description, Price, CategoryId) " +
                       "VALUES (@name, @description, @price, @categoryId);" +
                       "SELECT SCOPE_IDENTITY()";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@name", pizza.Name));
                cmd.Parameters.Add(new SqlParameter("@description", pizza.Description));
                cmd.Parameters.Add(new SqlParameter("@price", pizza.Price));
                cmd.Parameters.Add(new SqlParameter("@categoryId", pizza.CategoryId ?? (object)DBNull.Value));
                int pizzaId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                await HandleIngredients(pizza.IngredientIds, pizzaId, conn);
                return pizzaId;
            }
        }

        C)
        public async Task<int> InsertPizza(Pizza pizza)
        {
            var pizzas = await GetAllPizzas();
            int newId = pizzas.Max(p => p.Id) + 1;
            pizza.Id = newId;
            pizzas.Add(pizza);
            return newId;
        }
        */

        // SEZIONE DA COMPLETARE (20%)
        // Obiettivo: Implementare il metodo che aggiorna una pizza esistente
        // Tips:
        // 1. Il metodo deve accettare l'ID della pizza da modificare e i nuovi dati
        // 2. La query UPDATE deve modificare tutti i campi della pizza
        // 3. Usa parametri SQL per prevenire SQL injection
        // 4. Devi gestire anche gli ingredienti (usa HandleIngredients)
        // 5. Restituisci il numero di righe modificate

        // Il tuo codice qui...
    }
}