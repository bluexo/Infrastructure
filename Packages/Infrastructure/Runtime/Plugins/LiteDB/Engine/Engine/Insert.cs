﻿using System;
using System.Collections.Generic;

namespace UltraLiteDB
{
    public partial class UltraLiteEngine
    {
        /// <summary>
        /// Implements insert documents in a collection - returns _id value
        /// </summary>
        public BsonValue Insert(string collection, BsonDocument doc, BsonAutoId autoId = BsonAutoId.ObjectId)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));

            this.Insert(collection, new BsonDocument[] { doc }, autoId);

            return doc["_id"];
        }

        /// <summary>
        /// Implements insert documents in a collection - use a buffer to commit transaction in each buffer count
        /// </summary>
        public int Insert(string collection, IEnumerable<BsonDocument> docs, BsonAutoId autoId = BsonAutoId.ObjectId)
        {
            if (collection.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(collection));
            if (docs == null) throw new ArgumentNullException(nameof(docs));

            return this.Transaction<int>(collection, true, (col) =>
            {
                var count = 0;

                foreach (var doc in docs)
                {
                    this.InsertDocument(col, doc, autoId);

                    _trans.CheckPoint();

                    count++;
                }

                return count;
            });
        }

        /// <summary>
        /// Bulk documents to a collection - use data chunks for most efficient insert
        /// </summary>
        public int InsertBulk(string collection, IEnumerable<BsonDocument> docs, int batchSize = 5000, BsonAutoId autoId = BsonAutoId.ObjectId)
        {
            if (collection.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(collection));
            if (docs == null) throw new ArgumentNullException(nameof(docs));
            if (batchSize < 100 || batchSize > 100000) throw new ArgumentException("batchSize must be a value between 100 and 100000");

            var count = 0;

            foreach(var batch in docs.Batch(batchSize))
            {
                count += this.Insert(collection, batch, autoId);
            }

            return count;
        }

        /// <summary>
        /// Bulk upsert documents to a collection - use data chunks for most efficient insert
        /// </summary>
        public int UpsertBulk(string collection, IEnumerable<BsonDocument> docs, int batchSize = 5000, BsonAutoId autoId = BsonAutoId.ObjectId)
        {
            if (collection.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(collection));
            if (docs == null) throw new ArgumentNullException(nameof(docs));
            if (batchSize < 100 || batchSize > 100000) throw new ArgumentException("batchSize must be a value between 100 and 100000");

            var count = 0;

            foreach (var batch in docs.Batch(batchSize))
            {
                count += this.Upsert(collection, batch, autoId);
            }

            return count;
        }

        /// <summary>
        /// Internal implementation of insert a document
        /// </summary>
        private void InsertDocument(CollectionPage col, BsonDocument doc, BsonAutoId autoId)
        {
            // collection Sequence was created after release current datafile version. 
            // In this case, Sequence will be 0 but already has documents. Let's fix this
            // ** this code can be removed when datafile change from 7 (HeaderPage.FILE_VERSION) **
            if (col.Sequence == 0 && col.DocumentCount > 0)
            {
                var max = this.Max(col.CollectionName, "_id");

                // if max value is a number, convert to Sequence last value
                // if not, just set sequence as document count
                col.Sequence = (max.IsInt32 || max.IsInt64 || max.IsDouble || max.IsDecimal) ?
                    Convert.ToInt64(max.RawValue) :
                    Convert.ToInt64(col.DocumentCount);
            }

            // increase collection sequence _id
            col.Sequence++;

            _pager.SetDirty(col);

            // if no _id, add one
            if (!doc.RawValue.TryGetValue("_id", out var id))
            {
                doc["_id"] = id =
                    autoId == BsonAutoId.ObjectId ? new BsonValue(ObjectId.NewObjectId()) :
                    autoId == BsonAutoId.Guid ? new BsonValue(Guid.NewGuid()) :
                    autoId == BsonAutoId.Int32 ? new BsonValue((Int32)col.Sequence) :
                    autoId == BsonAutoId.Int64 ? new BsonValue(col.Sequence) : BsonValue.Null;
            }
            // create bubble in sequence number if _id is bigger than current sequence
            else if(autoId == BsonAutoId.Int32 || autoId == BsonAutoId.Int64)
            {
                var current = id.AsInt64;

                // if current id is bigger than sequence, jump sequence to this number. Other was, do not increse sequnce
                col.Sequence = current >= col.Sequence ? current : col.Sequence - 1;
            }

            // test if _id is a valid type
            if (id.IsNull || id.IsMinValue || id.IsMaxValue)
            {
                throw UltraLiteException.InvalidDataType("_id", id);
            }

            _log.Write(Logger.COMMAND, "insert document on '{0}' :: _id = {1}", col.CollectionName, id.RawValue);

            // serialize object
            var bytes = BsonWriter.Serialize(doc);

            // storage in data pages - returns dataBlock address
            var dataBlock = _data.Insert(col, bytes);

            // store id in a PK index [0 array]
            var pk = _indexer.AddNode(col.PK, id, null);

            // do link between index <-> data block
            pk.DataBlock = dataBlock.Position;

            // for each index, insert new IndexNode
            foreach (var index in col.GetIndexes(false))
            {
                // for each index, get all keys (support now multi-key) - gets distinct values only
                // if index are unique, get single key only
                var expr = new BsonFields(index.Field);
                var keys = expr.Execute(doc, true);

                // do a loop with all keys (multi-key supported)
                foreach(var key in keys)
                {
                    // insert node
                    var node = _indexer.AddNode(index, key, pk);

                    // link my index node to data block address
                    node.DataBlock = dataBlock.Position;
                }
            }
        }
    }
}