using BaşarsoftStaj.Entity;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Models;
using Npgsql;
using System.Data;
using System.Text.RegularExpressions;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;

namespace BaşarsoftStaj.Services;

public class ShapeServiceADO : IShapeService
{
    private readonly string _connectionString;
    private readonly string _masterConnectionString;
    
    public ShapeServiceADO(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var builder = new NpgsqlConnectionStringBuilder(_connectionString);
        var databaseName = builder.Database;
        builder.Database = "postgres";
        _masterConnectionString = builder.ConnectionString;
        
        InitializeDatabase(databaseName);
    }
    
    private void InitializeDatabase(string databaseName)
    {
        try
        {
           
            using (var masterConnection = new NpgsqlConnection(_masterConnectionString))
            {
                masterConnection.Open();
                
             
                var checkDbCommand = new NpgsqlCommand(
                    "SELECT 1 FROM pg_database WHERE datname = @dbname", masterConnection);
                checkDbCommand.Parameters.AddWithValue("@dbname", databaseName);
                
                var dbExists = checkDbCommand.ExecuteScalar() != null;
                
                if (!dbExists)
                {
                    
                    var createDbCommand = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", masterConnection);
                    createDbCommand.ExecuteNonQuery();
                }
            }
            
            
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                
                var createTableCommand = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS Points (
                        Id SERIAL PRIMARY KEY,
                        Name VARCHAR(100) NOT NULL,
                        Geometry GEOMETRY NOT NULL
                    )", connection);
                
                createTableCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize database: {ex.Message}", ex);
        }
    }

    public ApiResponse<List<Shape>> GetAllPoints()
    {
        try
        {
            var points = new List<Shape>();
            
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            var command = new NpgsqlCommand("SELECT Id, Name, ST_AsText(Geometry) as WKT FROM Points ORDER BY Id", connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                var point = new Shape
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    Geometry = new WKTReader().Read(reader.GetString("WKT"))
                };
                points.Add(point);
            }
            
            return ApiResponse<List<Shape>>.SuccessResponse(points, "PointsRetrievedSuccessfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<Shape>>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<Shape> GetPointById(int id)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            var command = new NpgsqlCommand("SELECT Id, Name, ST_AsText(Geometry) as WKT FROM Points WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var point = new Shape
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    Geometry = new WKTReader().Read(reader.GetString("WKT"))
                };
                
                return ApiResponse<Shape>.SuccessResponse(point, "PointRetrievedSuccessfully");
            }
            
