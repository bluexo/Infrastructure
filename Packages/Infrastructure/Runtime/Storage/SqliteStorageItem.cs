using Newtonsoft.Json;

using SQLite4Unity3d;

using System;
using System.Data.SqlClient;
using System.Linq;

using UnityEngine;

namespace Origine
{

    public class SqliteStorageItem<T> : IStorageItem<T> where T : new()
    {
        [Table(TableName)]
        public class Definition
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Key { get; set; }
            public string Json { get; set; }
        }

        public string Key { get; private set; }
        public bool IsNullOrEmpty => string.IsNullOrWhiteSpace(definition?.Json);

        public const string TableName = "UserData";
        private readonly SQLiteConnection _connection;
        private Definition definition;

        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new WritablePropertiesOnlyResolver()
        };

        public SqliteStorageItem(SQLiteConnection connection, string key)
        {
            Key = key;
            _connection = connection;
            if (!_connection.TableMappings.Any(t => t.TableName == TableName))
                _connection.CreateTable<Definition>();

            definition = _connection.Table<Definition>().FirstOrDefault(f => f.Key == key);
            if (definition == null)
            {
                definition = new Definition { Key = key, Json = string.Empty };
                _connection.Insert(definition);
            }
            if (!string.IsNullOrWhiteSpace(definition.Json))
                FromJson(definition.Json);
        }

        private T data;
        public T Value
        {
            get => data;
            set
            {
                data = value;
                Save();
            }
        }

        public string ToJson() => JsonConvert.SerializeObject(Value, SerializerSettings);

        public void FromJson(string json) => Value = JsonConvert.DeserializeObject<T>(json, SerializerSettings);

        public void Save()
        {
            definition.Key = Key;
            definition.Json = ToJson();
            _connection.Update(definition);
        }

        public void Clear() => _connection.Delete(definition);
    }
}