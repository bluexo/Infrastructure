using Newtonsoft.Json;

using UltraLiteDB;
using UnityEngine;
using System;
using System.Linq;

namespace Origine
{

    public class LiteDBStorageItem<T> : IStorageItem<T> where T : new()
    {
        public class Definition
        {
            public ObjectId Id { get; set; }
            public string Key { get; set; }
            public string Json { get; set; }
        }

        public string Key { get; private set; }
        public bool IsNullOrEmpty
        {
            get
            {
                var doc = _collection.FindOne(_filter);
                return doc == null || string.IsNullOrWhiteSpace(doc.Json);
            }
        }

        public const string TableName = "UserData";
        private readonly UltraLiteDatabase _database;
        private readonly UltraLiteCollection<Definition> _collection;
        private readonly Query _filter;

        public LiteDBStorageItem(UltraLiteDatabase db, string key)
        {
            Key = key;
            _database = db;
            _collection = _database.GetCollection<Definition>(TableName);
            _filter = Query.EQ("Key", new BsonValue(key));
            var doc = _collection.FindOne(_filter);
            if (doc == null)
                _collection.Insert(new Definition { Key = key });
            else if (!string.IsNullOrWhiteSpace(doc.Json))
                data = JsonConvert.DeserializeObject<T>(doc.Json);
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

        public string ToJson() => JsonConvert.SerializeObject(Value);

        public void FromJson(string json)
        {
            try
            {
                Value = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception exc)
            {
                Debug.LogError($"StorageItem= {Key}, FromJson={json} error , {exc}");
                throw exc;
            }
        }

        public void Save()
        {
            var doc = _collection.FindOne(_filter);
            doc.Json = JsonConvert.SerializeObject(data);
            _collection.Update(doc);
        }

        public void Clear() => _collection.Delete(_filter);
    }
}