            return ApiResponse<Shape>.ErrorResponse("PointNotFound");
        }
        catch (Exception ex)
        {
            return ApiResponse<Shape>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<Shape> AddPoint(AddShapeDto pointDto)
    {
        if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || pointDto.Geometry == null)
        {
            return ApiResponse<Shape>.ErrorResponse("ValidationError");
        }

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            var command = new NpgsqlCommand(@"
                INSERT INTO Points (Name, Geometry) 
                VALUES (@Name, ST_GeomFromText(@WKT, 4326)) 
                RETURNING Id, Name, ST_AsText(Geometry) as WKT", connection);
            
            command.Parameters.AddWithValue("@Name", pointDto.Name);
            command.Parameters.AddWithValue("@WKT", new WKTWriter().Write(pointDto.Geometry));
            
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var point = new Shape
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    Geometry = new WKTReader().Read(reader.GetString("WKT"))
                };
                
                return ApiResponse<Shape>.SuccessResponse(point, "PointAddedSuccessfully");
            }
            
            return ApiResponse<Shape>.ErrorResponse("FailedToAddPoint");
        }
        catch (Exception ex)
        {
            return ApiResponse<Shape>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<List<Shape>> AddRangePoints(List<AddShapeDto> pointDtos)
    {
        if (pointDtos == null || !pointDtos.Any())
        {
            return ApiResponse<List<Shape>>.ErrorResponse("InvalidInput");
        }

        // Validate all points before processing
        foreach (var pointDto in pointDtos)
        {
            if (pointDto == null || string.IsNullOrEmpty(pointDto.Name) || pointDto.Geometry == null)
            {
                return ApiResponse<List<Shape>>.ErrorResponse("ValidationError");
            }
        }

        try
        {
            var addedPoints = new List<Shape>();
            
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            using var transaction = connection.BeginTransaction();
            
            try
            {
                foreach (var pointDto in pointDtos)
                {
                    var command = new NpgsqlCommand(@"
                        INSERT INTO Points (Name, Geometry) 
                        VALUES (@Name, ST_GeomFromText(@WKT, 4326)) 
                        RETURNING Id, Name, ST_AsText(Geometry) as WKT", connection, transaction);
                    
                    command.Parameters.AddWithValue("@Name", pointDto.Name);
                    command.Parameters.AddWithValue("@WKT", new WKTWriter().Write(pointDto.Geometry));
                    
                    using var reader = command.ExecuteReader();
                    
                    if (reader.Read())
                    {
                        var point = new Shape
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name"),
                            Geometry = new WKTReader().Read(reader.GetString("WKT"))
                        };
                        addedPoints.Add(point);
                    }
                    
                    reader.Close();
                }
                
                transaction.Commit();
                return ApiResponse<List<Shape>>.SuccessResponse(addedPoints, "PointsAddedSuccessfully");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            return ApiResponse<List<Shape>>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<Shape> UpdatePoint(int id, string newName, Geometry newGeometry)
    {
        if (string.IsNullOrEmpty(newName) && newGeometry == null)
        {
            return ApiResponse<Shape>.ErrorResponse("ValidationError");
        }

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            // First check if the point exists
            var checkCommand = new NpgsqlCommand("SELECT COUNT(*) FROM Points WHERE Id = @Id", connection);
            checkCommand.Parameters.AddWithValue("@Id", id);
            var exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
            
            if (!exists)
            {
                return ApiResponse<Shape>.ErrorResponse("PointNotFound");
            }

            // Build dynamic update query
            var setParts = new List<string>();
            var command = new NpgsqlCommand();
            command.Connection = connection;
            command.Parameters.AddWithValue("@Id", id);

            if (!string.IsNullOrEmpty(newName))
            {
                setParts.Add("Name = @Name");
                command.Parameters.AddWithValue("@Name", newName);
            }

            if (newGeometry != null)
            {
                setParts.Add("Geometry = ST_GeomFromText(@WKT, 4326)");
                command.Parameters.AddWithValue("@WKT", new WKTWriter().Write(newGeometry));
            }

            command.CommandText = $@"
                UPDATE Points 
                SET {string.Join(", ", setParts)}
                WHERE Id = @Id 
                RETURNING Id, Name, ST_AsText(Geometry) as WKT";
            
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var point = new Shape
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    Geometry = new WKTReader().Read(reader.GetString("WKT"))
                };
                
                return ApiResponse<Shape>.SuccessResponse(point, "PointUpdatedSuccessfully");
            }
            
            return ApiResponse<Shape>.ErrorResponse("PointNotFound");
        }
        catch (Exception ex)
        {
            return ApiResponse<Shape>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }

    public ApiResponse<Shape> DeletePoint(int id)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            
            var command = new NpgsqlCommand(@"
                DELETE FROM Points 
                WHERE Id = @Id 
                RETURNING Id, Name, ST_AsText(Geometry) as WKT", connection);
            
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var point = new Shape
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    Geometry = new WKTReader().Read(reader.GetString("WKT"))
                };
                
                return ApiResponse<Shape>.SuccessResponse(point, "PointDeletedSuccessfully");
            }
            
            return ApiResponse<Shape>.ErrorResponse("PointNotFound");
        }
        catch (Exception ex)
        {
            return ApiResponse<Shape>.ErrorResponse($"DatabaseError: {ex.Message}");
        }
    }
}